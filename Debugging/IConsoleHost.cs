using System;
using System.Collections.Generic;
using System.Linq;

namespace XerUtilities.Debugging
{
    /// <summary>
    /// Types of messages issued by the console window.
    /// </summary>
    public enum ConsoleMessage
    {
        Standard = 1,
        Warning = 2,
        Error = 3
    }

    /// <summary>
    /// Command execution delegate.
    /// </summary>
    /// <param name="host">Host executing command.</param>
    /// <param name="command">Command name.</param>
    /// <param name="arguments">Command arguments.</param>
    public delegate void CommandAction(IConsoleHost host, string commandTag,IList<string> arguments);

    public interface ICommandExecutioner
    {
        /// <summary>
        /// Executes command identified by the command tag supplied.
        /// </summary>
        /// <param name="commandTag">Tag identifying the specific command.</param>
        void ExecuteCommand(string commandTag);
    }

    public interface IScriptEngine
    {
        void AddObject(string name, object obj);

    }

    public interface IEchoListener
    {
        /// <summary>
        /// Output message.
        /// </summary>
        /// <param name="messageType">Type of message.</param>
        /// <param name="text">Message to output.</param>
        void Echo(ConsoleMessage messageType, string text);
    }

    public interface IConsoleHost:ICommandExecutioner,IEchoListener ,IScriptEngine
    {
        /// <summary>
        /// Register new command by command tag for use in IConsoleHost
        /// </summary>
        /// <param name="commandTag">Tag to use when calling command in host.</param>
        /// <param name="description">Description of command.</param>
        /// <param name="callback">Function pointer holding the action to perform.</param>
        void RegisterCommand(string commandTag, string description, CommandAction callback);

        /// <summary>
        /// Unregisters command tag.
        /// </summary>
        /// <param name="commandTag">Tag that was used when calling command in host.</param>
        void UnregisterCommand(string commandTag);

        /// <summary>
        /// Output standard message.
        /// </summary>
        /// <param name="message">Text to be output.</param>
        void Echo(string message);

        /// <summary>
        /// Output warning message.
        /// </summary>
        /// <param name="message">Text to be output.</param>
        void EchoWarning(string message);

        /// <summary>
        /// Output error message.
        /// </summary>
        /// <param name="message">Text to be output.</param>
        void EchoError(string message);

        /// <summary>
        /// Register a listener that wants to also echo messages.
        /// </summary>
        /// <param name="listener"></param>
        void RegisterEchoListener(IEchoListener listener);

        /// <summary>
        /// Unregister listeners.
        /// </summary>
        /// <param name="listener"></param>
        void UnregisterEchoListener(IEchoListener listener);

        /// <summary>
        /// Add executioner.
        /// </summary>
        /// <param name="executioner"></param>
        void PushExecutioner(ICommandExecutioner executioner);

        /// <summary>
        /// Remove executioner.
        /// </summary>
        void PopExecutioner();
    }
}
