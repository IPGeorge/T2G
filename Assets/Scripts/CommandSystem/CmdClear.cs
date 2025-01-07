using UnityEngine;

public class CmdClear : Command
{
    public static readonly string CommandKey = "Clear";
    public override bool Execute(params string[] args)
    {
        ConsoleController.Instance.Clear();
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
