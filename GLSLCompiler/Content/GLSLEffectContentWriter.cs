using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GLSLCompiler.Content
{
    [ContentTypeWriter]
    class GLSLEffectContentWriter : ContentTypeWriter<GLSLCompiledEffect>
    {
        protected override void Write(ContentWriter output, GLSLCompiledEffect value)
        {
			if (!value.Loaded)
				return;
            var code = value.GetEffectCode();
            if (code == null)
                return;
            output.Write(code.Length);
            output.Write(code);

        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            var type = typeof(ContentReader);
            var readerType = type.Namespace + ".EffectReader, " + type.Assembly.FullName;
            return readerType;
        }
    }
}
