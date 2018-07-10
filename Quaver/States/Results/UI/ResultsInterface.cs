﻿using Quaver.GameState;
using Quaver.Graphics.Base;
using Quaver.Graphics.UI;
using Quaver.Main;

namespace Quaver.States.Results.UI
{
    internal class ResultsInterface : IGameStateComponent
    {
        /// <summary>
        ///     Reference to the parent screen.
        /// </summary>
        internal ResultsScreen Screen { get; }

        /// <summary>
        ///     Sprite container.
        /// </summary>
        private Container Container { get; set; }

        /// <summary>
        ///     The background of the map.
        /// </summary>
        private Background Background { get; set; }

        /// <summary>
        ///     Information about the map in the result sscreen.
        /// </summary>
        private MapInformation MapInformation { get; set; }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="screen"></param>
        internal ResultsInterface(ResultsScreen screen) => Screen = screen;

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="state"></param>
        public void Initialize(IGameState state)
        {
            Container = new Container();

            CreateBackground();
            CreateMapInformation();
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        public void UnloadContent() => Container.Destroy();

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="dt"></param>
        public void Update(double dt) => Container.Update(dt);

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        public void Draw()
        {
            GameBase.SpriteBatch.Begin();
            Container.Draw();
            GameBase.SpriteBatch.End();
        }

        /// <summary>
        ///     Creates the map background.
        /// </summary>
        private void CreateBackground() => Background = new Background(GameBase.QuaverUserInterface.MenuBackground, 40) {Parent = Container};

        /// <summary>
        ///     Creates the map information sprite.
        /// </summary>
        private void CreateMapInformation() => MapInformation = new MapInformation(Screen) {Parent = Container, PosY =  25};
    }
}