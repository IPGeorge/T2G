using SimpleJSON;
using System.Collections.Generic;

namespace T2G
{
    [Translator("export_gamedesc")]
    public class ExportGameDesc_Translator : Translator
    {
        public override (bool succeeded, string message) Translate((string name, string value)[] arguments, ref List<Instruction> instructions)
        {
            instructions.Clear();

            var attributeName = GetAttributeName();
            if (attributeName == null)
            {
                return (false, k_FailedToRetrieveAttribute);
            }

            Instruction instruction = new Instruction();

            string filePath = GetParamFromArguments(arguments, "filePath", string.Empty);


            return (true, null);
        }
    }
}
