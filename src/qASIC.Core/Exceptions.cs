﻿using System;

namespace qASIC
{
    public class CommandException : Exception
    {
        public CommandException() : base() { }
        public CommandException(string message) : base(message) { }

        public override string ToString()
        {
            return $"{Message}\n{StackTrace}";
        }

        public string ToString(bool includeStackTrace) =>
            includeStackTrace ?
            ToString() :
            Message;
    }

    public class CommandParseException : CommandException
    {
        public CommandParseException(Type type, string arg)
        {
            this.type = type;
            this.arg = arg;
        }

        Type type;
        string arg;

        public override string Message =>
            $"Unable to parse '{arg}' to {type}";
    }

    public class CommandArgsCountException : CommandException
    {
        public CommandArgsCountException() { }
        public CommandArgsCountException(int inputArgsCount, int minArgsCount, int maxArgsCount)
        {
            this.inputArgsCount = inputArgsCount;
            this.minArgsCount = minArgsCount;
            this.maxArgsCount = maxArgsCount;
        }

        int inputArgsCount;
        int minArgsCount;
        int maxArgsCount;

        public override string Message
        {
            get
            {
                if (inputArgsCount < minArgsCount)
                    return "Not enough arguments";

                if (inputArgsCount > maxArgsCount)
                    return "Too many arguments";

                return "Invalid argument count";
            }
        }
    }
}