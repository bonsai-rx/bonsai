using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bonsai.Osc
{
    class MessagePattern
    {
        const char SingleWildcard = '?';
        const char MultipleWildcard = '*';
        const char CharacterSetOpen = '[';
        const char CharacterSetClose = ']';
        const char CharacterRangeSeparator = '-';
        const char CharacterSetNegation = '!';
        const char StringSetOpen = '{';
        const char StringSetClose = '}';
        const char StringSetSeparator = ',';
        bool matchRegex;
        string pattern;

        public MessagePattern(string pattern)
        {
            matchRegex = false;
            var characterSet = false;
            var regexBuilder = new StringBuilder();
            foreach (var c in pattern)
            {
                switch (c)
                {
                    case SingleWildcard: regexBuilder.Append('.'); matchRegex = true; break;
                    case MultipleWildcard: regexBuilder.Append(".*"); matchRegex = true; break;
                    case CharacterSetOpen: regexBuilder.Append('['); characterSet = true; matchRegex = true; break;
                    case CharacterSetClose: regexBuilder.Append(']'); characterSet = false; break;
                    case CharacterSetNegation: regexBuilder.Append(characterSet ? '^' : c); break;
                    case StringSetOpen: regexBuilder.Append('('); matchRegex = true; break;
                    case StringSetSeparator: regexBuilder.Append('|'); break;
                    case StringSetClose: regexBuilder.Append(')'); break;
                    default: regexBuilder.Append(c); break;
                }
            }

            this.pattern = regexBuilder.ToString();
        }

        public bool IsMatch(string part)
        {
            if (matchRegex) return Regex.IsMatch(part, pattern);
            else return part == pattern;
        }
    }
}
