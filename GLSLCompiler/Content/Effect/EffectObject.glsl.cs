using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace GLSLCompiler.Content.Effect
{
    partial class EffectObject
    {
        private static byte[] CompileGLSL(ShaderInfo shaderInfo, string shaderFilename, string shaderXML, ContentProcessorContext context)
        {
			return System.IO.File.ReadAllBytes(shaderFilename);
        }
    }
}
