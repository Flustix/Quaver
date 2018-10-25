using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Audio;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Logging;
using Wobble.Audio;

namespace Quaver.Screens.Gameplay
{
    public class GameplayAudioTiming
    {
        /// <summary>
        ///     Reference to the gameplay screen itself.
        /// </summary>
        private GameplayScreen Screen { get; }

        /// <summary>
        ///     The amount of time it takes before the gameplay/song actually starts.
        /// </summary>
        public static int StartDelay { get; } = 3000;

        /// <summary>
        ///     The time in the audio/play.
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="screen"></param>
        public GameplayAudioTiming(GameplayScreen screen)
        {
            Screen = screen;

            // Reload the audio stream.
            try
            {
                AudioEngine.LoadCurrentTrack();
            }
            catch (AudioEngineException e)
            {
                Logger.LogError(e, LogType.Runtime);
            }

            // Set the base time to - the start delay.
            Time = -StartDelay * AudioEngine.Track.Rate;
        }

        /// <summary>
        ///     Updates the audio time of the track.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            // Don't bother updating if the game is paused or the user failed.
            if (Screen.IsPaused || Screen.Failed)
                return;

            // If they audio hasn't begun yet, start counting down until the beginning of the map.
            // This is to give a delay before the audio starts.
            if (Time < 0)
            {
                Time += gameTime.ElapsedGameTime.TotalMilliseconds * AudioEngine.Track.Rate;
                return;
            }

            // Play the track if the game hasn't started yet.
            if (!Screen.HasStarted)
            {
                try
                {
                    Screen.HasStarted = true;
                    AudioEngine.Track.Play();
                }
                catch (AudioEngineException)
                {
                    // ignored
                }
            }

            // If the audio track is playing, use that time.
            if (AudioEngine.Track.IsPlaying)
            {
                // Average out between delta time and audio time for smooth playback.
                AudioEngine.Track.CorrectTime(gameTime.ElapsedGameTime.TotalMilliseconds);
                Time = AudioEngine.Track.Time;
            }

            // Otherwise use deltatime to calculate the proposed time.
            else
            {
                Time += gameTime.ElapsedGameTime.TotalMilliseconds * AudioEngine.Track.Rate;
            }
        }
    }
}
