﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlasticMetal.MobileSuit.Core;
using PlasticMetal.MobileSuit.Logging;

namespace PlasticMetal.MobileSuit
{
    /// <summary>
    /// Providing status of current host.
    /// </summary>
    public interface IHostStatus
    {
        /// <summary>
        /// TraceBack of last Command.
        /// </summary>
        public TraceBack TraceBack{ get; }
        /// <summary>
        /// Return value of last Command.
        /// </summary>
        public object? ReturnValue { get; }
    }
    /// <summary>
    ///     A host of Mobile Suit, which may run commands.
    /// </summary>
    public interface IMobileSuitHost
    {
        /// <summary>
        /// Providing status of current host.
        /// </summary>
        public IHostStatus HostStatus { get; }

        /// <summary>
        ///     Basic Settings of this MobileSuitHost
        /// </summary>
        HostSettings Settings { get; set; }

        /// <summary>
        ///     Logger for current host
        /// </summary>
        public ISuitLogger Logger { get; }

        /// <summary>
        ///     IOServer for current host
        /// </summary>
        public IIOHub IO { get; }

        /// <summary>
        ///     Split a commandline string to args[] array.
        /// </summary>
        /// <param name="commandLine">commandline string</param>
        /// <returns>args[] array</returns>
        protected static string[]? SplitCommandLine(string commandLine)
        {
            if (string.IsNullOrEmpty(commandLine)) return null;
            string submit;
            var l = new List<string>();
            var separating = false;
            var separationPrefix = false;
            var separationCharacter = '"';
            var left = 0;
            var right = 0;
            for (; right < commandLine.Length; right++)
                switch (commandLine[right])
                {
                    case '"':
                        if (separationPrefix) continue;
                        if (separating && separationCharacter == '"')
                        {
                            l.Add(commandLine[left..right]);
                            left = right + 1;
                        }
                        else if (!separating)
                        {
                            separating = true;
                            separationCharacter = '"';
                            left = right + 1;
                        }

                        break;
                    case '\'':
                        if (separationPrefix) continue;
                        if (separating && separationCharacter == '\'')
                        {
                            l.Add(commandLine[left..right]);
                            left = right + 1;
                        }
                        else if (!separating)
                        {
                            separating = true;
                            separationCharacter = '\'';
                            left = right + 1;
                        }

                        break;
                    case ' ':
                        submit = commandLine[left..right];
                        if (!string.IsNullOrEmpty(submit))
                            l.Add(submit);
                        left = right + 1;
                        separationPrefix = false;
                        break;
                    default:
                        if (!separating) separationPrefix = true;
                        break;
                }

            submit = commandLine[left..right];
            if (!string.IsNullOrEmpty(submit))
                l.Add(submit);
            return l.ToArray();
        }


        /// <summary>
        ///     Run a Mobile Suit with default Prompt.
        /// </summary>
        /// <returns>0, is All ok.</returns>
        public int Run();

        /// <summary>
        ///     Run a Mobile Suit with default Prompt.
        /// </summary>
        /// <returns>0, is All ok.</returns>
        public int Run(string[] args);

        /// <summary>
        ///     Asynchronously run some SuitCommands in current environment, until one of them returns a non-AllOK TraceBack.
        /// </summary>
        /// <param name="scripts">SuitCommands</param>
        /// <param name="withPrompt">if this run contains prompt, or silent</param>
        /// <param name="scriptName">name of these scripts</param>
        /// <returns>The TraceBack of the last executed command.</returns>
        Task<TraceBack> RunScriptsAsync(IAsyncEnumerable<string?> scripts, bool withPrompt = false,
            string? scriptName = null);

        /// <summary>
        ///     Run some SuitCommands in current environment, until one of them returns a non-AllOK TraceBack.
        /// </summary>
        /// <param name="scripts">SuitCommands</param>
        /// <param name="withPrompt">if this run contains prompt, or silent</param>
        /// <param name="scriptName">name of these scripts</param>
        /// <returns>The TraceBack of the last executed command.</returns>
        TraceBack RunScripts(IEnumerable<string> scripts, bool withPrompt = false, string? scriptName = null);

        /// <summary>
        ///     Run a command in current host.
        /// </summary>
        /// <param name="command">the command to run</param>
        /// <returns>result of the command</returns>
        public TraceBack RunCommand(string? command);
    }

    /// <summary>
    ///     Basic Settings of a MobileSuitHost
    /// </summary>
    public record HostSettings
    {
        /// <summary>
        ///     If the prompt contains the reference (For example, System.Console.Title) of current instance.
        /// </summary>
        public bool HideReference { get; set; }

        /// <summary>
        ///     Throw Exceptions, instead of using TraceBack.
        /// </summary>
        public bool EnableThrows { get; set; }

        /// <summary>
        ///     If show that a command has been executed.
        /// </summary>
        public bool ShowDone { get; set; }

        /// <summary>
        ///     If this SuitHost will not exit UNLESS user input exit command.
        /// </summary>
        public bool NoExit { get; set; }





    }
}