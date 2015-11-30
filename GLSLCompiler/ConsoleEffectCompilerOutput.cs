using GLSLCompiler.Content.Effect;
using Microsoft.Xna.Framework.Content.Pipeline;
using System;

namespace GLSLCompiler
{
	internal class ConsoleEffectCompilerOutput : IEffectCompilerOutput
	{
        private ContentImporterContext context;
        public ConsoleEffectCompilerOutput(ContentImporterContext context)
        {
            this.context = context;
        }
		public void WriteWarning(string file, int line, int column, string message)
		{
			context.Logger.LogImportantMessage("Warning: {0}({1},{2}): {3}" , file, line, column, message);
		}

		public void WriteError(string file, int line, int column, string message)
		{
            context.Logger.LogImportantMessage(string.Format("Error: {0}({1},{2}): {3}", file, line, column, message));
		}
	}
}

