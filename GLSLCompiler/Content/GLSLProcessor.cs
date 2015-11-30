using Microsoft.Xna.Framework.Content.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace GLSLCompiler.Content
{
	[ContentProcessor(DisplayName = "GLSL Effect Processor")]
    class GLSLProcessor : ContentProcessor<GLSLSourceCode, GLSLCompiledEffect>
    {
		bool debug;

		/// <summary>
		/// The debug mode for compiling effects.
		/// </summary>
		/// <value>The debug mode to use when compiling effects.</value>
		public bool Debug { get { return debug; } set { debug = value; } }
        public override GLSLCompiledEffect Process(GLSLSourceCode input, ContentProcessorContext context)
        {
			input.ShaderInfo.Debug = debug;
			GLSLCompiledEffect compiled = new GLSLCompiledEffect(input);
			foreach (var dep in input.ShaderInfo.Dependencies) {
				context.AddDependency (dep);
			}
			compiled.Compile (context);
            return compiled;
        }
    }
}
