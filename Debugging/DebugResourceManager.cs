#region File Description
//-----------------------------------------------------------------------------
// DebugManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

#endregion

namespace XerUtilities.Debugging
{
    /// <summary>
    /// DebugResourceManager class that holds graphics resources for debug
    /// </summary>
    public class DebugResourceManager : IGameComponent
    {
        // the name of the font to load
        private string debugFont;

        #region Properties

        /// <summary>
        /// Gets a sprite batch for debug.
        /// </summary>
        public SpriteBatch SpriteBatch { get; private set; }

        /// <summary>
        /// Gets white texture.
        /// </summary>
        public Texture2D WhiteTexture { get; private set; }

        /// <summary>
        /// Gets SpriteFont for debug.
        /// </summary>
        public SpriteFont DebugFont { get; private set; }

        #endregion

        #region Initialize

        public DebugResourceManager(Game game, string debugFont)
        {
            // Added as a Service.
            game.Services.AddService(typeof(DebugResourceManager), this);
            Logger.Initialize(game);
            this.debugFont = debugFont;

            // Load debug content.
            SpriteBatch = new SpriteBatch(game.GraphicsDevice);

            DebugFont = game.Content.Load<SpriteFont>(debugFont);

            // Create white texture.
            WhiteTexture = new Texture2D(game.GraphicsDevice, 1, 1);
            Color[] whitePixels = new Color[] { Color.White };
            WhiteTexture.SetData<Color>(whitePixels);
        }

        public void Initialize()
        {
        }


        #endregion
    }
}