using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using System.Collections;

namespace GLSLCompiler.Content.Effect
{
	public partial class ShaderData
	{
		private static string RemoveWhitespaces(string glsl)
		{
			return Regex.Replace (glsl, @"\s+",new MatchEvaluator( m =>{ return m.Value[0].ToString();}));
		}
		private static string RemoveComments (string glsl)
		{
			return Regex.Replace (glsl, @"(?>/(/[^\r\n]*|(?s:\*((?!\*/).)*\*/)))", "");
		}
		private static string RemoveComments (string glsl, out List<KeyValuePair<int,int>> commentPositions)
		{
			Regex r = new Regex (@"(?>/(/[^\r\n]*|(?s:\*((?!\*/).)*\*/)))");
			//int removedChars = 0;
			List<KeyValuePair<int,int>>  tmp = new List<KeyValuePair<int,int>> ();

			string nocomment= r.Replace (glsl, new MatchEvaluator (m => {
				tmp.Add (new KeyValuePair<int,int> (m.Index, m.Value.Length));
				return "";
			}));
			commentPositions = tmp;
			return nocomment;
		}

		private static int findClosingCurlyBracket (string glsl, int start)
		{
			if (start == -1 || start >= glsl.Length)
				return -1;
			int opening = 1;
			if (glsl [start] == '{')
				start++;
			
			do {
				if (start >= glsl.Length)
					return -1;
				int fClose = glsl.IndexOf('}',start);
				int fOpen = glsl.IndexOf('{',start);

				int fFound = fClose == -1 ? fOpen : fClose;
				if (fFound == -1)
					return -1;
				
				if (fOpen != -1)
					fFound = Math.Min(fOpen,fFound);

				if (glsl[fFound] == '{')
					opening++;
				else /*if (glsl[fFound] == '}')*/
					opening--;
				start = fFound+1;
			} while (opening > 0);
			return start - 1;
		}
		private static int calcAbsoluteIndex(int index,List<KeyValuePair<int,int>> commentPositions)
		{
			if (commentPositions.Count == 0)
				return index;
			Queue<KeyValuePair<int,int>> q = new Queue<KeyValuePair<int,int>> (commentPositions);
			int newIndex = index;
			do {
				KeyValuePair<int,int> comment = q.Dequeue();
				if (newIndex < comment.Key)
					return newIndex;
				newIndex +=comment.Value;
			} while(q.Count > 0);
			return newIndex;
		}
		private static string AddPosFixup (string glsl)
		{
			/*	gl_Position.y = gl_Position.y * posFixup.y;
	gl_Position.xy += posFixup.zw * gl_Position.ww;*/
			List<KeyValuePair<int,int>> commentPositions;
			string noComment = RemoveComments (glsl,out commentPositions);
			if (noComment.Contains ("posFixup")) {
				throw new Exception("error: The name 'posFixup' is reserved by MonoGame");
			}
			Match methodHead = new Regex ("void(\\s*)main(\\s*)\\((\\s*)\\)").Match (noComment);

			int openingBracket = noComment.IndexOf ('{', methodHead.Index + methodHead.Value.Length);
			if (openingBracket == -1)
				throw new Exception ("Could not insert posFixup:problem finding main methods opening bracket");
			int closingBracket = findClosingCurlyBracket (noComment, openingBracket);
			if (closingBracket == -1)
				throw new Exception ("Could not insert posFixup:problem finding main methods closing bracket");
			closingBracket = calcAbsoluteIndex (closingBracket, commentPositions);
			//throw new Exception (glsl.Length+"lbla:"+closingBracket);

			glsl = glsl.Insert (closingBracket - 1, "\n\tgl_Position.y = gl_Position.y * posFixup.y;\n\tgl_Position.xy += posFixup.zw * gl_Position.ww;");

			//(glsl);
			int methodHeadStart = calcAbsoluteIndex (methodHead.Index, commentPositions);

			glsl = glsl.Insert (methodHeadStart - 1, "\nuniform vec4 posFixup;\n");


			return glsl;
		}
		private static string AddPrecisions(string glsl,bool vertexShader,bool debug)
		{
			List<KeyValuePair<int,int>> commentPositions;
			string noComment = RemoveComments (glsl,out commentPositions);
			int insertPos=0;
			if (noComment.Contains ("#version")) {
				Match versionPos = new Regex (".*#version .*\n").Match (noComment);
				if (versionPos.Success)
					insertPos = calcAbsoluteIndex (versionPos.Index + versionPos.Length,commentPositions);
			}
			string insertionString = "";
			var floatPrecision = vertexShader ? "precision highp float;\r\n" : "precision mediump float;\r\n";
			if (debug) {//TODO: setting debug flags
				insertionString = "#pragma optimize(off)\r\n";
			}
			insertionString += "#ifdef GL_ES\r\n" +
				floatPrecision +
				"precision mediump int;\r\n" +
				"#endif\r\n";
			glsl = glsl.Insert (Math.Max (insertPos, 0), insertionString);
			if(!debug)
			{
				glsl = RemoveWhitespaces(RemoveComments(glsl));
			}
			return glsl;
		}
		public static ShaderData CreatePureGLSL (byte[] byteCode, string filename, bool isVertexShader, List<ConstantBufferData> cbuffers, int sharedIndex, Dictionary<string, SamplerStateInfo> samplerStates, string shaderInfo, bool debug)
		{
			var dxshader = new ShaderData ();
			dxshader.SharedIndex = sharedIndex;
			dxshader.Bytecode = (byte[])byteCode.Clone ();
			// Use MojoShader to convert the HLSL bytecode to GLSL.
			string glsl = System.Text.Encoding.ASCII.GetString (byteCode);
			if (isVertexShader)
				glsl = AddPosFixup (glsl);

			Dictionary<string, MojoShader.MOJOSHADER_usage> usageMapping = new Dictionary<string, MojoShader.MOJOSHADER_usage> ();
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (shaderInfo);
			XmlElement shader = (XmlElement)doc.FirstChild;
			foreach (XmlElement attribute in shader.ChildNodes) {
				if (attribute.Name == "attribute") {
					usageMapping.Add (attribute.GetAttribute ("name"), (MojoShader.MOJOSHADER_usage)Enum.Parse (typeof(MojoShader.MOJOSHADER_usage), "MOJOSHADER_USAGE_" + attribute.InnerText.ToUpper (), true));
				}
			}
			Additional.GLSLShaderParser parser = new Additional.GLSLShaderParser (glsl, filename, isVertexShader, usageMapping);
			if (!parser.Parse ())
				return null;

			dxshader.IsVertexShader = isVertexShader;

			dxshader._attributes = parser.Attributes.ToArray ();

			var symbols = parser.Symbols.ToArray ();

			//try to put the symbols in the order they are eventually packed into the uniform arrays
			//this /should/ be done by pulling the info from mojoshader
			Array.Sort (symbols, delegate (MojoShader.MOJOSHADER_symbol a, MojoShader.MOJOSHADER_symbol b) {
				uint va = a.register_index;
				if (a.info.elements == 1)
					va += 1024; //hax. mojoshader puts array objects first
				uint vb = b.register_index;
				if (b.info.elements == 1)
					vb += 1024;
				return va.CompareTo (vb);
			}
			);//(a, b) => ((int)(a.info.elements > 1))a.register_index.CompareTo(b.register_index));

			// NOTE: It seems the latest versions of MojoShader only 
			// output vec4 register sets.  We leave the code below, but
			// the runtime has been optimized for this case.

			// For whatever reason the register indexing is 
			// incorrect from MojoShader.
			{
				uint bool_index = 0;
				uint float4_index = 0;
				uint int4_index = 0;

				for (var i = 0; i < symbols.Length; i++) {
					switch (symbols [i].register_set) {
					case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_BOOL:
						symbols [i].register_index = bool_index;
						bool_index += symbols [i].register_count;
						break;

					case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_FLOAT4:
						symbols [i].register_index = float4_index;
						float4_index += symbols [i].register_count;
						break;

					case MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_INT4:
						symbols [i].register_index = int4_index;
						int4_index += symbols [i].register_count;
						break;
					}
				}
			}
			// Get the samplers.
			var samplers = parser.Samplers.ToArray ();
			dxshader._samplers = new Sampler[samplers.Length];
			for (var i = 0; i < samplers.Length; i++) {
				// We need the original sampler name... look for that in the symbols.
				var originalSamplerName =
					symbols.First (e => e.register_set == MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_SAMPLER &&
					e.register_index == samplers [i].index
					).name;

				var sampler = new Sampler {
					//sampler mapping to parameter is unknown atm
					parameter = -1,

					// GLSL needs the MojoShader mangled sampler name.
					samplerName = samplers [i].name,

					// By default use the original sampler name for the parameter name.
					parameterName = originalSamplerName,

					textureSlot = samplers [i].index,
					samplerSlot = samplers [i].index,
					type = samplers [i].type,
				};

				SamplerStateInfo state;
				if (samplerStates.TryGetValue (originalSamplerName, out state)) {
					sampler.state = state.State;
					sampler.parameterName = state.TextureName ?? originalSamplerName;
				}

				// Store the sampler.
				dxshader._samplers [i] = sampler;
			}

			// Gather all the parameters used by this shader.
			var symbol_types = new[] {
				new { name = dxshader.IsVertexShader ? "vs_uniforms_bool" : "ps_uniforms_bool", set = MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_BOOL, },
				new { name = dxshader.IsVertexShader ? "vs_uniforms_ivec4" : "ps_uniforms_ivec4", set = MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_INT4, },
				new { name = dxshader.IsVertexShader ? "vs_uniforms_vec4" : "ps_uniforms_vec4", set = MojoShader.MOJOSHADER_symbolRegisterSet.MOJOSHADER_SYMREGSET_FLOAT4, },
			};
			var cbuffer_index = new List<int> ();
			for (var i = 0; i < symbol_types.Length; i++) {
				var cbuffer = new ConstantBufferData (symbol_types [i].name,
					                          symbol_types [i].set,
					                          symbols);
				if (cbuffer.Size == 0)
					continue;

				var match = cbuffers.FindIndex (e => e.SameAs (cbuffer));
				if (match == -1) {
					cbuffer_index.Add (cbuffers.Count);
					cbuffers.Add (cbuffer);
				} else
					cbuffer_index.Add (match);
			}
			dxshader._cbuffers = cbuffer_index.ToArray ();



			//TODO: glslCode translator


			// TODO: This sort of sucks... why does MojoShader not produce
			// code valid for GLES out of the box?

			// GLES platforms do not like this.
			//glsl = glsl.Replace ("#version 110", "");

			// Add the required precision specifiers for GLES.
			AddPrecisions(glsl,dxshader.IsVertexShader,debug);


			// Store the code for serialization.
			dxshader.ShaderCode = Encoding.ASCII.GetBytes (glsl);
			return dxshader;
		}
	}
}
