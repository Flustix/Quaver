﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Quaver.API.Enums;
using Quaver.API.Replays;
using Quaver.States.Gameplay.Replays;

namespace Quaver.States.Gameplay.GameModes.Keys.Input
{
    internal class ReplayInputManagerKeys
    {
        /// <summary>
        ///     Reference to the actual gameplay screen.
        /// </summary>
        private GameplayScreen Screen { get; }
        
        /// <summary>
        ///     The replay that is currently loaded.
        /// </summary>
        internal Replay Replay { get; }

        /// <summary>
        ///     The frame that we are currently on in the replay.
        /// </summary>
        internal int CurrentFrame { get; private set; } = 1;

        /// <summary>
        ///     If there are unique key presses in the current frame, per lane.
        /// </summary>
        internal List<bool> UniquePresses { get; } = new List<bool>();
        
        /// <summary>
        ///     If there are unique key releases in the current frame, per lane.
        /// </summary>
        internal List<bool> UniqueReleases { get; } = new List<bool>();

        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="screen"></param>
        internal ReplayInputManagerKeys(GameplayScreen screen)
        {
            Screen = screen;
            Replay = Screen.LoadedReplay;
            
            // Populate unique key presses/releases.
            for (var i = 0; i < screen.Map.FindKeyCountFromMode(); i++)
            {
                UniquePresses.Add(false);
                UniqueReleases.Add(false);
            }
        }

        /// <summary>
        ///     Determines which frame we are on in the replay and sets if it has unique key presses/releases.
        /// </summary>
        internal void HandleInput()
        {
            if (CurrentFrame >= Replay.Frames.Count || !(Screen.Timing.CurrentTime >= Replay.Frames[CurrentFrame].Time)) 
                return;
          
            // Get active keys in both the current and previous frames.
            var previousActive = Replay.KeyPressStateToLanes(Replay.Frames[CurrentFrame - 1].Keys);
            var currentActive = Replay.KeyPressStateToLanes(Replay.Frames[CurrentFrame].Keys);

            foreach (var activeLane in currentActive)
            {
                if (!previousActive.Contains(activeLane))
                    UniquePresses[activeLane] = true;
            }
            
            foreach (var activeLane in previousActive)
            {
                if (!currentActive.Contains(activeLane))
                    UniqueReleases[activeLane] = true;
            }

            CurrentFrame++;
        }

        internal void HandleSkip()
        {
            // Find the next frame 
            var frame = Replay.Frames.FindLastIndex(x => x.Time <= Screen.Timing.CurrentTime);

            if (frame != -1)
                CurrentFrame = frame;
        }
    }
}