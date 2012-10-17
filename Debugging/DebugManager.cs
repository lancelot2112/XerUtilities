using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XerUtilities.Debugging
{
    public class DebugManager
    {
        private static DebugManager instance;

        /// <summary>
        /// Gets the singleton instance of the debug system. You must call Initialize
        /// to create the instance.
        /// </summary>
        public static DebugManager Instance
        {
            get { return instance; }
        }

        /// <summary>
        /// Gets the DebugManager for the system.
        /// </summary>
        public DebugResourceManager DebugResourceManager { get; private set; }

        /// <summary>
        /// Gets the DebugCommandUI for the system.
        /// </summary>
        public Console Console { get; private set; }

        /// <summary>
        /// Gets the FpsCounter for the system.
        /// </summary>
        public DebugDisplay DebugDisplay { get; private set; }

        /// <summary>
        /// Gets the TimeRuler for the system.
        /// </summary>
        public TimeRuler TimeRuler { get; private set; }

        /// <summary>
        /// Initializes the DebugSystem and adds all components to the game's Components collection.
        /// </summary>
        /// <param name="game">The game using the DebugSystem.</param>
        /// <param name="debugFont">The font to use by the DebugSystem.</param>
        /// <returns>The DebugSystem for the game to use.</returns>
        public DebugManager(Game game, string debugFont, bool luaConsole) 
        {
            instance = this;

            // Create all of the system components
            DebugResourceManager = new DebugResourceManager(game, debugFont);
            game.Components.Add(DebugResourceManager);

            Console = new Console(game, luaConsole);
            game.Components.Add(Console);

            DebugDisplay = new DebugDisplay(game);
            game.Components.Add(DebugDisplay);

            TimeRuler = new TimeRuler(game);
            game.Components.Add(TimeRuler);

            Logger.Initialize(game);
        }
    }
}
