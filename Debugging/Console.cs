using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XerUtilities.Input;
using XerUtilities.Scripting;
using System.IO;


namespace XerUtilities.Debugging
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>    
    public class Console : DrawableGameComponent, IConsoleHost
    {
        #region Constants
        /// <summary>
        /// Maximum lines that shows in Console
        /// </summary>
        const int MaxLineCount = 20;

        /// <summary>
        /// Maximum commands stored in history
        /// </summary>
        const int MaxCommandHistory = 32;

        /// <summary>
        /// Cursor used to display current position
        /// </summary>
        const char Cursor = '_';

        /// <summary>
        /// Default prompt indicating current line
        /// </summary>
        const string DefaultPrompt = ">";
        #endregion

        #region Properties
        /// <summary>
        /// Prompt currently in use for indicating current line
        /// </summary>
        public string Prompt { get; set; }

        /// <summary>
        /// True when console is accepting input
        /// </summary>
        public bool Focused { get { return state != State.Closed; } }
        #endregion

        #region Fields

        // Command window states
        enum State
        {
            Closed,
            Opening,
            Opened,
            Closing
        }

        /// <summary>
        /// class CommandInfo contains information used in describing and executing a build in command
        /// </summary>
        class CommandInfo
        {
            public CommandInfo(string commandTag, string description, CommandAction action)
            {
                this.Command = commandTag;
                this.Description = description;
                this.Callback = action;
            }

            // command tag
            public string Command;

            // Description of command.
            public string Description;

            // Delegate for executing the commmand.
            public CommandAction Callback;
        }

        // Current state
        private State state = State.Closed;

        // Helps to handle state transitions.
        private float stateTransition;

        // Registered echo listeners.
        List<IEchoListener> listeners = new List<IEchoListener>();

        // Stack of command executioners.
        Stack<ICommandExecutioner> executioners = new Stack<ICommandExecutioner>();

        // Registered commands.
        private Dictionary<string,CommandInfo> commandTable = new Dictionary<string,CommandInfo>();

        // Current command line string and cursor position.
        private StringBuilder commandLine = new StringBuilder();
        private int cursorIndex = 0;

        private int numOfLines = 0;
        private int linesLastIndex = 0;
        private int currentScrollValue = 0;
        private int renderStartIndex = 0;
        private int maxLinesInMemory = 20000;
        private string[] lines;

        // Command history buffer.
        private List<string> commandHistory = new List<string>();

        // Selecting command history index.
        private int commandHistoryIndex;

        #region Keyboard Input Variables
        // Previous fram keyboard state.
        private KeyboardState prevKeyState;

        // Key that pressed last frame.
        private Keys pressedKey;

        // Timer for key repeating.
        private float keyRepeatTimer;

        // Key repeat duration in seconds for the first key press.
        private float keyRepeatStartDuration = 0.5f;

        // Key repeat duration for continuous pressing.
        private float keyRepeatDuration = 0.05f;
  
        // Cursor blink timer duration
        private float cursorBlinkDuration = 0.25f;

        // Timer for cursor blinking.
        private float cursorBlinkTimer;
        #endregion

        #region Python
        private bool usingPython;
        private IronPythonHost pyHost;
        #endregion

        DebugResourceManager debugResources;

        #endregion

        #region Initialization
        public Console(Game game,DebugResourceManager resources, bool isUsingPython)
            : base(game)
        {
            // Set defaults.
            Prompt = DefaultPrompt + " ";

            // Set manager
            debugResources = resources;

            // Add this instance as a service.
            Game.Services.AddService(typeof(IConsoleHost), this);

            // Draw the command UI on top of everything
            DrawOrder = int.MaxValue;

            //Initiate the array to hold the console view
            lines = new string[maxLinesInMemory];

            Echo(">>> Xna Console v0.5 <<<");
            Echo("");
            this.usingPython = isUsingPython;

            // Adding default commands
            // Help command displays registered command information.
            RegisterCommand("help", "Show command descriptions.",
                delegate(IConsoleHost host, string command, IList<string> args)
                {
                    int maxLen = 0;
                    foreach (CommandInfo cmd in commandTable.Values)
                        maxLen = Math.Max(maxLen, cmd.Command.Length);

                    string fmt = String.Format("{{0,-{0}}}    {{1}}", maxLen);

                    foreach (CommandInfo cmd in commandTable.Values)
                    {
                        Echo(String.Format(fmt, cmd.Command, cmd.Description));
                    }
                });

            // Clear screen command
            RegisterCommand("cls", "Clear console.",
                delegate(IConsoleHost host, string command, IList<string> args)
                {
                    Clear();
                });

            // Echo command
            RegisterCommand("echo", "Display messages.",
                delegate(IConsoleHost host, string command, IList<string> args)
                {
                    Echo(command.Substring(5));
                });

            if(usingPython)
            {
                pyHost = new IronPythonHost(this);

                // Directory command
                RegisterCommand("pyDir", "List loaded objects.",
                    delegate(IConsoleHost host, string command, IList<string> args)
                    {
                        pyHost.Directory();
                    });
            }
            
        }     

        #endregion

        #region IConsoleHost Implementation

        public void RegisterCommand(string commandTag, string description, CommandAction callback)
        {
            string lowerCommand = commandTag.ToLower();
            if (commandTable.ContainsKey(lowerCommand))
            {
                throw new InvalidOperationException(
                    String.Format("Command \"{0}\" is already registered.", commandTag));
            }

            commandTable.Add(lowerCommand, new CommandInfo(commandTag, description, callback));
        }

        public void UnregisterCommand(string commandTag)
        {
            string lowerCommand = commandTag.ToLower();
            if (!commandTable.ContainsKey(lowerCommand))
            {
                throw new InvalidOperationException(
                   String.Format("Command \"{0}\" is not registered.", commandTag));
            }

            commandTable.Remove(lowerCommand);
        }

        public void ExecuteCommand(string command)
        {
            // Call registered executioner.
            if (executioners.Count != 0)
            {
                executioners.Peek().ExecuteCommand(command);
                return;
            }

            // Run the command.
            char[] spaceChars = new char[] { ' ' };

            Echo(Prompt + command);

            command = command.TrimStart(spaceChars);

            List<string> args = new List<string>(command.Split(spaceChars));
            string cmdText = args[0];
            args.RemoveAt(0);

            CommandInfo cmd;
            if (commandTable.TryGetValue(cmdText.ToLower(), out cmd))
            {
                try
                {
                    // Call registered command delegate.
                    cmd.Callback(this, command, args);
                }
                catch (Exception e)
                {
                    // Exception occurred while running command.
                    EchoError("Unhandled Exception occurred");

                    string[] lines = e.Message.Split(new char[] { '\n' });
                    foreach (string line in lines)
                        EchoError(line);
                }
            }
            else
            {
                if (usingPython)
                {
                    try
                    {
                        pyHost.ExecutePythonCode(command);
                    }
                    catch (Exception ex)
                    {
                        EchoError(ex.Message);
                    }
                }
                else
                {
                    Echo("Unknown Command");
                }
            }

            // Add to command history.
            commandHistory.Add(command);
            while (commandHistory.Count > MaxCommandHistory)
                commandHistory.RemoveAt(0);

            commandHistoryIndex = commandHistory.Count;
        }

        public void RegisterEchoListener(IEchoListener listener)
        {
            listeners.Add(listener);
        }

        public void UnregisterEchoListener(IEchoListener listener)
        {
            listeners.Remove(listener);
        }

        public void Echo(ConsoleMessage messageType, string text)
        {
            string[] buffer = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            string[] substringBuffer;
            int ind = linesLastIndex % maxLinesInMemory;
            int drawWidth = Game.Window.ClientBounds.Width-10;
            int j;
            
            //Get the starting index to begin writing.  If we already have the maximum number of lines
            //in memory begin overwriting at the start index.

            for (int i = 0; i<buffer.Length; i++)
            {
                if (debugResources.DebugFont.MeasureString(buffer[i]).X > drawWidth)
                {
                    substringBuffer = WrapText(buffer[i], drawWidth);
                    for (j = 0; j < substringBuffer.Length; j++)
                    {
                        lines[ind] = substringBuffer[j];
                        linesLastIndex++;
                        ind = linesLastIndex % maxLinesInMemory;
                        numOfLines++;
                    }
                }
                else
                {
                    lines[ind] = buffer[i];
                    linesLastIndex++;
                    ind = linesLastIndex % maxLinesInMemory;
                    numOfLines++;
                }                
            }

            renderStartIndex = linesLastIndex - MaxLineCount;
            linesLastIndex %= maxLinesInMemory;            
            if (numOfLines >= maxLinesInMemory)
            {
                numOfLines = maxLinesInMemory;
                if (renderStartIndex < 0) renderStartIndex += maxLinesInMemory;
            }
            else if (renderStartIndex < 0)
            {
                renderStartIndex = 0;
            }

            // Call registered listeners.
            foreach (IEchoListener listner in listeners)
                listner.Echo(messageType, text);
        }

        public void Echo(string text)
        {
            Echo(ConsoleMessage.Standard, text);
        }

        public void EchoWarning(string text)
        {
            Echo(ConsoleMessage.Warning, text);
        }

        public void EchoError(string text)
        {
            Echo(ConsoleMessage.Error, text);
        }

        public void PushExecutioner(ICommandExecutioner executioner)
        {
            executioners.Push(executioner);
        }

        public void PopExecutioner()
        {
            executioners.Pop();
        }
        #endregion

        #region Basic Functions
        public void AddObject(string name, object obj)
        {
            IronPythonHost.SetVariable(name,obj);
        }
        public void Execute(string code)
        {
            pyHost.ExecutePythonCode(code);
        }
        public void Clear()
        {
            linesLastIndex = 0;
        }
        #endregion

        #region Update and Draw

        /// <summary>
        /// Show console.
        /// </summary>
        public void Show()
        {
            if (state == State.Closed)
            {
                stateTransition = 0.0f;
                state = State.Opening;
            }
        }

        /// <summary>
        /// Hide console.
        /// </summary>
        public void Hide()
        {
            if (state == State.Opened)
            {
                stateTransition = 1.0f;
                state = State.Closing;
            }
        }
        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            const float OpenSpeed = 5.0f;
            const float CloseSpeed = 5.0f;

            switch (state)
            {
                case State.Closed:
                    if (keyState.IsKeyDown(Keys.Tab))
                        Show();
                    break;
                case State.Opening:
                    stateTransition += dt * OpenSpeed;
                    if (stateTransition > 1.0f)
                    {
                        stateTransition = 1.0f;
                        state = State.Opened;
                    }
                    break;
                case State.Opened:
                    // if keys were pressed
                    if (ProcessKeyInputs(dt) || cursorBlinkTimer > 2 * cursorBlinkDuration)
                    {
                        cursorBlinkTimer = 0;
                    }
                    else
                    {
                        cursorBlinkTimer += dt;
                    }
                    break;
                case State.Closing:
                    stateTransition -= dt * CloseSpeed;
                    if (stateTransition < 0.0f)
                    {
                        stateTransition = 0.0f;
                        state = State.Closed;
                    }
                    break;
            }

            prevKeyState = keyState;

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            // Do nothing when command window is completely closed.
            if (state == State.Closed)
                return;


            // Compute command window size and draw.
            float windowWidth = GraphicsDevice.Viewport.Width;
            float windowHeight = GraphicsDevice.Viewport.Height;
            float consolePixelsFromTop = 0.0f;
            float consolePixelsFromLeft = 0.0f;

            Rectangle rect = new Rectangle();
            rect.X = (int)consolePixelsFromLeft;
            rect.Y = (int)consolePixelsFromTop;
            rect.Width = (int)(windowWidth);
            rect.Height = (int)(MaxLineCount * debugResources.DebugFont.LineSpacing);

            Matrix mtx = Matrix.CreateTranslation(
                        new Vector3(0, -rect.Height * (1.0f - stateTransition), 0));

            debugResources.SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, mtx);

            debugResources.SpriteBatch.Draw(debugResources.WhiteTexture, rect, new Color(0, 0, 0, 200));

            //Draw 


            // Draw each of the stored lines 3 pixels from the left of the console.
            Vector2 pos = new Vector2(consolePixelsFromLeft+=3, consolePixelsFromTop);

            //Get the number of lines to render
            int linesToRender = Math.Min(numOfLines, MaxLineCount);
            //Find which index the rendering should start based on the start index and scroll value
            int renderStart = renderStartIndex - currentScrollValue;
            if (renderStart < 0) renderStart += maxLinesInMemory;
            //Loop over values found and draw each string
            for (int i = renderStart; i < renderStart + linesToRender; i++)
            {
                debugResources.SpriteBatch.DrawString(debugResources.DebugFont, lines[i % maxLinesInMemory], pos, Color.White);
                pos.Y += debugResources.DebugFont.LineSpacing;
            }

            // Draw prompt string.
            // Get the string to be displayed.
            string leftPart = Prompt + commandLine.ToString();

            // Draw the display string to the window
            debugResources.SpriteBatch.DrawString(debugResources.DebugFont,
                String.Format("{0}{1}", Prompt, commandLine.ToString()), pos, Color.White);

            // Cursor blink timer is incremented in update method
            // while cursor blink timer is less than duration show cursor
            if (cursorBlinkTimer < cursorBlinkDuration)
            {
                // Get the substring of the display string before the cursor
                string subString = leftPart.Substring(0,leftPart.Length-(leftPart.Length-Prompt.Length - cursorIndex));
                // Find the length of that substring in pixels to determine the cursor position on screen
                Vector2 cursorPos = pos + debugResources.DebugFont.MeasureString(subString);
                // Set the y position of the cursor
                cursorPos.Y = pos.Y;           

                // Draw the cursor to the screen
                debugResources.SpriteBatch.DrawString(debugResources.DebugFont, new string(Cursor, 1), cursorPos, Color.White);

            }

            // End the sprite batch operation
            debugResources.SpriteBatch.End();
        }
        #endregion

        #region Keyboard Input Handlers
        public bool ProcessKeyInputs(float dt)
        {
            // Get current keyboard state along with all currently pressed keys.
            KeyboardState keyState = Keyboard.GetState();
            Keys[] keys = keyState.GetPressedKeys();

            // Determine if either shift is held down.
            bool shift = keyState.IsKeyDown(Keys.LeftShift) ||
                keyState.IsKeyDown(Keys.RightShift);

            // Enumerate through key array
            foreach (Keys key in keys)
            {
                // Go to next in loop because key is not being processed
                if (!IsKeyPressed(key, dt)) continue;

                char ch;
                if (XerKeyboardKeys.KeyToString(key, shift, out ch))
                {
                    // Handle typical character input.
                    commandLine = commandLine.Insert(cursorIndex, new string(ch, 1));
                    cursorIndex++;
                }
                else
                {
                    switch (key)
                    {
                        case Keys.Back:
                            if (cursorIndex > 0)
                                commandLine.Remove(--cursorIndex, 1);
                            break;
                        case Keys.Delete:
                            if (cursorIndex < commandLine.Length)
                                commandLine.Remove(cursorIndex, 1);
                            break;
                        case Keys.Left:
                            if (cursorIndex > 0)
                                cursorIndex--;
                            break;
                        case Keys.Right:
                            if (cursorIndex < commandLine.Length)
                                cursorIndex++;
                            break;
                        case Keys.Enter:
                            // Run the command.
                            ExecuteCommand(commandLine.ToString());
                            commandLine.Clear();
                            cursorIndex = 0;
                            break;
                        case Keys.Up:
                            // Show command history
                            if (commandHistory.Count > 0)
                            {
                                commandHistoryIndex = Math.Max(0, commandHistoryIndex - 1);
                                commandLine.Clear();
                                commandLine.Append(commandHistory[commandHistoryIndex]);
                                cursorIndex = commandLine.Length;
                            }
                            break;
                        case Keys.Down:
                            // Show command history
                            if (commandHistory.Count > 0)
                            {
                                commandHistoryIndex = Math.Min(commandHistory.Count - 1, commandHistoryIndex + 1);
                                commandLine.Clear();
                                commandLine.Append(commandHistory[commandHistoryIndex]);
                                cursorIndex = commandLine.Length;
                            }
                            break;
                        case Keys.Tab:
                            Hide();
                            break;
                        case Keys.Escape:
                            Hide();
                            break;
                        case Keys.PageUp:
                            currentScrollValue += MaxLineCount;
                            int val = Math.Max(numOfLines - MaxLineCount, 0);
                            if (currentScrollValue > val) currentScrollValue = val;
                            break;
                        case Keys.PageDown:
                            currentScrollValue -= MaxLineCount;
                            if (currentScrollValue < 0) currentScrollValue = 0;
                            break;
                    }//End switch
                }//End if
            }//End foreach

            return keys.Length > 0;

        }//End ProcessKeyInputs()

        bool IsKeyPressed(Keys key, float dt)
        {
            bool pressed = false;
            // Treat it as pressed if given key was not pressed in last frame
            if (prevKeyState.IsKeyUp(key))
            {
                keyRepeatTimer = keyRepeatStartDuration;
                pressedKey = key;
                pressed = true;
            }
            // Handling key repeating if given key was pressed in previous fram
            else if (key == pressedKey)
            {
                keyRepeatTimer -= dt;
                if (keyRepeatTimer <= 0.0f)
                {
                    keyRepeatTimer += keyRepeatDuration;
                    pressed = true;
                }
            }

            // Else return false
            return pressed;
        }

        private string[] WrapText(string text, int maxLineWidth)
        {
            string[] words = text.Split(' ');

            StringBuilder sb = new StringBuilder();

            float lineWidth = 0f;

            float spaceWidth = debugResources.DebugFont.MeasureString(" ").X;

            foreach (string word in words)
            {
                Vector2 size = debugResources.DebugFont.MeasureString(word);

                if (lineWidth + size.X < maxLineWidth)
                {
                    sb.Append(word + " ");
                    lineWidth += size.X + spaceWidth;
                }
                else
                {
                    sb.Append("\n" + word + " ");
                    lineWidth = size.X + spaceWidth;
                }
            }

            return sb.ToString().Split(new string[]{"\n"},StringSplitOptions.None);
        }
        #endregion

    }

}
