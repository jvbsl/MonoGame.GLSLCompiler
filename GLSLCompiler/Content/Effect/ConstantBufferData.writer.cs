using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GLSLCompiler.Content.Effect
{
    public partial class ConstantBufferData
    {
        public void Write(BinaryWriter writer, Options options)
        {
            if (options.Profile == ShaderProfile.OpenGL)
                writer.Write(Name);

            writer.Write((ushort)Size);

            writer.Write((byte)ParameterIndex.Count);
            for (var i = 0; i < ParameterIndex.Count; i++)
            {
                writer.Write((byte)ParameterIndex[i]);
                writer.Write((ushort)ParameterOffset[i]);
            }
        }
    }
}
