﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Sprites;
using Quaver.Main;
using Quaver.States.Gameplay;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Replays;
using Quaver.Audio;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Database.Scores;
using Quaver.Discord;
using Quaver.Graphics.Text;
using Quaver.Graphics.UI;
using Quaver.Helpers;
using Quaver.Logging;
using Quaver.States.Gameplay.Replays;
using Quaver.States.Results.UI;
using Quaver.States.Select;
using AudioEngine = Quaver.Audio.AudioEngine;
using Color = Microsoft.Xna.Framework.Color;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Quaver.States.Results
{
    internal class ResultsScreen : IGameState
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public State CurrentState { get; set; } = State.Results;

        /// <summary>
        ///     The type of results screen.
        /// </summary>
        internal ResultsScreenType Type { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool UpdateReady { get; set; }

        /// <summary>
        ///     Reference to the gameplay screen that was just played.
        /// </summary>
        internal GameplayScreen GameplayScreen { get; }

        /// <summary>
        ///     All the UI elements.
        /// </summary>
        private ResultsInterface UI { get; set; }

        /// <summary>
        ///     The .qua that this is results screen is referencing to.
        /// </summary>
        internal Qua Qua { get; private set; }

        /// <summary>
        ///     Applause sound effect.
        /// </summary>
        private SoundEffectInstance ApplauseSound { get; set; }

        /// <summary>
        ///     Song title + Difficulty name.
        /// </summary>
        private string SongTitle => $"{Qua.Artist} - {Qua.Title} [{Qua.DifficultyName}]";

        /// <summary>
        ///     MD5 Hash of the map played.
        /// </summary>
        private string Md5 => GameplayScreen.MapHash;

        /// <summary>
        ///     The user's scroll speed.
        /// </summary>
        private int ScrollSpeed => Qua.Mode == GameMode.Keys4 ? ConfigManager.ScrollSpeed4K.Value : ConfigManager.ScrollSpeed7K.Value;

        /// <summary>
        ///     The replay that was just played.
        /// </summary>
        internal Replay Replay { get; private set; }

        /// <summary>
        ///     Score processor.
        /// </summary>
        internal ScoreProcessor ScoreProcessor { get; private set; }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="gameplay"></param>
        public ResultsScreen(GameplayScreen gameplay)
        {
            GameplayScreen = gameplay;
            Qua = GameplayScreen.Map;
            Type = ResultsScreenType.FromGameplay;
        }

        /// <summary>
        ///     When going to the results screen with just a replay.
        /// </summary>
        /// <param name="replay"></param>
        public ResultsScreen(Replay replay)
        {
            Replay = replay;
            ScoreProcessor = new ScoreProcessorKeys(Replay);
            Type = ResultsScreenType.FromReplayFile;
        }

        /// <summary>
        ///     When loading up the results screen with a local score.
        /// </summary>
        /// <param name="score"></param>
        public ResultsScreen(LocalScore score)
        {
            GameBase.SelectedMap.Qua = GameBase.SelectedMap.LoadQua();
            Qua = GameBase.SelectedMap.Qua;

            var localPath = $"{ConfigManager.DataDirectory.Value}/r/{score.Id}.qr";

            // Try to find replay w/ local score id.
            // Otherwise we want to find
            if (File.Exists(localPath))
            {
                Replay = new Replay(localPath);
            }
            // Otherwise we want to create an "artificial" replay with the local score data..
            else
            {
                Replay = new Replay(score.Mode, score.Name, score.Mods, score.MapMd5)
                {
                    Date = Convert.ToDateTime(score.DateTime, CultureInfo.InvariantCulture),
                    Score = score.Score,
                    Accuracy = (float) score.Accuracy,
                    MaxCombo = score.MaxCombo,
                    CountMarv = score.CountMarv,
                    CountPerf = score.CountPerf,
                    CountGreat = score.CountGreat,
                    CountGood = score.CountGood,
                    CountOkay = score.CountOkay,
                    CountMiss = score.CountMiss
                };
            }

            ScoreProcessor = new ScoreProcessorKeys(Replay);
            Type = ResultsScreenType.FromLocalScore;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Initialize()
        {
            // Initialize the state depending on if we're coming from the gameplay screen
            // or loading up a replay file.
            switch (Type)
            {
                case ResultsScreenType.FromGameplay:
                    InitializeFromGameplay();
                    break;
                case ResultsScreenType.FromReplayFile:
                    InitializeFromReplayFile();
                    break;
                case ResultsScreenType.FromLocalScore:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Create the actual interface.
            UI = new ResultsInterface(this);
            UI.Initialize(this);

            UpdateReady = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <exception cref="!:NotImplementedException"></exception>
        public void UnloadContent() => UI.UnloadContent();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            GameBase.Navbar.PerformHideAnimation(dt);

            HandleInput();
            UI.Update(dt);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Draw()
        {
            GameBase.GraphicsDevice.Clear(Color.Black);
            UI.Draw();
        }

        /// <summary>
        ///     Changes discord rich presence to show results.
        /// </summary>
        private void ChangeDiscordPresence()
        {
            DiscordManager.Presence.Timestamps = null;

            // Don't change if we're loading in from a replay file.
            if (Type == ResultsScreenType.FromReplayFile || GameplayScreen.InReplayMode)
            {
                DiscordManager.Presence.Details = "Idle";
                DiscordManager.Presence.State = "In the menus";
                DiscordManager.Client.SetPresence(DiscordManager.Presence);
                return;
            }


            var state = GameplayScreen.Failed ? "Fail" : "Pass";
            var score = $"{ScoreProcessor.Score / 1000}k";
            var acc = $"{StringHelper.AccuracyToString(ScoreProcessor.Accuracy)}";
            var grade = GameplayScreen.Failed ? "F" : GradeHelper.GetGradeFromAccuracy(ScoreProcessor.Accuracy).ToString();
            var combo = $"{ScoreProcessor.MaxCombo}x";

            DiscordManager.Presence.State = $"{state}: {grade} {score} {acc} {combo}";
            DiscordManager.Client.SetPresence(DiscordManager.Presence);
        }

        /// <summary>
        ///     Plays the appluase sound effect.
        /// </summary>
        private void PlayApplauseEffect()
        {
            ApplauseSound = GameBase.Skin.SoundApplause.CreateInstance();

            if (!GameplayScreen.Failed && ScoreProcessor.Accuracy >= 80 && !GameplayScreen.InReplayMode)
                ApplauseSound.Play();
        }

        /// <summary>
        ///     Goes through the score submission process.
        /// </summary>
        private void SubmitScore()
        {
            // Don't save scores if the user quit themself.
            if (GameplayScreen.HasQuit || GameplayScreen.InReplayMode)
                return;

            // Run all of these tasks inside of a new thread to avoid blocks.
            var t = new Thread(() =>
            {
                SaveLocalScore();
#if DEBUG
                SaveDebugReplayData();
#endif
            });

            t.Start();
        }

        /// <summary>
        ///     Initializes the results screen if we're coming from the gameplay screen.
        /// </summary>
        private void InitializeFromGameplay()
        {
            // Keep the same replay and score processor if the user was watching a replay before.
            if (GameplayScreen.InReplayMode)
            {
                Replay = GameplayScreen.LoadedReplay;
                ScoreProcessor = Replay.Mods.HasFlag(ModIdentifier.Autoplay) ? GameplayScreen.Ruleset.ScoreProcessor : new ScoreProcessorKeys(Replay);
            }
            // Otherwise the replay and processor should be the one that the user just played.
            else
            {
                // Populate the replay with values from the score processor.
                Replay = GameplayScreen.ReplayCapturer.Replay;
                ScoreProcessor = GameplayScreen.Ruleset.ScoreProcessor;

                Replay.FromScoreProcessor(ScoreProcessor);
            }

            ChangeDiscordPresence();
            PlayApplauseEffect();

            // Submit score
            SubmitScore();
        }

        /// <summary>
        ///     Initialize the screen if we're coming from a replay file.
        /// </summary>
        private void InitializeFromReplayFile()
        {
            var mapset = GameBase.Mapsets.FirstOrDefault(x => x.Maps.Any(y => y.Md5Checksum == Replay.MapMd5));

            // Send the user back to the song select screen with an error if there was no found mapset.
            if (mapset == null)
            {
                Logger.LogError($"You do not have the map that this replay is for", LogType.Runtime);
                GameBase.GameStateManager.ChangeState(new SongSelectState());
                return;
            }

            // Find the map that actually has the correct hash.
            var map = mapset.Maps.Find(x => x.Md5Checksum == Replay.MapMd5);
            Map.ChangeSelected(map);

            // Load up the .qua file and change the selected map's Qua.
            Qua = map.LoadQua();
            GameBase.SelectedMap.Qua = Qua;

            // Make sure the background is loaded, we don't run this async because we
            // want it to be loaded when the user starts the map.
            BackgroundManager.LoadBackground();
            BackgroundManager.Change(GameBase.CurrentBackground);

            // Reload and play song.
            try
            {
                GameBase.AudioEngine.ReloadStream();
                GameBase.AudioEngine.Play();
            }
            catch (AudioEngineException e)
            {
                // No need to handle here.
            }
        }

        /// <summary>
        ///     Saves a local score to the database.
        /// </summary>
        private void SaveLocalScore()
        {
            Task.Run(async () =>
            {
                var scoreId = 0;
                try
                {
                    var localScore = LocalScore.FromScoreProcessor(ScoreProcessor, Md5, ConfigManager.Username.Value, ScrollSpeed);
                    scoreId = await LocalScoreCache.InsertScoreIntoDatabase(localScore);
                }
                catch (Exception e)
                {
                    Logger.LogError($"There was a fatal error when saving the local score!" + e.Message, LogType.Runtime);
                }

                try
                {
                    Replay.Write($"{ConfigManager.DataDirectory}/r/{scoreId}.qr");
                }
                catch (Exception e)
                {
                    Logger.LogError($"There was an error when writing the replay: " + e, LogType.Runtime);
                }
            });
        }

        /// <summary>
        ///     Saves replay data related to debugging.
        /// </summary>
        private void SaveDebugReplayData()
        {
            // Save debug replay and hit stat data.
            Task.Run(() =>
            {
                try
                {
                    File.WriteAllText($"{ConfigManager.DataDirectory.Value}/replay_debug.txt", Replay.FramesToString(true));

                    var hitStats = "";
                    GameplayScreen.Ruleset.ScoreProcessor.Stats.ForEach(x => hitStats += $"{x.ToString()}\r\n");
                    File.WriteAllText($"{ConfigManager.DataDirectory.Value}/replay_debug_hitstats.txt", hitStats);
                }
                catch (Exception e)
                {
                    Logger.LogError($"There was an error when writing debug replay files: {e}", LogType.Runtime);
                }
            });
        }

        /// <summary>
        ///     Handles input for the entire screen.
        /// </summary>
        private void HandleInput()
        {
            if (InputHelper.IsUniqueKeyPress(Keys.F2))
                ExportReplay();

            if (InputHelper.IsUniqueKeyPress(Keys.Escape))
                GameBase.GameStateManager.ChangeState(new SongSelectState());
        }

        /// <summary>
        ///     Exports the currently looked at replay.
        /// </summary>
        private void ExportReplay()
        {
            if (!Replay.HasData)
            {
                Logger.LogError($"Replay doesn't have any data", LogType.Runtime);
                return;
            }

            if (Replay.Mods.HasFlag(ModIdentifier.Autoplay))
            {
                Logger.LogError($"Exporting autoplay replays is disabled", LogType.Runtime);
                return;
            }

            Logger.LogImportant($"Just a second... We're exporting your replay!", LogType.Network, 2.0f);

            Task.Run(() =>
            {
                var path = $@"{ConfigManager.ReplayDirectory.Value}/{Replay.PlayerName} - {SongTitle} - {DateTime.Now:yyyyddMMhhmmss}{GameBase.GameTime.ElapsedMilliseconds}.qr";
                Replay.Write(path);

                // Open containing folder
                Process.Start("explorer.exe", "/select, \"" + path.Replace("/", "\\") + "\"");

                Logger.LogSuccess($"Replay successfully exported", LogType.Runtime);
            });
        }
    }
}