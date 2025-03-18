using System.Collections.Generic;
using UnityEngine;

namespace T2G
{
    [Translator("create_project_gamedesc")]
    public class GameDesc_Translator : Translator
    {
        public override (bool succeeded, string message) Translate((string name, string value)[] arguments, ref List<Instruction> instructions)
        {
            return (true, null);
        }
    }
}
