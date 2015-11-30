using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GLSLCompiler.Content.Effect
{
    public class TechniqueInfo
    {
        public int startPos;
        public int length;

        public string name;
        public List<PassInfo> Passes = new List<PassInfo>();
    }
}
