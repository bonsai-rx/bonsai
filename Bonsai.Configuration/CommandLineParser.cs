using System;
using System.Collections.Generic;

namespace Bonsai.Configuration
{
    public class CommandLineParser
    {
        const char WordSeparator = '-';
        const char OptionSeparator = ':';
        const string CommandPrefix = "--";
        readonly Dictionary<string, Delegate> commands = new Dictionary<string, Delegate>();
        Action<string, int> defaultHandler;

        public void RegisterCommand(Action<string, int> handler)
        {
            defaultHandler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public void RegisterCommand(string name, Action handler)
        {
            RegisterCommandHandler(name, handler);
        }

        public void RegisterCommand(string name, Action<string> handler)
        {
            RegisterCommandHandler(name, handler);
        }

        void RegisterCommandHandler(string name, Delegate handler)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "The name of the command cannot be null and must contain non-whitespace characters.",
                    nameof(name));
            }

            if (!name.StartsWith(CommandPrefix))
            {
                throw new ArgumentException(
                    "The name of the command must include the long prefix \"" + CommandPrefix + "\".",
                    nameof(name));
            }

            commands.Add(name, handler ?? throw new ArgumentNullException(nameof(handler)));
            name = name.Substring(1);

            var words = name.Split(WordSeparator);
            words[0] = CommandPrefix;
            if (words.Length > 2)
            {
                commands.Add(string.Concat(words), handler);
            }

            var shorthand = new string(Array.ConvertAll(words, word => word[0]));
            if (!commands.ContainsKey(shorthand))
                commands.Add(shorthand, handler);
        }

        public void Parse(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                var options = args[i].Split(new[] { OptionSeparator }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (commands.TryGetValue(options[0], out Delegate handler))
                {
                    if (handler is Action action) action();
                    else if (handler is Action<string> command)
                    {
                        var argument = string.Empty;
                        if (options.Length > 1) argument = options[1];
                        else if (args.Length > i + 1 && !args[i + 1].StartsWith(CommandPrefix))
                        {
                            argument = args[++i];
                        }
                        command(argument);
                    }
                }
                else
                {
                    defaultHandler?.Invoke(args[i], i);
                }
            }
        }
    }
}
