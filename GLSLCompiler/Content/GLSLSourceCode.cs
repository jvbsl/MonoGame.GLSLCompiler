using GLSLCompiler.Content.Effect;
using Microsoft.Xna.Framework.Content.Pipeline;
using System;
using System.Collections.Generic;

using System.Xml;

namespace GLSLCompiler.Content
{
    class GLSLSourceCode
    {
        public ShaderInfo ShaderInfo
        {
            get; private set;
        }
        public Options Options
        {
            get; private set;
        } = new Options();
		public bool Loaded{ get; private set; }
        public GLSLSourceCode(string filename)
        {
            Options.SourceFile = filename;
            Options.Profile = ShaderProfile.PureGLSL;
        }
		public void Load(ContentImporterContext context)
		{
			ShaderInfo = ShaderInfo.FromFile(Options.SourceFile, Options, new ConsoleEffectCompilerOutput(context));
			Loaded = true;
		}
    }
}
