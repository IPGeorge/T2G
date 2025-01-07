using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using T2G.UnityAdapter;
using System.Threading.Tasks;

public class CmdDisconnect : Command
{
    public static readonly string CommandKey = "Disconnect";

    public override bool Execute(params string[] args)
    {
        CommunicatorClient.Instance.Disconnect();
        Task.Run(async () => { await WaitForDisconnection(); });
        return true;
    }


    async Task WaitForDisconnection()
    {
        while (CommunicatorClient.Instance.ClientState == CommunicatorClient.eClientState.Disconnecting)
        {
            await Task.Delay(100);
        }
        OnExecutionCompleted?.Invoke(true, ConsoleController.eSender.System, "Disconnected!");
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
