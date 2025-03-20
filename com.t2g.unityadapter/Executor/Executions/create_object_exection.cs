using System;
using System.Threading.Tasks;
using T2G.Executor;
using UnityEngine;

namespace T2G
{
    [Execution("create_object ")]
    public class create_object_exection : Execution
    {
        public async override Awaitable<(bool succeeded, string message)> Execute(Instruction instruction)
        {
            await Task.Delay(1000);
            return (true, "Ok!");
        }
    }
}
