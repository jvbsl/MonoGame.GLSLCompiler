using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GLSLCompiler.Content.Effect
{
    public class Options
    {
        public string SourceFile;

        public string OutputFile = string.Empty;

      
        public ShaderProfile Profile = ShaderProfile.OpenGL;

        public bool Debug;

        public string Defines;
    }
}
