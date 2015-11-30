using GLSLCompiler.Content.Effect;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GLSLCompiler.Additional
{
    class GLSLShaderParser
    {
        private INativeWindow window;
        private IGraphicsContext context;
        private int program;
        private int shader;
        private bool isVertexShader;
        private string code;
        private Dictionary<string, MojoShader.MOJOSHADER_usage> usageMapping;
        private string filename;
        public GLSLShaderParser(string code,string filename, bool isVertexShader, Dictionary<string, MojoShader.MOJOSHADER_usage> usageMapping)
        {
            this.usageMapping = usageMapping;
            this.code = code;
            this.isVertexShader = isVertexShader;
            this.filename = filename;

        }
		public bool Parse()
		{
			Exception e=null;
			Thread thrd = new Thread(new ThreadStart(delegate
				{
					try{
					CreateContext();

					CreateProgram();

					if (Compile())
					{
						Attach();
						if (Link())
						{

							LoadAttributes();
							LoadUniforms();
						}
					}
					}catch(Exception ex)
					{
						e = ex;
					}
				}));
			thrd.Start();
			thrd.Join();
			window.Dispose ();
			context.Dispose ();
			if (e != null)
				throw e;
			return true;
		}
        public List<ShaderData.Attribute> Attributes { get; private set; }
        public List<MojoShader.MOJOSHADER_symbol> Symbols { get; private set; }
        public List<MojoShader.MOJOSHADER_sampler> Samplers { get; private set; }
        private void LoadAttributes()
        {
            Attributes = new List<ShaderData.Attribute>();
            int count = 0;
            GL.GetProgram(program, GetProgramParameterName.ActiveAttributes, out count);

            int maxNameLength = 0;
            GL.GetProgram(program, GetProgramParameterName.ActiveAttributeMaxLength, out maxNameLength);
            StringBuilder nameData = new StringBuilder(maxNameLength);
            for (int attrib = 0; attrib < count; ++attrib)
            {
                int arraySize = 0;
                ActiveAttribType type = ActiveAttribType.None;
                int actLen = 0;
                GL.GetActiveAttrib(program, attrib, maxNameLength, out actLen, out arraySize, out type, nameData);
                ShaderData.Attribute attribute = new ShaderData.Attribute();
                attribute.name = nameData.ToString();
                attribute.index = 0;
                MojoShader.MOJOSHADER_usage usage;
                if (usageMapping.TryGetValue(attribute.name, out usage))
                    attribute.usage = EffectObject.ToXNAVertexElementUsage(usage);


                Attributes.Add(attribute);

                nameData.Clear();
            }
        }
        private MojoShader.MOJOSHADER_symbolClass getSymbolClass(ActiveUniformType type)
        {
            switch (type)
            {
                case ActiveUniformType.FloatMat2:
                case ActiveUniformType.FloatMat3:
                case ActiveUniformType.FloatMat4:
                case ActiveUniformType.FloatMat2x3:
                case ActiveUniformType.FloatMat2x4:
                case ActiveUniformType.FloatMat3x2:
                case ActiveUniformType.FloatMat3x4:
                case ActiveUniformType.FloatMat4x2:
                case ActiveUniformType.FloatMat4x3:
                    return MojoShader.MOJOSHADER_symbolClass.MOJOSHADER_SYMCLASS_MATRIX_COLUMNS;
                case ActiveUniformType.Float:
                    return MojoShader.MOJOSHADER_symbolClass.MOJOSHADER_SYMCLASS_SCALAR;
                case ActiveUniformType.BoolVec2:
                case ActiveUniformType.BoolVec3:
                case ActiveUniformType.BoolVec4:
                case ActiveUniformType.DoubleVec2:
                case ActiveUniformType.DoubleVec3:
                case ActiveUniformType.DoubleVec4:
                case ActiveUniformType.FloatVec2:
                case ActiveUniformType.FloatVec3:
                case ActiveUniformType.FloatVec4:
                case ActiveUniformType.IntVec2:
                case ActiveUniformType.IntVec3:
                case ActiveUniformType.IntVec4:
                case ActiveUniformType.UnsignedIntVec2:
                case ActiveUniformType.UnsignedIntVec3:
                case ActiveUniformType.UnsignedIntVec4:
                    return MojoShader.MOJOSHADER_symbolClass.MOJOSHADER_SYMCLASS_VECTOR;
            }
            return MojoShader.MOJOSHADER_symbolClass.MOJOSHADER_SYMCLASS_OBJECT;
        }
        private MojoShader.MOJOSHADER_symbolType getSymbolType(ActiveUniformType type)
        {
            ActiveUniformType baseType = UniformTypeInfos.getBaseType(type);
            switch (baseType)
            {
                case ActiveUniformType.Int:
                case ActiveUniformType.UnsignedInt:
                    return MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_INT;
                case ActiveUniformType.Float:
                case ActiveUniformType.Double:
                    return MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_FLOAT;
                case ActiveUniformType.Sampler1D:
                    return MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_SAMPLER1D;
                case ActiveUniformType.Sampler2D:
                    return MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_SAMPLER2D;
                case ActiveUniformType.Sampler3D:
                    return MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_SAMPLER3D;
                case ActiveUniformType.SamplerCube:
                    return MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_SAMPLERCUBE;
            }
            return MojoShader.MOJOSHADER_symbolType.MOJOSHADER_SYMTYPE_VOID;
        }
        private void LoadUniforms()
        {
            Symbols = new List<MojoShader.MOJOSHADER_symbol>();
            Samplers = new List<MojoShader.MOJOSHADER_sampler>();
            int count = 0;
            GL.GetProgram(program, GetProgramParameterName.ActiveUniforms, out count);

            int maxNameLength = 0;
            GL.GetProgram(program, GetProgramParameterName.ActiveUniformMaxLength, out maxNameLength);
            StringBuilder nameData = new StringBuilder(maxNameLength);
            for (int uni = 0; uni < count; ++uni)
            {
                int arraySize = 0;
                ActiveUniformType type;
                int actLen = 0;
                GL.GetActiveUniform(program, uni, maxNameLength, out actLen, out arraySize, out type, nameData);

                string name = nameData.ToString();
                uint elementSize = UniformTypeInfos.getElementSize(type);

                MojoShader.MOJOSHADER_symbol sym = new MojoShader.MOJOSHADER_symbol();
                sym.name = name;
                sym.info = new MojoShader.MOJOSHADER_symbolTypeInfo();
                sym.info.elements = (uint)arraySize;
                sym.info.columns = UniformTypeInfos.getColumnCount(type);
                sym.info.rows = UniformTypeInfos.getElementCount(type, UniformTypeInfos.getBaseType(type)) / sym.info.columns;

                sym.info.members = IntPtr.Zero;
                sym.info.member_count = 0;
                sym.info.parameter_class = getSymbolClass(type);//TODO: 
                sym.info.parameter_type = getSymbolType(type);//TODO: 

                sym.register_index = (uint)GL.GetUniformLocation(program, name);
                sym.register_count = (uint)arraySize * (uint)Math.Ceiling(elementSize / 4.0f);
                ActiveUniformType baseType = UniformTypeInfos.getBaseType(type);
                if (baseType == ActiveUniformType.Bool)
                    sym.register_set = MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_BOOL;
                else if (baseType == ActiveUniformType.Float)
                    sym.register_set = MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_FLOAT4;
                else if (baseType == ActiveUniformType.Int || baseType == ActiveUniformType.UnsignedInt)
                    sym.register_set = MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_INT4;
                else
                    sym.register_set = MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_SAMPLER;


                Symbols.Add(sym);

                if (sym.register_set == MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_SAMPLER)
                    LoadSampler(sym, type);


                //TODO: add to list


                nameData.Clear();
            }
        }
        private ActiveUniformType getBaseSampler(ActiveUniformType type)
        {
            if (type == ActiveUniformType.Sampler1D || type == ActiveUniformType.Sampler1DShadow || type == ActiveUniformType.IntSampler1D || type == ActiveUniformType.UnsignedIntSampler1D)
                return ActiveUniformType.Sampler1D;

            if (type == ActiveUniformType.Sampler2D || type == ActiveUniformType.Sampler2DShadow
                || type == ActiveUniformType.IntSampler2D || type == ActiveUniformType.UnsignedIntSampler2D
                || type == ActiveUniformType.IntSampler2DRect || type == ActiveUniformType.UnsignedIntSampler2DRect
                || type == ActiveUniformType.IntSampler2DMultisample || type == ActiveUniformType.Sampler2DMultisample
                || type == ActiveUniformType.Sampler2DRectShadow || type == ActiveUniformType.UnsignedIntSampler2D
                || type == ActiveUniformType.UnsignedIntSampler2DMultisample)
                return ActiveUniformType.Sampler2D;
            if (type == ActiveUniformType.Sampler3D
                 || type == ActiveUniformType.IntSampler3D || type == ActiveUniformType.UnsignedIntSampler3D
                 || type == ActiveUniformType.UnsignedIntSampler3D)
                return ActiveUniformType.Sampler3D;

            if (type == ActiveUniformType.SamplerCube || type == ActiveUniformType.IntSamplerCube
                || type == ActiveUniformType.SamplerCubeShadow || type == ActiveUniformType.UnsignedIntSamplerCube)
                return ActiveUniformType.SamplerCube;

            return ActiveUniformType.Sampler1D;

        }
        private MojoShader.MOJOSHADER_samplerType getSamplerType(ActiveUniformType type)
        {
            ActiveUniformType t = getBaseSampler(type);
            switch (t)
            {
                case ActiveUniformType.Sampler1D:
                    return MojoShader.MOJOSHADER_samplerType.MOJOSHADER_SAMPLER_1D;
                case ActiveUniformType.Sampler2D:
                    return MojoShader.MOJOSHADER_samplerType.MOJOSHADER_SAMPLER_2D;
                case ActiveUniformType.SamplerCube:
                    return MojoShader.MOJOSHADER_samplerType.MOJOSHADER_SAMPLER_CUBE;
                case ActiveUniformType.Sampler3D:
                    return MojoShader.MOJOSHADER_samplerType.MOJOSHADER_SAMPLER_VOLUME;
            }
            return MojoShader.MOJOSHADER_samplerType.MOJOSHADER_SAMPLER_UNKNOWN;
        }
        private void LoadSampler(MojoShader.MOJOSHADER_symbol sym, ActiveUniformType type)
        {
            MojoShader.MOJOSHADER_sampler s = new MojoShader.MOJOSHADER_sampler();

            s.index = (int)sym.register_index;
            s.name = sym.name;
            s.texbem = 0;
            s.type = getSamplerType(UniformTypeInfos.getBaseType(type));

            Samplers.Add(s);
        }

        private void CreateContext()
        {
            window = new GameWindow();
            context = new GraphicsContext(GraphicsMode.Default, window.WindowInfo);
            context.MakeCurrent(window.WindowInfo);
        }
        private void CreateProgram()
        {
            program = GL.CreateProgram();
            GL.UseProgram(program);
        }
        private bool Compile()
        {

            shader = GL.CreateShader(isVertexShader ? ShaderType.VertexShader : ShaderType.FragmentShader);
            GL.ShaderSource(shader, code);
            GL.CompileShader(shader);

            int isCompiled = 0;
            GL.GetShader(shader, ShaderParameter.CompileStatus, out isCompiled);
            if (isCompiled == 0)
            {
                int maxLength = 0;
                GL.GetShader(shader, ShaderParameter.InfoLogLength, out maxLength);

                // The maxLength includes the NULL character
                StringBuilder str = new StringBuilder(maxLength);
                GL.GetShaderInfoLog(shader, maxLength, out maxLength, str);

                // Provide the infolog in whatever manor you deem best.
                // Exit with failure.
                GL.DeleteShader(shader); // Don't leak the shader.
                throw new Exception("'" + System.IO.Path.GetFileName(filename) + "' compilation failed: \n" + str.ToString());
                return false;
            }
            return true;
        }
        private void Attach()
        {
            GL.AttachShader(program, shader);
        }
        private bool Link()
        {
            GL.LinkProgram(program);

            int isLinked = 0;
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out isLinked);
            if (isLinked == 0)
            {
                int maxLength = 0;
                GL.GetProgram(program, GetProgramParameterName.InfoLogLength, out maxLength);

                // The maxLength includes the NULL character
                StringBuilder str = new StringBuilder(maxLength);
                GL.GetProgramInfoLog(program, maxLength, out maxLength, str);

                // Provide the infolog in whatever manor you deem best.
                // Exit with failure.
                GL.DeleteShader(shader); // Don't leak the shader.
                GL.DeleteProgram(program);
                throw new Exception("'" + System.IO.Path.GetFileName(filename) + "' linking failed: \n" + str.ToString());
                return false;
            }
            return true;
        }
    }
}
