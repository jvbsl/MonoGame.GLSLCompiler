using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GLSLCompiler.Content.Effect
{
    public interface IEffectCompilerOutput
    {
        void WriteWarning(string file, int line, int column, string message);
        void WriteError(string file, int line, int column, string message);
    }
}
