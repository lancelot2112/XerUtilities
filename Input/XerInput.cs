using System;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XerUtilities.Input
{
    public class XerInput : GameComponent
    {
        XerKeyboard keyboard;
        XerMouse mouse;
        bool guiFocused;

        #region Properties
        public XerMouse Mouse { get { return mouse; } }
        public XerKeyboard Keyboard { get { return keyboard; } }
        public bool GUIFocused { get { return guiFocused; } set { guiFocused = value; } }
        #endregion


        public XerInput(Game game)
            : base(game)
        {
            this.keyboard = new XerKeyboard(game);
            this.mouse = new XerMouse(game);
        }

        public XerInput(Game game, string cursorPath)
            : base(game)
        {
            this.keyboard = new XerKeyboard(game);
            this.mouse = new XerMouse(game,cursorPath);
        }

        public override void  Update(GameTime gameTime)
        {
 	        keyboard.Update();
            mouse.Update();
            HandleInput();
        }


        public void HandleInput()
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.CurrentState.IsKeyDown(Keys.Escape))
            {
                Game.Exit();
            }

            if (Keyboard.F1JustPressed)
                mouse.ShowMouse = mouse.ShowMouse ? false : true;
        }
    }

    public class XerKeyboard
    {
        KeyboardState prevKeyboardState;
        KeyboardState currKeyboardState;

        public XerKeyboard(Game game)
        {
        }

        #region Properties
        //Keys
        public bool AJustPressed { get { return prevKeyboardState.IsKeyUp(Keys.A) && currKeyboardState.IsKeyDown(Keys.A); } }
        public bool APressedAndHeld { get { return currKeyboardState.IsKeyDown(Keys.A); } }
        public bool AReleased { get { return prevKeyboardState.IsKeyDown(Keys.A) && currKeyboardState.IsKeyUp(Keys.A); } }

        public bool SJustPressed { get { return prevKeyboardState.IsKeyUp(Keys.S) && currKeyboardState.IsKeyDown(Keys.S); } }
        public bool SPressedAndHeld { get { return currKeyboardState.IsKeyDown(Keys.S); } }
        public bool SReleased { get { return prevKeyboardState.IsKeyDown(Keys.S) && currKeyboardState.IsKeyUp(Keys.S); } }

        public bool WJustPressed { get { return prevKeyboardState.IsKeyUp(Keys.W) && currKeyboardState.IsKeyDown(Keys.W); } }
        public bool WPressedAndHeld { get { return currKeyboardState.IsKeyDown(Keys.W); } }
        public bool WReleased { get { return prevKeyboardState.IsKeyDown(Keys.W) && currKeyboardState.IsKeyUp(Keys.W); } }

        public bool DJustPressed { get { return prevKeyboardState.IsKeyUp(Keys.D) && currKeyboardState.IsKeyDown(Keys.D); } }
        public bool DPressedAndHeld { get { return currKeyboardState.IsKeyDown(Keys.D); } }
        public bool DReleased { get { return prevKeyboardState.IsKeyDown(Keys.D) && currKeyboardState.IsKeyUp(Keys.D); } }

        public bool QJustPressed { get { return prevKeyboardState.IsKeyUp(Keys.Q) && currKeyboardState.IsKeyDown(Keys.Q); } }
        public bool QPressedAndHeld { get { return currKeyboardState.IsKeyDown(Keys.Q); } }
        public bool QReleased { get { return prevKeyboardState.IsKeyDown(Keys.Q) && currKeyboardState.IsKeyUp(Keys.Q); } }

        public bool EJustPressed { get { return prevKeyboardState.IsKeyUp(Keys.E) && currKeyboardState.IsKeyDown(Keys.E); } }
        public bool EPressedAndHeld { get { return currKeyboardState.IsKeyDown(Keys.E); } }
        public bool EReleased { get { return prevKeyboardState.IsKeyDown(Keys.E) && currKeyboardState.IsKeyUp(Keys.E); } }

        public bool CPressedAndHeld { get { return currKeyboardState.IsKeyDown(Keys.C); } }

        public bool XPressedAndHeld { get { return currKeyboardState.IsKeyDown(Keys.X); } }

        public bool ZPressedAndHeld { get { return currKeyboardState.IsKeyDown(Keys.Z); } }

        public bool UpPressedAndHeld { get { return currKeyboardState.IsKeyDown(Keys.Up); } }

        public bool DownPressedAndHeld { get { return currKeyboardState.IsKeyDown(Keys.Down); } }

        public bool LeftPressedAndHeld { get { return currKeyboardState.IsKeyDown(Keys.Left); } }

        public bool RightPressedAndHeld { get { return currKeyboardState.IsKeyDown(Keys.Right); } }

        public bool HomeJustPressed { get { return prevKeyboardState.IsKeyUp(Keys.Home) && currKeyboardState.IsKeyDown(Keys.Home); } }

        public bool ShiftPressed { get { return currKeyboardState.IsKeyDown(Keys.LeftShift) || currKeyboardState.IsKeyDown(Keys.RightShift); } }

        public bool SpaceJustPressed { get { return prevKeyboardState.IsKeyUp(Keys.Space) && currKeyboardState.IsKeyDown(Keys.Space); } }
        public bool SpacePressedAndHeld { get { return currKeyboardState.IsKeyDown(Keys.Space); } }
        public bool SpaceReleased { get { return prevKeyboardState.IsKeyDown(Keys.Space) && currKeyboardState.IsKeyUp(Keys.Space); } }

        public bool F1JustPressed { get { return prevKeyboardState.IsKeyUp(Keys.F1) && currKeyboardState.IsKeyDown(Keys.F1); } }

        //States
        public KeyboardState PreviousState { get { return prevKeyboardState; } }
        public KeyboardState CurrentState { get { return currKeyboardState; } }
        #endregion

        public void Update()
        {
            prevKeyboardState = currKeyboardState;
            currKeyboardState = Keyboard.GetState();
        }
    }

    public class XerMouse : DrawableGameComponent
    {


        #region Graphics
        string cursorPath;
        Texture2D cursor;
        SpriteBatch spriteBatch;

        #endregion

        #region Fields
        int centerX = 0;
        int centerY = 0;

        MouseState prevMouseState;
        MouseState currMouseState;

        bool showMouse;
        public bool ShowMouse
        {
            get { return showMouse; }
            set
            {
                showMouse = value;
                if (cursor == null || spriteBatch == null)
                {
                    Game.IsMouseVisible = value;
                }
            }
        }
        #endregion

        #region Initialization
        public XerMouse(Game game)
            :base(game)
        {
            showMouse = false;
            this.centerX = game.GraphicsDevice.Viewport.Width / 2;
            this.centerY = game.GraphicsDevice.Viewport.Height / 2;
        }

        public XerMouse(Game game,string cursorPath)
            : base(game)
        {
            showMouse = true;
            this.cursorPath = cursorPath;
            game.Components.Add(this);
            this.centerX = game.GraphicsDevice.Viewport.Width / 2;
            this.centerY = game.GraphicsDevice.Viewport.Height / 2;
        }
        #endregion

        #region Properties
        public Vector2 ScreenCenter
        {
            get { return new Vector2(centerX, centerY); }
        }
        public Vector2 Position
        {
            get { return new Vector2(X, Y); }
        }
        public Vector2 DeltaPosition
        {
            get { return new Vector2(DeltaX, DeltaY); }
        }
        public float DeltaX
        {
            get
            {
                if (!showMouse) return currMouseState.X - centerX;
                else return currMouseState.X-prevMouseState.X;
            }
        }
        public float DeltaY
        {
            get
            {
                if (!showMouse) return currMouseState.Y - centerY;
                else return currMouseState.Y-prevMouseState.Y;
            }
        }
        public float X { get { return currMouseState.X; } }
        public float Y { get { return currMouseState.Y; } }

        //Right Button States
        public bool RightButtonJustPressed
        {
            get { return prevMouseState.RightButton == ButtonState.Released && currMouseState.RightButton == ButtonState.Pressed; }
        }
        public bool RightButtonPressedAndHeld
        {
            get { return currMouseState.RightButton == ButtonState.Pressed; }
        }
        public bool RightButtonReleased
        {
            get { return prevMouseState.RightButton == ButtonState.Pressed && currMouseState.RightButton == ButtonState.Released; }
        }
        //Center Button States
        public bool CenterButtonJustPressed
        {
            get { return prevMouseState.MiddleButton == ButtonState.Released && currMouseState.MiddleButton == ButtonState.Pressed; }
        }
        public bool CenterButtonPressedAndHeld
        {
            get { return currMouseState.MiddleButton == ButtonState.Pressed; }
        }
        public bool CenterButtonReleased
        {
            get { return prevMouseState.MiddleButton == ButtonState.Pressed && currMouseState.MiddleButton == ButtonState.Released; }
        }
        //Left Button States
        public bool LeftButtonJustPressed
        {
            get { return prevMouseState.LeftButton == ButtonState.Released && currMouseState.LeftButton == ButtonState.Pressed; }
        }
        public bool LeftButtonPressedAndHeld
        {
            get { return currMouseState.LeftButton == ButtonState.Pressed; }
        }
        public bool LeftButtonReleased
        {
            get { return prevMouseState.LeftButton == ButtonState.Pressed && currMouseState.LeftButton == ButtonState.Released; }
        }
        //Scroll Wheel States
        public float DeltaScroll
        {
            get { return currMouseState.ScrollWheelValue - prevMouseState.ScrollWheelValue; }
        }
        public float TotalScroll
        {
            get { return currMouseState.ScrollWheelValue; }
        }
        //Total Mouse States
        public MouseState PreviousState { get { return prevMouseState; } }
        public MouseState CurrentState { get { return currMouseState; } }
        #endregion

        #region Update Methods
        public void Update()
        {
            prevMouseState = currMouseState;
            currMouseState = Mouse.GetState();

            if (Game.IsActive && !showMouse) CenterMouse();
        }
        public void CenterMouse()
        {
            Mouse.SetPosition(centerX, centerY);
        }
        #endregion

        #region DrawableGameComponent
        protected override void LoadContent()
        {
            cursor = Game.Content.Load<Texture2D>(cursorPath);
            spriteBatch = Game.Services.GetService(typeof(SpriteBatch)) as SpriteBatch;
            if (spriteBatch == null) throw new NullReferenceException("spriteBatch");
        }

        protected override void UnloadContent()
        {
            if (cursor != null)
            {
                cursor.Dispose();
                cursor = null;
            }
            spriteBatch = null;
            base.UnloadContent();
        }
        public override void Draw(GameTime gameTime)
        {
            if (showMouse)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(cursor, new Vector2(currMouseState.X, currMouseState.Y), Color.White);
                spriteBatch.End();
            }
        }

        #endregion

    }

}
