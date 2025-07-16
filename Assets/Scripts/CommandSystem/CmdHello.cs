using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace T2G
{
    public class CmdHello : Command
    {
        public static readonly string CommandKey = "hello";

        public override async Awaitable<bool> Execute(params string[] args)
        {
            OnExecutionCompleted?.Invoke(true, ConsoleController.eSender.Assistant,
                    $"Hello {SettingsT2G.User}, {GetGreeting()}");
            await Task.Yield();
            return true;
        }

        string GetGreeting()
        {
            string greeting = "Good ";

            int hour = DateTime.Now.Hour;

            if (hour >= 5 && hour < 12)
                greeting += "Morning!";
            else if (hour >= 12 && hour < 17)
                greeting += "Afternoon!";
            else if (hour >= 17 && hour < 21)
                greeting += "Evening!";
            else
                greeting += "Night!";

            return greeting;
        }

        public override string GetKey()
        {
            return CommandKey.ToLower();
        }

        public override string[] GetArguments()
        {
            string[] args = { };
            return args;
        }
    }
}