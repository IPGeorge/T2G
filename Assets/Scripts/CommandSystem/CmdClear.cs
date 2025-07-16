using System.Threading.Tasks;
using UnityEngine;

namespace T2G
{
    public class CmdClear : Command
    {
        public static readonly string CommandKey = "clear";
        public override async Awaitable<bool> Execute(params string[] args)
        {
            ConsoleController.Instance.Clear();
            await Task.Yield();
            OnExecutionCompleted?.Invoke(true, ConsoleController.eSender.Assistant, "Cleared!");
            return true;
        }

        public override string GetKey()
        {
            return CommandKey.ToLower();
        }

        public override string[] GetArguments()
        {
            return null;
        }

    }
}