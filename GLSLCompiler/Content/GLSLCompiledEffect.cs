using GLSLCompiler.Content.Effect;
using Microsoft.Xna.Framework.Content.Pipeline;
using System;
using System.IO;

namespace GLSLCompiler.Content
{
    internal class GLSLCompiledEffect
    {
        public Effect.EffectObject EffectObject
        {
            get; private set;
        }
		public bool Loaded{ get; private set; }
        private GLSLSourceCode sourceCode;

		public GLSLCompiledEffect(GLSLSourceCode sourceCode)
        {
            this.sourceCode = sourceCode;
        }
        public bool Compile(ContentProcessorContext context)
        {
			if (!sourceCode.Loaded)
				return Loaded = false;
            EffectObject = EffectObject.CompileEffect(sourceCode.ShaderInfo, context);

            /*catch (ShaderCompilerException)
            {
                // Write the compiler errors and warnings and let the user know what happened.
                Console.Error.WriteLine(shaderErrorsAndWarnings);
                Console.Error.WriteLine("Failed to compile '{0}'!", options.SourceFile);
                return false;
            }*/
			return Loaded = true;
        }
        public byte[] GetEffectCode()
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    using (var writer = new BinaryWriter(stream))
                        EffectObject.Write(writer, sourceCode.Options);
                    return stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine("Failed to write '{0}'!", sourceCode.Options.OutputFile);
                return null;
            }
        }
    }
}