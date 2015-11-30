using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace GLSLCompiler.Content.Effect
{
	public class ShaderInfo
	{
		public string FilePath { get; internal set; }

		public string FileContent { get; internal set; }

		public ShaderProfile Profile { get; internal set; }

		public string OutputFilePath { get; internal set; }

		public bool Debug { get; internal set; }

		public List<TechniqueInfo> Techniques = new List<TechniqueInfo> ();
		public Dictionary<string, SamplerStateInfo> SamplerStates = new Dictionary<string, SamplerStateInfo> ();

		public List<string> Dependencies { get; internal set; } = new List<string>();

		public List<string> AdditionalOutputFiles { get; internal set; }

		static public ShaderInfo FromFile (string path, Options options, IEffectCompilerOutput output)
		{
            
			var effectSource = File.ReadAllText (path);
			if (options.Profile == ShaderProfile.PureGLSL)
				return PureGLSLFromString (effectSource, path, options, output);
			return null;
			//return FromString(effectSource, path, options, output);
		}
		private static float parseColorPart(string value)
		{
			value = value.Trim ();
			int tmp;
			if (int.TryParse (value, out tmp))
				return  tmp / 255.0f;
			return float.Parse (value);
		}

		private static Color ParseColor (XmlElement el)
		{
			if (el.HasChildNodes) {
				float a=1.0f, r=0, g=0, b=0;
				foreach (XmlElement e in el.ChildNodes.OfType<XmlElement>()) {
					if (e.Name == "A") {
						a = parseColorPart (e.InnerText);							
					} else if (e.Name == "R") {
						r = parseColorPart (e.InnerText);							
					} else if (e.Name == "G") {
						g = parseColorPart (e.InnerText);							
					} else if (e.Name == "B") {
						b = parseColorPart (e.InnerText);							
					} else {
						throw new PipelineException("'"+e.Name+"' is not an option for the Color element");
					}
				}
				
				return new Color ( r, g, b,a);
			}
			if (string.IsNullOrEmpty (el.InnerText.Trim ()))
				throw new PipelineException ("Empty value not allowed for Colors");
			try{
				System.Reflection.FieldInfo fI = typeof(Color).GetType().GetField(el.InnerText.Trim(),System.Reflection.BindingFlags.Static);
				return (Color)fI.GetValue(null);
			}catch{
			}
			{
				string value = el.InnerText.Trim ();
				int a, r, g, b;
				if (value.Length == 4 || value.Length == 3) {
					int index = 0;
					if (value.Length == 4)
						a = Convert.ToInt16 (value [index++].ToString(), 16);
					else
						a = 0xF;
					r = Convert.ToInt16 (value [index++].ToString(), 16);
					g = Convert.ToInt16 (value [index++].ToString(), 16);
					b = Convert.ToInt16 (value [index].ToString(), 16);

					a = a << 4 | a;
					r = r << 4 | r;
					g = g << 4 | g;
					b = b << 4 | b;
				} else if (value.Length == 6 || value.Length == 8) {
					int index = 0;
					if (value.Length == 6)
						a = Convert.ToInt16 (value.Substring (index += 2, 2), 16);
					else
						a = 0xFF;
					r = Convert.ToInt16 (value.Substring (index += 2, 2), 16);
					g = Convert.ToInt16 (value.Substring (index += 2, 2), 16);
					b = Convert.ToInt16 (value.Substring (index, 2), 16);
				} else {
					throw new PipelineException ("Color must either use A/R/G/B Xml Elements or be a Hexadecimal value of Length 3/4/6/8");
				}
				

				return new Color (r, g, b, a);
			}
			//throw new Exception ("Unknown error while parsing color");
		}

		private static BlendState ParseBlendState (XmlElement element)
		{
			if (!element.HasChildNodes || element.Name != "BlendState")
				return null;
			BlendState blendState = new BlendState ();
			foreach (XmlElement el in element.ChildNodes.OfType<XmlElement>()) {
				if (el.Name == "AlphaBlendFunction")
					blendState.AlphaBlendFunction = (BlendFunction)Enum.Parse (typeof(BlendFunction), el.InnerText);
				else if (el.Name == "AlphaDestinationBlend")
					blendState.AlphaDestinationBlend = (Blend)Enum.Parse (typeof(Blend), el.InnerText);
				else if (el.Name == "AlphaSourceBlend")
					blendState.AlphaSourceBlend = (Blend)Enum.Parse (typeof(Blend), el.InnerText);
				else if (el.Name == "BlendFactor")
					blendState.BlendFactor = ParseColor (el);
				else if (el.Name == "ColorBlendFunction")
					blendState.ColorBlendFunction = (BlendFunction)Enum.Parse (typeof(BlendFunction), el.InnerText);
				else if (el.Name == "ColorDestinationBlend")
					blendState.ColorDestinationBlend = (Blend)Enum.Parse (typeof(Blend), el.InnerText);
				else if (el.Name == "ColorSourceBlend")
					blendState.ColorSourceBlend = (Blend)Enum.Parse (typeof(Blend), el.InnerText);
				else if (el.Name == "ColorWriteChannels")
					blendState.ColorWriteChannels = (ColorWriteChannels)Enum.Parse (typeof(ColorWriteChannels), el.InnerText);
				else if (el.Name == "ColorWriteChannels1")
					blendState.ColorWriteChannels1 = (ColorWriteChannels)Enum.Parse (typeof(ColorWriteChannels), el.InnerText);
				else if (el.Name == "ColorWriteChannels2")
					blendState.ColorWriteChannels2 = (ColorWriteChannels)Enum.Parse (typeof(ColorWriteChannels), el.InnerText);
				else if (el.Name == "ColorWriteChannels3")
					blendState.ColorWriteChannels3 = (ColorWriteChannels)Enum.Parse (typeof(ColorWriteChannels), el.InnerText);
				else if (el.Name == "IndependentBlendEnable")
					blendState.IndependentBlendEnable = bool.Parse (el.InnerText);
				else if (el.Name == "MultiSampleMask")
					blendState.MultiSampleMask = int.Parse (el.InnerText);
				else
					throw new PipelineException ("'" + el.Name + "' is not an option of the BlendState");
				
			}
			return blendState;
		}

		private static DepthStencilState ParseDepthStencilState (XmlElement element)
		{
			if (!element.HasChildNodes || element.Name != "DepthStencilState")
				return null;
			DepthStencilState depthStencilState = new DepthStencilState ();
			foreach (XmlElement el in element.ChildNodes.OfType<XmlElement>()) {
				if (el.Name == "CounterClockwiseStencilDepthBufferFail")
					depthStencilState.CounterClockwiseStencilDepthBufferFail = (StencilOperation)Enum.Parse (typeof(StencilOperation), el.InnerText);
				else if (el.Name == "CounterClockwiseStencilFail")
					depthStencilState.CounterClockwiseStencilFail = (StencilOperation)Enum.Parse (typeof(StencilOperation), el.InnerText);
				else if (el.Name == "CounterClockwiseStencilFunction")
					depthStencilState.CounterClockwiseStencilFunction = (CompareFunction)Enum.Parse (typeof(CompareFunction), el.InnerText);
				else if (el.Name == "CounterClockwiseStencilPass")
					depthStencilState.CounterClockwiseStencilPass = (StencilOperation)Enum.Parse (typeof(StencilOperation), el.InnerText);
				else if (el.Name == "DepthBufferEnable")
					depthStencilState.DepthBufferEnable = bool.Parse (el.InnerText);
				else if (el.Name == "DepthBufferFunction")
					depthStencilState.DepthBufferFunction = (CompareFunction)Enum.Parse (typeof(CompareFunction), el.InnerText);
				else if (el.Name == "DepthBufferWriteEnable")
					depthStencilState.DepthBufferWriteEnable = bool.Parse (el.InnerText);
				else if (el.Name == "ReferenceStencil")
					depthStencilState.ReferenceStencil = int.Parse (el.InnerText);
				else if (el.Name == "DepthBufferFunction")
					depthStencilState.StencilDepthBufferFail = (StencilOperation)Enum.Parse (typeof(StencilOperation), el.InnerText);
				else if (el.Name == "ReferenceStencil")
					depthStencilState.StencilEnable = bool.Parse (el.InnerText);
				else if (el.Name == "StencilFail")
					depthStencilState.StencilFail = (StencilOperation)Enum.Parse (typeof(StencilOperation), el.InnerText);
				else if (el.Name == "StencilFunction")
					depthStencilState.StencilFunction = (CompareFunction)Enum.Parse (typeof(CompareFunction), el.InnerText);
				else if (el.Name == "DepthBufferFunction")
					depthStencilState.StencilMask = int.Parse (el.InnerText);
				else if (el.Name == "StencilPass")
					depthStencilState.StencilPass = (StencilOperation)Enum.Parse (typeof(StencilOperation), el.InnerText);
				else if (el.Name == "DepthBufferFunction")
					depthStencilState.StencilWriteMask = int.Parse (el.InnerText);
				else if (el.Name == "TwoSidedStencilMode")
					depthStencilState.TwoSidedStencilMode = bool.Parse (el.InnerText);
				else
					throw new PipelineException ("'" + el.Name + "' is not an option of the DepthStencilState");
			}
			return depthStencilState;
		}

		private static RasterizerState ParseRasterizerState (XmlElement element)
		{
			if (!element.HasChildNodes || element.Name != "RasterizerState")
				return null;
			RasterizerState rasterizerState = new RasterizerState ();
			foreach (XmlElement el in element.ChildNodes.OfType<XmlElement>()) {
				if (el.Name == "CullMode")
					rasterizerState.CullMode = (CullMode)Enum.Parse (typeof(CullMode), el.InnerText);
				else if (el.Name == "DepthBias")
					rasterizerState.DepthBias = float.Parse (el.InnerText);
				else if (el.Name == "DepthClipEnable")
					rasterizerState.DepthClipEnable = bool.Parse (el.InnerText);
				else if (el.Name == "FillMode")
					rasterizerState.FillMode = (FillMode)Enum.Parse (typeof(FillMode), el.InnerText);
				else if (el.Name == "MultiSampleAntiAlias")
					rasterizerState.MultiSampleAntiAlias = bool.Parse (el.InnerText);
				else if (el.Name == "ScissorTestEnable")
					rasterizerState.ScissorTestEnable = bool.Parse (el.InnerText);
				else if (el.Name == "SlopeScaleDepthBias")
					rasterizerState.SlopeScaleDepthBias = float.Parse (el.InnerText);
				else
					throw new Exception ("'" + el.Name + "' is not an option of the RasterizerState");

			}
			return rasterizerState;
		}

		static public ShaderInfo PureGLSLFromString (string effectSource, string filePath, Options options, IEffectCompilerOutput output)
		{

			ShaderInfo shaderInfo = new ShaderInfo ();
			shaderInfo.FilePath = options.SourceFile;
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (effectSource);
			XmlElement effectElement;
			XmlNode current = doc.FirstChild;
			while (current != null && current.NodeType != XmlNodeType.Element) {
				current = current.NextSibling;
			}
			effectElement = (XmlElement)current;
			shaderInfo.Techniques = new List<TechniqueInfo> ();
			foreach (XmlElement technique in effectElement.ChildNodes.OfType<XmlElement>()) {
				TechniqueInfo info = new TechniqueInfo ();
				info.name = technique.GetAttribute ("name");
				info.Passes = new List<PassInfo> ();
				foreach (XmlElement pass in technique.ChildNodes.OfType<XmlElement>()) {
					PassInfo pi = new PassInfo ();
					pi.name = pass.GetAttribute ("name");
					foreach (XmlElement sh in pass.ChildNodes) {
						if (sh.Name == "Shader") {
							if (sh.GetAttribute ("type") == "PixelShader") {
								pi.psFileName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(shaderInfo.FilePath),sh.GetAttribute ("filename"));
								shaderInfo.Dependencies.Add (pi.psFileName);
								pi.psShaderXML = sh.OuterXml;
							} else if (sh.GetAttribute ("type") == "VertexShader") {
								pi.vsFilename = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(shaderInfo.FilePath),sh.GetAttribute ("filename"));
								shaderInfo.Dependencies.Add (pi.vsFilename);
								pi.vsShaderXML = sh.OuterXml;
							} else {
								throw new PipelineException ("Unsupported Shader type detected");
							}
						} else if (sh.Name == "BlendState") {
							pi.blendState = ParseBlendState (sh);

						} else if (sh.Name == "DepthStencilState") {
							pi.depthStencilState = ParseDepthStencilState (sh);
						} else if (sh.Name == "RasterizerState") {
							pi.rasterizerState = ParseRasterizerState (sh);
						} else {
							throw new PipelineException ("'" + sh.Name + "' element not recognized");
						}
					}
					info.Passes.Add (pi);
				}
				shaderInfo.Techniques.Add (info);
			}

			shaderInfo.Profile = options.Profile;
			shaderInfo.Debug = options.Debug;

			shaderInfo.OutputFilePath = options.OutputFile;

			return shaderInfo;
		}
	}
}
