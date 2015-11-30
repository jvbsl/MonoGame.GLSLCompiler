using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GLSLCompiler.Content.Effect
{
    public enum ShaderProfile
    {
        // NOTE: This order matters and is used as part
        // of the file format... don't change it.
        OpenGL = 0,
        DirectX_11 = 1,
        PlayStation4 = 2,
        PureGLSL = 4,
    }
}
