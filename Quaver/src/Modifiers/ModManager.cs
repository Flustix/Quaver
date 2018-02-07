﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Audio;
using Quaver.Logging;
using Quaver.Modifiers.Mods;

namespace Quaver.Modifiers
{
    /// <summary>
    ///     Entire class that controls the addition and removal of game mods.
    /// </summary>
    internal class ModManager
    {
        /// <summary>
        ///     Adds a mod to our list, getting rid of any incompatible mods that are currently in there.
        ///     Also, specifying a speed, if need-be. That is only "required" if passing in ModIdentifier.Speed
        /// </summary>
        public static void AddMod(ModIdentifier modIdentifier)
        {
            IMod mod;

            // Set the newMod based on the ModType that is coming in
            switch (modIdentifier)
            {
                case ModIdentifier.Speed05X:                  
                case ModIdentifier.Speed06X:
                case ModIdentifier.Speed07X:
                case ModIdentifier.Speed08X:
                case ModIdentifier.Speed09X:
                case ModIdentifier.Speed11X:
                case ModIdentifier.Speed12X:
                case ModIdentifier.Speed13X:
                case ModIdentifier.Speed14X:
                case ModIdentifier.Speed15X:
                case ModIdentifier.Speed16X:
                case ModIdentifier.Speed17X:
                case ModIdentifier.Speed18X:
                case ModIdentifier.Speed19X:
                case ModIdentifier.Speed20X:
                    mod = new Speed(modIdentifier);
                    break;
                case ModIdentifier.NoSliderVelocity:
                    mod = new NoSliderVelocities();
                    break;
                case ModIdentifier.Strict:
                    mod = new Strict();
                    break;
                case ModIdentifier.Chill:
                    mod = new Chill();
                    break;
                default:
                    return;
            }

            // Check if any incompatible mods are already in our current game modifiers, and remove them if that is the case.
            var incompatibleMods = GameBase.CurrentGameModifiers.FindAll(x => x.IncompatibleMods.Contains(mod.ModIdentifier));
            incompatibleMods.ForEach(x => RemoveMod(x.ModIdentifier));

            // Add The Mod
            GameBase.CurrentGameModifiers.Add(mod);

            // Initialize the mod and set its score multiplier.
            GameBase.ScoreMultiplier += mod.ScoreMultiplierAddition;
            mod.InitializeMod();  
            
            Logger.LogSuccess($"Added Mod: {mod.ModIdentifier} and removed all incompatible mods.", LogType.Runtime);
            Logger.LogInfo($"Current Mods: {string.Join(", ", GameBase.CurrentGameModifiers.Select(x => x.ToString()))}", LogType.Runtime);
        }

        /// <summary>
        ///     Removes a mod from our GameBase
        /// </summary>
        public static void RemoveMod(ModIdentifier modIdentifier)
        {
            try
            {
                // Try to find the removed mod in the list
                var removedMod = GameBase.CurrentGameModifiers.Find(x => x.ModIdentifier == modIdentifier);

                // Remove The Mod's score multiplier
                GameBase.ScoreMultiplier -= removedMod.ScoreMultiplierAddition;
   
                // Remove the Mod
                GameBase.CurrentGameModifiers.Remove(removedMod);
                Logger.LogSuccess($"Removed {modIdentifier} from the current game modifiers.", LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Checks if a mod is currently activated.
        /// </summary>
        /// <param name="modIdentifier"></param>
        /// <returns></returns>
        public static bool Activated(ModIdentifier modIdentifier)
        {
            return GameBase.CurrentGameModifiers.Exists(x => x.ModIdentifier == modIdentifier);
        }

        /// <summary>
        ///     Removes all items from our list of mods
        /// </summary>
        public static void RemoveAllMods()
        {
            GameBase.CurrentGameModifiers.Clear();

            // Reset all GameBase variables to its defaults
            GameBase.ScoreMultiplier = 1.0f;
            GameBase.GameClock = 1.0f;
        }

        /// <summary>
        ///     Removes any speed mods from the game and resets the clock
        /// </summary>
        public static void RemoveSpeedMods()
        {
            try
            {
                GameBase.CurrentGameModifiers.RemoveAll(x => x.Type == ModType.Speed);
                GameBase.GameClock = 1.0f;
                SongManager.ChangeSongSpeed();

                Logger.LogSuccess($"Removed Speed Mods from the current game modifiers.", LogType.Runtime);
                Logger.LogInfo($"Current Mods: {string.Join(", ", GameBase.CurrentGameModifiers.Select(x => x.ToString()))}", LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Makes sure that the speed mod selected matches up with the game clock and sets the correct one.
        /// </summary>
        public static void CheckModInconsistencies()
        {
            var mod = GameBase.CurrentGameModifiers.Find(x => x.Type == ModType.Speed);

            // Re-intialize the correct mod.
            var index = GameBase.CurrentGameModifiers.IndexOf(mod);

            if (index != -1)
                GameBase.CurrentGameModifiers[index] = new Speed(mod.ModIdentifier);
            else
                GameBase.GameClock = 1.0f;
        }
    }
}
