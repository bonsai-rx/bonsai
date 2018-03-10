using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    class CommandLineParser
    {
        const char WordSeparator = '-';
        const char OptionSeparator = ':';
        const string CommandPrefix = "--";
        readonly Dictionary<string, Action<string>> commands = new Dictionary<string, Action<string>>();
        Action<string> defaultHandler;

        public void RegisterCommand(Action<string> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            defaultHandler = handler;
        }

        public void RegisterCommand(string name, Action handler)
        {
            RegisterCommand(name, option => handler());
        }

        public void RegisterCommand(string name, Action<string> handler)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The name of the command cannot be null and must contain non-whitespace characters.");
            }

            if (!name.StartsWith(CommandPrefix))
            {
                throw new ArgumentException("The name of the command must include the long prefix \"" + CommandPrefix + "\".");
            }

            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            commands.Add(name, handler);
            name = name.Substring(1);

            var words = name.Split(WordSeparator);
            words[0] = CommandPrefix;
            if (words.Length > 2)
            {
                commands.Add(string.Concat(words), handler);
            }

            var shorthand = new string(Array.ConvertAll(words, word => word[0]));
            commands.Add(shorthand, handler);
        }

        public void Parse(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                Action<string> handler;
                var options = args[i].Split(new[] { OptionSeparator }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (commands.TryGetValue(options[0], out handler))
                {
                    handler(options.Length > 1 ? options[1] : string.Empty);
                }
                else if (defaultHandler != null)
                {
                    defaultHandler(args[i]);
                }
            }
        }
    }
}
