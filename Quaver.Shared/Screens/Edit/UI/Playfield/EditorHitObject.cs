using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Shared.Skinning;
using Wobble.Audio.Tracks;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Edit.UI.Playfield
{
    public class EditorHitObject : Sprite
    {
        /// <summary>
        /// </summary>
        protected Qua Map { get; }

        /// <summary>
        /// </summary>
        protected EditorPlayfield Playfield { get; }

        /// <summary>
        /// </summary>
        protected HitObjectInfo Info { get; }

        /// <summary>
        /// </summary>
        protected Bindable<SkinStore> Skin { get; }

        /// <summary>
        /// </summary>
        protected IAudioTrack Track { get; }

        /// <summary>
        /// </summary>
        protected Bindable<bool> AnchorHitObjectsAtMidpoint { get; }

        /// <summary>
        /// </summary>
        protected Bindable<bool> ViewLayers { get; }

        /// <summary>
        /// </summary>
        protected SkinKeys SkinMode => Skin.Value.Keys[Map.Mode];

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="playfield"></param>
        /// <param name="info"></param>
        /// <param name="skin"></param>
        /// <param name="track"></param>
        /// <param name="anchorHitObjectsAtMidpoint"></param>
        /// <param name="viewLayers"></param>
        public EditorHitObject(Qua map, EditorPlayfield playfield, HitObjectInfo info, Bindable<SkinStore> skin, IAudioTrack track,
            Bindable<bool> anchorHitObjectsAtMidpoint, Bindable<bool> viewLayers)
        {
            Map = map;
            Playfield = playfield;
            Info = info;
            Skin = skin;
            Track = track;
            AnchorHitObjectsAtMidpoint = anchorHitObjectsAtMidpoint;
            ViewLayers = viewLayers;

            Image = GetHitObjectTexture();

            SetPosition();

            ViewLayers.ValueChanged += OnViewLayersChanged;
        }

        /// <inheritdoc />
        /// <summary>
        ///     When drawing the object, we only want to just draw it to SpriteBatch.
        ///     We don't want to handle anything else since its all manual in this purpose
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime) => DrawToSpriteBatch();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            ViewLayers.ValueChanged -= OnViewLayersChanged;
            base.Destroy();
        }

        /// <summary>
        ///     Sets the size of the object
        /// </summary>
        public virtual void SetSize()
        {
            Width = Playfield.ColumnSize - Playfield.BorderLeft.Width * 2;
            Height = (Playfield.ColumnSize - Playfield.BorderLeft.Width * 2) * Image.Height / Image.Width;
        }

        /// <summary>
        ///     Sets the position of the object
        /// </summary>
        public void SetPosition()
        {
            var x = Playfield.ScreenRectangle.X + Playfield.ColumnSize * (Info.Lane - 1) + Playfield.BorderLeft.Width;
            var y = Playfield.HitPositionY - Info.StartTime * Playfield.TrackSpeed - Height;

            if (AnchorHitObjectsAtMidpoint.Value)
                y += Height / 2f;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (X != x || Y != y)
                Position = new ScalableVector2(x, y);
        }

        /// <summary>
        ///    Returns the texture for the hitobject
        /// </summary>
        private Texture2D GetHitObjectTexture()
        {
            var index = SkinMode.ColorObjectsBySnapDistance ? HitObjectManager.GetBeatSnap(Info, Info.GetTimingPoint(Map.TimingPoints)) : 0;
            return ViewLayers.Value ? SkinMode.EditorLayerNoteHitObjects[Info.Lane - 1] : SkinMode.NoteHoldHitObjects[Info.Lane - 1][index];
        }

        /// <summary>
        ///     Checks if the object is visible and on the screen
        /// </summary>
        /// <returns></returns>
        public virtual bool IsOnScreen() => Info.StartTime * Playfield.TrackSpeed >= Playfield.TrackPositionY - Playfield.Height &&
                                                 Info.StartTime * Playfield.TrackSpeed <= Playfield.TrackPositionY + Playfield.Height;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnViewLayersChanged(object sender, BindableValueChangedEventArgs<bool> e)
        {
            Image = GetHitObjectTexture();
            Tint = GetNoteTint();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected Color GetNoteTint()
        {
            if (!ViewLayers.Value || Info.EditorLayer >= Map.EditorLayers.Count)
                return Color.White;

            var layer = Map.EditorLayers[Info.EditorLayer];
            return ColorHelper.ToXnaColor(layer.GetColor());
        }
    }
}