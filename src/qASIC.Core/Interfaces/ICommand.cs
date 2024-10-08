﻿namespace qASIC
{
    public interface ICommand
    {
        /// <summary>Main name used for identifying which command to execute.</summary>
        string CommandName { get; }
        /// <summary>Name aliases for identifying which command to execute.</summary>
        string[] Aliases { get; }
        /// <summary>Short description used when displaying a list of commands in help.</summary>
        string Description { get; }
        /// <summary>Detailed description used when displaying command specific description in help.</summary>
        string DetailedDescription { get; }

        /// <summary>Method for executing command logic.</summary>
        /// <param name="args">Arguments for the command.</param>
        object Run(CommandArgs args);
    }
}