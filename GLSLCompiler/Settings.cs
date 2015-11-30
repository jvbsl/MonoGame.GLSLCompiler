using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLSLCompiler
{
    class Settings
    {
        public Settings(string[] args)
        {
            if (args.Length < 2)
                throw new ArgumentException("No input file specified");
            
            int index = 0;
            do
            {
                parsePart(args,ref index);
            } while (index < args.Length);
        }
        public void parsePart(string[] args,ref int index)
        {
            if (isType(args[index],"input","i"))
            {
                InputFile = args[++index];
                ++index;
            }
            else if (isType(args[index], "output", "o"))
            {
                OutputFile = args[++index];
                ++index;
            }
            else if (isType(args[index], "dummy", "d"))
            {
                DummyFile = args[++index];
                ++index;
            }
            else
            {
                throw new ArgumentException("Unrecognized option:" + args[index]);
            }

        }
        private static bool isType(string input,string full,string shrt)
        {
            if (input[0] == '-')
            {
                if (input[1] == '-')
                    return input.Substring(2) == full;
                return input.Substring(1) == shrt;
            }
            return false;
        }
        public string DummyFile { get; private set; } = "dummy.xnb";
        public string InputFile { get; private set; }
        public string OutputFile { get; private set; } = "output.xnb";
    }
}
