﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Threading;
using Microsoft.Xna.Framework;
using Quaver.Database.Scores;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Helpers;
using Quaver.Main;
using Quaver.States.Results;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.States.Select
{
    internal class ScoresDisplay : Container
    {
        /// <summary>
        ///     All the currently displayed local scores.
        /// </summary>
        internal List<LocalScore> Scores { get; private set; }

        /// <summary>
        ///     All of the current score displays.
        /// </summary>
        internal List<TextButton> Displays { get; private set; } = new List<TextButton>();

        /// <summary>
        ///     Updates the display with new scores.
        /// </summary>
        /// <param name="scores"></param>
        internal void UpdateDisplay(List<LocalScore> scores)
        {
            Scores = scores;           
            
            // Get rid of all other displays.
            Displays.ForEach(x =>
            {
                x.Destroy();
                x.Clicked = null;
            });

            Displays = new List<TextButton>();
            
            for (var i = 0; i < scores.Count && i < 8; i++)
            {
                var display = new TextButton(new Vector2(320, 75), "")
                {
                    Parent = this,
                    Alignment = Alignment.TopCenter,
                    Tint = Color.White,
                    PosY = i * 80 + 100,
                    PosX = -20,
                    Image = GameBase.Skin.ScoreboardOther
                };

                display.Clicked += (o, e) =>
                {
                    var localScore = Scores[Displays.IndexOf((TextButton) o)];
                    GameBase.GameStateManager.ChangeState(new ResultsScreen(localScore));
                };
                
                 
                // Create avatar
                var avatar = new Sprite()
                {
                    Parent = display,
                    Size = new UDim2D(display.SizeY, display.SizeY),
                    Alignment = Alignment.MidLeft,
                    Image = GameBase.QuaverUserInterface.UnknownAvatar,
                };
            
                // Create username text.
                var username = new SpriteText()
                {
                    Parent = display,
                    Font = QuaverFonts.AssistantRegular16,
                    Text = (i + 1) + ". " + scores[i].Name,
                    Alignment = Alignment.TopLeft,
                    Alpha = 1,
                    TextScale = 0.85f
                };

                // Set username position.
                var usernameTextSize = username.Font.MeasureString(username.Text);        
                username.PosX = avatar.SizeX + usernameTextSize.X * username.TextScale / 2f + 10;
                username.PosY = usernameTextSize.Y * username.TextScale / 2f - 2;

                var mods = new SpriteText()
                {
                    Parent = display,
                    Font = QuaverFonts.AssistantRegular16,
                    Text = Scores[i].Mods != 0 ? "+ " + Scores[i].Mods : "",
                    Alignment = Alignment.BotLeft,
                    Alpha = 1,
                    TextScale = 0.65f
                };
                
                
                // Set modsposition.
                var modsTextSize = mods.Font.MeasureString(mods.Text);        
                mods.PosX = avatar.SizeX + modsTextSize.X * mods.TextScale / 2f + 10;
                mods.PosY = -modsTextSize.Y * mods.TextScale / 2f - 2;
                
                // Create score text.
                var score = new SpriteText()
                {
                    Parent = display,
                    Font = QuaverFonts.AssistantRegular16,
                    Alignment = Alignment.TopLeft,
                    Text = scores[i].Score.ToString("N0"),
                    TextScale = 0.78f,
                    Alpha = 1
                };
            
                var scoreTextSize = score.Font.MeasureString(score.Text);
                score.PosX = avatar.SizeX + scoreTextSize.X * score.TextScale / 2f + 12;
                score.PosY = username.PosY + scoreTextSize.Y * score.TextScale / 2f + 12;
                
                // Create score text.
                var acc = new SpriteText()
                {
                    Parent = display,
                    Font = QuaverFonts.AssistantRegular16,
                    Alignment = Alignment.BotLeft,
                    Text = StringHelper.AccuracyToString((float)scores[i].Accuracy),
                    TextScale = 0.65f,
                    Alpha = 1
                };
            
                var accTextSize = acc.Font.MeasureString(acc.Text);
                acc.PosX = avatar.SizeX + accTextSize.X * acc.TextScale / 2f + 12;
                acc.PosY = acc.PosY -accTextSize.Y * acc.TextScale / 2f - 18;
                
                // Create score text.
                var maxCombo = new SpriteText()
                {
                    Parent = display,
                    Font = QuaverFonts.AssistantRegular16,
                    Alignment = Alignment.BotRight,
                    Text = $"{scores[i].MaxCombo:N0}x",
                    TextScale = 0.78f,
                    Alpha = 1
                };
                
                var comboTextSize = maxCombo.Font.MeasureString(maxCombo.Text);
                maxCombo.PosX = -comboTextSize.X * maxCombo.TextScale / 2f - 8;
                maxCombo.PosY = -comboTextSize.Y / 2f;
                
                // Create score text.
                var ma = new SpriteText()
                {
                    Parent = display,
                    Font = QuaverFonts.AssistantRegular16,
                    Alignment = Alignment.MidRight,
                    Text = $"{Scores[i].CountMarv}/{Scores[i].CountPerf}/{Scores[i].CountGreat}/{Scores[i].CountGood}/{Scores[i].CountOkay}/{Scores[i].CountMiss}",
                    TextScale = 0.72f,
                    Alpha = 1
                };
                
                var maTextSize = ma.Font.MeasureString(ma.Text);
                ma.PosX = -maTextSize.X * ma.TextScale / 2f - 8;
                ma.PosY = -maTextSize.Y / 2f + 10;

                var grade = new Sprite()
                {
                    Parent = display,
                    Image = GameBase.Skin.Grades[scores[i].Grade],
                    Alignment = Alignment.TopRight,
                    Size = new UDim2D(20, 20),
                    PosY = 2
                };
                Displays.Add(display);
            }
        }
    }
}