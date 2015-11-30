using System;
using System.IO;


namespace GLSLCompiler
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try {
                Settings settings = new Settings(args);
                Compiler comp = new Compiler(settings.DummyFile);

                comp.Compile(settings.InputFile);

                using (FileStream fs = new FileStream(settings.OutputFile, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    comp.SaveStream(fs);
                }
                Console.WriteLine("Done");
            }catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Usage: GLSLCompiler -i InputFile [-o OutputFile] [-d DummyFile]");
                Console.WriteLine("\t-i/--input:\t Specifies the input file to compile");
                Console.WriteLine("\t-o/--output:\t Specifies the output file for the compiled xnb");
                Console.WriteLine("\t-d/--dummy:\t Specifies a dummy file to use the xnb header from");
            }
        }
    }
}
