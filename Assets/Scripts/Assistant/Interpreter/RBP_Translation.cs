using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace T2G
{
    public class RBP_Translation : Translation     //Rule-Based Process
    {
        //TODO: Should create a unit test and test data to improve the following patterns
        static (string pattern, string key)[] _rules = {
            (@"^(test|test instruction)$", "test"),
            (@"^(create|create a|create a new|create new)\s+((game|project|game project)\s+)(?:(under|at|in)\s+)?(?<path>[a-zA-Z]:[\\/][^\s]+(?:[\\/][^\s]+)*)?(?:\.)?$", "create_project"),
                //create\s+(\w\s+)*game(?: project)? --> Matches "create a game" or "create game" and optionally "project".
                //  (?: project)? --> The (?: ... ) is a non-capturing group, making "project" optional.
                //(?:under|at) --> Matches either "under" or "at".
                //  (?: (?:under|at))? --> Makes "under" or "at" optional, so it still matches even if they are missing.
                //(?<path>[a-zA-Z]:[\\/][^\s]+) --> Captures the file path as the named group "path":
                //                 [\\/] --> This allows both \ and / as valid path separators.
                //  [a-zA-Z]:\\ --> Matches a drive letter(C:, D:, etc.).
                //  [^\s]+ --> Matches the rest of the path(until a space appears).
                //\.? --> Makes the trailing period optional.
            (@"(init|initialize)\s+(?:(game|project|game project)\s+)?(?:(under|at|in)\s+)?(?<path>[a-zA-Z]:[\\/][^\s]+(?:[\\/][^\s]+)*)?(?:\.)?", "init_project"),
            (@"open\s+(?:(game|project|game project)\s+)?(?:(under|at)\s+)?(?<path>[a-zA-Z]:[\\/][^\s]+(?:[\\/][^\s]+)*)?(?:\.)?", "open_project"),
            (@"^(connect|connect to)$(?:\s+\w)?(?:\.)?", "connect"),
            (@"^(disconnect|disconnect from)$(?:\s+\w)?(?:\.)?", "disconnect"),
            (@"(clear|clear all)(?:\.)?", "clear"),
            (@"^create\s+(?:(a|a new|new)\s+)?(?<type>scene|level|space)(?:\s+(?:named|with the name))?\s+(?<name>.+?)(?:\.)?$", "create_space"),
                //^ --> Start of the string
                //create\s+ --> Matches "create" followed by one or more spaces
                //(?:a\s+new\s+)? --> Matches optional "a new " (non-capturing group)
                //(?<type>scene|level|space) --> Captures "scene", "level", or "space" as the argument named "type"
                //(?:\s+named)? --> Matches optional " named" (non-capturing group)
                //\s+(<name>.+?) --> Captures the name (one or more characters after "named" or directly after the type) as tghe argument named "name"
                //(?:\.)? --> Matches an optional period at the end
                //$ --> Ensures the command ends there
            (@"^(enter|go to|open)\s+(?:(?<type>scene|level|space)\s+)?(?:named|with the name\s+)?(?<name>.+?)(?:\.)?$", "enter_space"),
            (@"^(?:\w+\s+)?(create|place|Add)\s+(?:a|an\s+)?(?<type>(?:\s+\w+))(?:\s+(?:name|named|with the name))?\s+(?<name>.+?)(?:\.)?$", "create_object"),
            (@"^(?:\w+\s+)?(delete|remove)(?:\s+(?:object))?\s+(?<name>.+?)(?:\.)?$", "delete_object"),
            //("", "set_object_position"),
            //("", "set_object_orientation"),
            //("", "set_object_scale"),
            //("", "move_object"),
            //("", "rotate_object"),
            //("", "scale_object_up"),
            //("", "select_object"),
            //("", "set_object_property"),
            //("", "add_object_component"),
            //("", "create_from_gamedesc"),
            //("", "save_gamedesc")
        };

        public static int[] TestRegexMatch(string text)
        {
            List<int> matchIndices = new List<int>();
            for(int i = 0; i < _rules.Length; ++i)
            {
                var pattern = _rules[i].pattern;
                var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
                if(match.Success)
                {
                    matchIndices.Add(i);
                }
            }
            return matchIndices.ToArray();
        }

        protected override bool ParseInstructionData(string prompt, out string key, out (string name, string value)[] arguments)
        {
            List<(string, string)> args = new List<(string, string)>();
            for(int i = 0; i < _rules.Length; ++i)  //TODO: consider using multi-thread for betterr performance
            {
                var pattern = _rules[i].pattern;
                var match = Regex.Match(prompt, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    key = _rules[i].key;
                    for(int j = 0; j < match.Groups.Count; ++j)
                    {
                        string groupName = match.Groups[j].Name;
                        if(!string.IsNullOrEmpty(groupName) && !int.TryParse(groupName, out var value))
                        {
                            args.Add((match.Groups[j].Name, match.Groups[j].Value));
                        }
                    }
                    arguments = args.ToArray();
                    return true;
                }
            }
            key = string.Empty;
            arguments = null;
            return false;
        }
    }
}
