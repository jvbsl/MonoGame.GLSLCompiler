using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GLSLCompiler
{
    class Compiler : IDisposable
    {
        private static char[] header = new char[] { 'M', 'G', 'F', 'X' };
        private string dummyFile, effectFile;
        public Compiler(string dummyFile)
        {
            this.dummyFile = dummyFile;
        }
        public bool Compile(string filename)
        {
            return false;
            /*string output = System.IO.Path.GetTempFileName();
			TwoMGFX.Options options = new TwoMGFX.Options ();
			options.Debug = false;
			options.SourceFile = filename;
			options.Profile = TwoMGFX.ShaderProfile.PureGLSL;
			options.OutputFile = output;
			TwoMGFX.ShaderInfo shaderInfo;
			try
			{
				shaderInfo = TwoMGFX.ShaderInfo.FromFile(options.SourceFile, options, new ConsoleEffectCompilerOutput());
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex.Message);
				Console.Error.WriteLine("Failed to parse '{0}'!", options.SourceFile);
				return false;
			}

			// Create the effect object.
			TwoMGFX.EffectObject effect;
			var shaderErrorsAndWarnings = string.Empty;
			try
			{
				effect = TwoMGFX.EffectObject.CompileEffect(shaderInfo, out shaderErrorsAndWarnings);

				if (!string.IsNullOrEmpty(shaderErrorsAndWarnings))
					Console.Error.WriteLine(shaderErrorsAndWarnings);
			}
			catch (TwoMGFX.ShaderCompilerException)
			{
				// Write the compiler errors and warnings and let the user know what happened.
				Console.Error.WriteLine(shaderErrorsAndWarnings);
				Console.Error.WriteLine("Failed to compile '{0}'!", options.SourceFile);
				return false;
			}
			catch (Exception ex)
			{
				// First write all the compiler errors and warnings.
				if (!string.IsNullOrEmpty(shaderErrorsAndWarnings))
					Console.Error.WriteLine(shaderErrorsAndWarnings);

				// If we have an exception message then write that.
				if (!string.IsNullOrEmpty(ex.Message))
					Console.Error.WriteLine(ex.Message);

				// Let the user know what happened.
				Console.Error.WriteLine("Unexpected error compiling '{0}'!", options.SourceFile);
				return false;
			}

			// Get the output file path.
			if (options.OutputFile == string.Empty)
				options.OutputFile = Path.GetFileNameWithoutExtension(options.SourceFile) + ".mgfxo";

			// Write out the effect to a runtime format.
			try
			{
				using (var stream = new FileStream(options.OutputFile, FileMode.Create, FileAccess.Write))
				using (var writer = new BinaryWriter(stream))
					effect.Write(writer, options);
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex.Message);
				Console.Error.WriteLine("Failed to write '{0}'!", options.OutputFile);
				return false;
			}

			// We finished succesfully.
			Console.WriteLine("Compiled '{0}' to '{1}'.", options.SourceFile, options.OutputFile);

            this.effectFile = output;
			return true;*/
        }
        public void SaveStream(Stream output)
        {
            using (Stream input = new FileStream(dummyFile, FileMode.Open, FileAccess.Read))
            {
                int currentHeaderPos = 0;
                byte[] effectBuffer = System.IO.File.ReadAllBytes(effectFile);
                BinaryReader inputReader = new BinaryReader(input,System.Text.Encoding.ASCII);
                output.Write(inputReader.ReadBytes(6), 0, 6);
                do
                {
                    char current = inputReader.ReadChar();
                    if (current == header[currentHeaderPos])
                        currentHeaderPos++;
                    else
                        currentHeaderPos = 0;
                } while (currentHeaderPos < header.Length);
                output.Write(BitConverter.GetBytes((int)effectBuffer.Length), 0, sizeof(int));
                long count = inputReader.BaseStream.Position - header.Length - 6 - sizeof(int);
                inputReader.BaseStream.Seek(6 + sizeof(int), SeekOrigin.Begin);
                output.Write(inputReader.ReadBytes((int)count), 0, (int)count);
                output.Write(effectBuffer, 0, effectBuffer.Length);
            }
        }
        public void Dispose()
        {

        }
    }

}
