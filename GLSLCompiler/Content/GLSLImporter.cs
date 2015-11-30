using Microsoft.Xna.Framework.Content.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GLSLCompiler.Content
{
    [ContentImporter(".glsl", DefaultProcessor = "GLSLProcessor",
          DisplayName = "GLSL Effect Importer")]
    class GLSLImporter : ContentImporter<GLSLSourceCode>
    {
        public override GLSLSourceCode Import(string filename,
            ContentImporterContext context)
        {
			GLSLSourceCode source = new GLSLSourceCode (filename);
			source.Load(context);
			return source;
        }
    }
}
