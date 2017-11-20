﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Logging;

namespace Quaver.Modifiers.Mods
{
    internal class Speed09X : IMod
    {
        /// <summary>
        ///     The name of the mod.
        /// </summary>
        public string Name { get; set; } = "0.9x";

        /// <summary>
        ///     The identifier of the mod.
        /// </summary>
        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.Speed09X;

        /// <summary>
        ///     The type of mod as defined in the enum
        /// </summary>
        public ModType Type { get; set; } = ModType.Speed;

        /// <summary>
        ///     The description of the Mod
        /// </summary>
        public string Description { get; set; } = "Sets the song's speed to 0.9x";

        /// <summary>
        ///     Is the mod ranked?
        /// </summary>
        public bool Ranked { get; set; } = false;

        /// <summary>
        ///     The addition to the score multiplier this mod will have
        /// </summary>
        public float ScoreMultiplierAddition { get; set; } = 0;

        /// <summary>
        ///     The identifier of mods that are incompatible with this one.
        /// </summary>
        public ModIdentifier[] IncompatibleMods { get; set; } =
        {
            ModIdentifier.Speed05X, ModIdentifier.Speed06X, ModIdentifier.Speed07X, ModIdentifier.Speed08X,
            ModIdentifier.Speed11X, ModIdentifier.Speed12X, ModIdentifier.Speed13X, ModIdentifier.Speed14X, ModIdentifier.Speed15X,
            ModIdentifier.Speed16X, ModIdentifier.Speed17X, ModIdentifier.Speed18X, ModIdentifier.Speed19X, ModIdentifier.Speed20X
        };

        /// <summary>
        ///     All the mod logic should go here, setting unique variables. NEVER call this directly. Always use
        ///     ModManager.AddMod();
        /// </summary>
        public void InitializeMod()
        {
            GameBase.GameClock = 0.9f;
            Logger.Log($"Speed is now set to {GameBase.GameClock}x", Color.Cyan);

            // Change the song's speed
            if (GameBase.SelectedBeatmap.Song != null)
                GameBase.SelectedBeatmap.Song.ChangeSongSpeed();
        }
    }
}