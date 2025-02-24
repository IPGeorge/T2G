using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Windows.Input;
using System;


/* 
 * operations:
//The object in front of view is current default selected 
//What is the object name?
//Set -options (wear glasses)
//set daytime - weather
//set object scale (sx, sy, sz)
//place|move object at|to (x, y, z)
//move object further/closer for 10 units
//Move actor to left/right for 10 units 
//Rotate actor (drx, dry, drz)
//play
//stop
*/


public partial class Interpreter
{
    private static List<(string, Func<Match, string[]> handler)> _rules = null;

    static void InitializeRules()
    {


        _rules = new List<(string, Func<Match, string[]>)>
        {
            (@"(?i)^\s*(please\s+)?(add|place|create)\s+(?<x>(a|an|\d+))?\s*(enemy|enemies)$\s*", GenInstructions_AddEnemy),
                //(?i): case insensitive
                //^\s*: starts with 0 to multiple spaces
                //(please\s+)?: optional with the world "please" and certain number of spaces
                //(add|place|create)\s+: one of "add", ""place", or "create" followed with one or certain number of spaces
                //(?<x>(a|an|\d+))?: match group index alias name "x". it can be "a", "an", or a number (one or more digits).
                //                   This section is aoptional
                //\s*: no or some spaces
                //(enemy|enemies)$\s*: end with "enemy" or "enemies" followed by no or some spaces is accepted.. 
            (@"(?i)^\s*(please\s+)?(set|change)\s+(move speed)\s+(?<x>\d+)\s*", GenInstructions_SetMoveSpeed),  //units
            (@"(?i)^\s*(please\s+)?(set|change)\s+(turn speed)\s+(?<x>\d+)\s*", GenInstructions_SetTurnSpeed),  //degrees
/*
            (@"(?i)^\s*move\s+(forward)?\s*", GenInstructions_View_MoveForward),
            (@"(?i)^\s*move\s+(backward)?\s*", GenInstructions_View_MoveBackward),
            (@"(?i)^\s*move\s+(left)?\s*", GenInstructions_View_MoveLeft),
            (@"(?i)^\s*move\s+(right)?\s*", GenInstructions_View_MoveRight),
            (@"(?i)^\s*move\s+(up)?\s*", GenInstructions_View_MoveUp),
            (@"(?i)^\s*move\s+(down)?\s*", GenInstructions_View_MoveDown),
            (@"(?i)^\s*(turn|rotate)\s+(left|counterclockwise)?\s*", GenInstructions_View_TurnLeft),
            (@"(?i)^\s*(turn|rotate)\s+(right|clockwise)?\s*", GenInstructions_View_TurnRight),
            (@"(?i)^\s*(go|move)\s+(to)?\s+\.*\s*$", GenInstructions_View_GoToObject),
            (@"(?i)^\s*(go|move)\s+to\s+\(?\s*(?<x>-?\d+(\.\d+)?\s*,\s*?<y>-?\d+(\.\d+)?\s*,\s*?<z>-?d+(\.\d+)?\)?\s*$", GenInstructions_View_GoToPosition),
*/

        };
    }

    public static string[] InterpretPromptRBP(string prompt)
    {
        if(_rules == null)
        {
            InitializeRules();
        }

        foreach (var (pattern, handler) in _rules)
        {
            var match = Regex.Match(prompt, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var value = match.Groups["x"].Value;
                return handler(match);
            }
        }
        return null;
    }


    //Interpretion functions =========================================================
    //================================================================================
    static string[] GenInstructions_AddEnemy(Match match)
    {
        List<string> instructions = new List<string>();
        var quantityStr = match.Groups["x"].Value;
        if(string.IsNullOrWhiteSpace(quantityStr) || 
            string.Compare(quantityStr, "a", true) == 0 ||
            string.Compare(quantityStr, "an", true) == 0)
        {
            quantityStr = "1";
        }

        if(int.TryParse(quantityStr, out var quantity))
        {
            quantity = Mathf.Clamp(quantity, 1, 10);    //Maximum 10 


        }

        return instructions.ToArray();
    }

    static string[] GenInstructions_SetMoveSpeed(Match match)
    {
        List<string> instructions = new List<string>();

        return instructions.ToArray();
    }

    static string[] GenInstructions_SetTurnSpeed(Match match)
    {
        List<string> instructions = new List<string>();

        return instructions.ToArray();
    }

    static string[] GenInstructions_Move_View(Match match)
    {
        List<string> instructions = new List<string>();

        return instructions.ToArray();
    }

    static string[] GenInstructions_Turn_View(Match match)
    {
        List<string> instructions = new List<string>();

        return instructions.ToArray();
    }

}
