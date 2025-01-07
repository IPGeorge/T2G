using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class Command
{
    public Action<bool, ConsoleController.eSender, string> OnExecutionCompleted;
    public abstract string GetKey();
    public abstract string[] GetArguments();
    public abstract bool Execute(params string[] args);
}
