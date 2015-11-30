# MonoGame.GLSLCompiler
A GLSL Content Processor for MonoGame(needs MonoGame.GLSLLibrary to use compiled effect)

## Usage
Add the Compiler assembly to your Content Project(.mgcb file)
```
#-------------------------------- References --------------------------------#

/reference:../../tools/GLSLCompiler.dll
```

Create your shader files(VertexShader/PixelShader).

Create a new file which combines the shader files for each pass:
```xml
<Effect>
    <Technique name="Technique1">
        <Pass name="Pass1">
            <Shader type="PixelShader" filename="pass1.ps">

            </Shader>
            <Shader type="VertexShader" filename="pass1.vs">
                <attribute name="position">Position</attribute><!-- Maps the attributes to the VertexInputs -->
                <attribute name="normal">Normal</attribute><!-- Maps the attributes to the VertexInputs -->
                <attribute name="texCoord">TexCoord</attribute><!-- Maps the attributes to the VertexInputs -->
            </Shader>
            <RasterizerState>
                <!-- Multiple options possible(possibly everything the RasterizerState of MonoGame can do -->
            </RasterizerState>
            <DepthStencilState>
              <!-- Multiple options possible(possibly everything the DepthStencilState of MonoGame can do -->
            </DepthStencilState>
            <BlendState>
              <!-- Multiple options possible(possibly everything the BlendState of MonoGame can do -->
            </BlendState>
        </Pass>
    </Technique>
</Effect>
```
Save that file with the .glsl extension.

Add that file to your content project and compile it.

You should no be able to use the [GLSLLibrary](https://github.com/jvbsl/MonoGame.GLSLLibrary) to load and render your compiled shader.
