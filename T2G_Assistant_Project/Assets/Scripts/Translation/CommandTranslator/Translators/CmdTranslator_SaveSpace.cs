using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace T2G.Assistant
{
    [CommandTranslator("save_space")]
    public class CmdTranslator_SaveSpace : CmdTranslatorBase
    {
        public override (bool succeeded, InstructionBase instructionBase) Translate((string name, string value)[] args)
        {
            Instruction instruction = new Instruction();
            instruction.action = GetActionName();
            return (true, instruction);
        }
    }
}