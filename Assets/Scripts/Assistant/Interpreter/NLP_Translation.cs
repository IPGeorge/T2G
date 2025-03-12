using UnityEngine;

namespace T2G
{
    public class NLP_Translation : Translation  //Natual Language Process
    {
        protected override bool ParseInstructionData(string prompt, out string key, out (string name, string value)[] arguments)
        {
            key = string.Empty;
            arguments = null;
            return false;
        }

        public override bool Translate(string prompt, out Instruction[] instructions)
        {
            _instructionList.Clear();
            instructions = _instructionList.ToArray();
            return (_instructionList.Count > 0);
        }
    }
}