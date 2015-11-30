using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GLSLCompiler.Additional
{
    internal class UniformTypeInfos
    {
        public static uint getBaseSize(ActiveUniformType type)
        {
            switch (type)
            {
                case ActiveUniformType.Bool:
                    return 1;
                case ActiveUniformType.Double:
                    return 2;
                case ActiveUniformType.Float:
                    return 1;
                /*case ActiveUniformType.Image1D:
                    return 1;
                case ActiveUniformType.Image2D:
                    return 1;
                case ActiveUniformType.Image3D:
                    return 1;*/
                case ActiveUniformType.Int:
                    return 1;
                case ActiveUniformType.Sampler1D:
                    return 1;
                case ActiveUniformType.Sampler2D:
                    return 1;
                case ActiveUniformType.Sampler3D:
                    return 1;
                case ActiveUniformType.UnsignedInt:
                    return 1;
            }
            return 1;
        }
        public static ActiveUniformType getBaseType(ActiveUniformType type)
        {
            if (type <= ActiveUniformType.BoolVec4 && type >= ActiveUniformType.Bool)
                return ActiveUniformType.Bool;
            if (type <= ActiveUniformType.DoubleVec4 && type >= ActiveUniformType.DoubleVec2 || type == ActiveUniformType.Double)
                return ActiveUniformType.Double;
            if (type <= ActiveUniformType.FloatVec4 && type >= ActiveUniformType.FloatVec2 || type == ActiveUniformType.Float)
                return ActiveUniformType.Float;
            if (type == ActiveUniformType.Image1DArray)
                return ActiveUniformType.Image1D;
            if (type == ActiveUniformType.Image2DArray)
                return ActiveUniformType.Image2D;
            if (type == ActiveUniformType.Image2DMultisampleArray)
                return ActiveUniformType.Image2DMultisample;
            if (type == ActiveUniformType.ImageCubeMapArray)
                return ActiveUniformType.ImageCube;
            if (type <= ActiveUniformType.IntVec4 && type >= ActiveUniformType.IntVec2 || type == ActiveUniformType.Int)
                return ActiveUniformType.Int;
            if (type == ActiveUniformType.IntSampler1DArray)
                return ActiveUniformType.IntSampler1D;
            if (type == ActiveUniformType.IntSampler2DArray)
                return ActiveUniformType.IntSampler2D;
            if (type == ActiveUniformType.IntSampler2DMultisampleArray)
                return ActiveUniformType.IntSampler2DMultisample;
            if (type == ActiveUniformType.IntSamplerCubeMapArray)
                return ActiveUniformType.IntSamplerCube;
            if (type == ActiveUniformType.Sampler1DArray || type == ActiveUniformType.Sampler1DArrayShadow)
                return ActiveUniformType.Sampler1D;
            if (type == ActiveUniformType.Sampler2DArray || type == ActiveUniformType.Sampler2DArrayShadow)
                return ActiveUniformType.Sampler2D;
            if (type == ActiveUniformType.SamplerCubeMapArray || type == ActiveUniformType.SamplerCubeMapArrayShadow)
                return ActiveUniformType.SamplerCube;
            if (type == ActiveUniformType.UnsignedIntImage1DArray)
                return ActiveUniformType.UnsignedIntImage1D;
            if (type == ActiveUniformType.UnsignedIntImage2DArray)
                return ActiveUniformType.UnsignedIntImage2D;
            if (type == ActiveUniformType.UnsignedIntImage2DMultisampleArray)
                return ActiveUniformType.UnsignedIntImage2DMultisample;
            if (type == ActiveUniformType.UnsignedIntImageCubeMapArray)
                return ActiveUniformType.UnsignedIntImageCube;
            if (type == ActiveUniformType.UnsignedIntSampler1DArray)
                return ActiveUniformType.UnsignedIntSampler1D;
            if (type == ActiveUniformType.UnsignedIntSampler2DArray)
                return ActiveUniformType.UnsignedIntSampler2D;
            if (type == ActiveUniformType.UnsignedIntSampler2DMultisampleArray)
                return ActiveUniformType.UnsignedIntSampler2DMultisample;
            if (type == ActiveUniformType.UnsignedIntSamplerCubeMapArray)
                return ActiveUniformType.UnsignedIntSamplerCube;
            if (type <= ActiveUniformType.UnsignedIntVec4 && type >= ActiveUniformType.UnsignedIntVec2 || type == ActiveUniformType.UnsignedInt)
                return ActiveUniformType.UnsignedInt;
            if (type <= ActiveUniformType.FloatMat4 && type >= ActiveUniformType.FloatMat2)
                return ActiveUniformType.Float;
            if (type <= ActiveUniformType.FloatMat4x3 && type >= ActiveUniformType.FloatMat2x3)
                return ActiveUniformType.Float;
            return type;
        }
        public static uint getElementCount(ActiveUniformType type, ActiveUniformType baseType)
        {
            switch (type)
            {
                case ActiveUniformType.FloatMat2:
                    return getElementCount(ActiveUniformType.FloatVec2, ActiveUniformType.Float) * getElementCount(ActiveUniformType.FloatVec2, ActiveUniformType.Float);
                case ActiveUniformType.FloatMat3:
                    return getElementCount(ActiveUniformType.FloatVec3, ActiveUniformType.Float) * getElementCount(ActiveUniformType.FloatVec3, ActiveUniformType.Float);
                case ActiveUniformType.FloatMat4:
                    return getElementCount(ActiveUniformType.FloatVec4, ActiveUniformType.Float) * getElementCount(ActiveUniformType.FloatVec4, ActiveUniformType.Float);
                case ActiveUniformType.FloatMat2x3:
                    return getElementCount(ActiveUniformType.FloatVec2, ActiveUniformType.Float) * getElementCount(ActiveUniformType.FloatVec3, ActiveUniformType.Float);
                case ActiveUniformType.FloatMat2x4:
                    return getElementCount(ActiveUniformType.FloatVec2, ActiveUniformType.Float) * getElementCount(ActiveUniformType.FloatVec4, ActiveUniformType.Float);
                case ActiveUniformType.FloatMat3x2:
                    return getElementCount(ActiveUniformType.FloatVec3, ActiveUniformType.Float) * getElementCount(ActiveUniformType.FloatVec2, ActiveUniformType.Float);
                case ActiveUniformType.FloatMat3x4:
                    return getElementCount(ActiveUniformType.FloatVec3, ActiveUniformType.Float) * getElementCount(ActiveUniformType.FloatVec4, ActiveUniformType.Float);
                case ActiveUniformType.FloatMat4x2:
                    return getElementCount(ActiveUniformType.FloatVec4, ActiveUniformType.Float) * getElementCount(ActiveUniformType.FloatVec2, ActiveUniformType.Float);
                case ActiveUniformType.FloatMat4x3:
                    return getElementCount(ActiveUniformType.FloatVec4, ActiveUniformType.Float) * getElementCount(ActiveUniformType.FloatVec3, ActiveUniformType.Float);
            }
            if (type == baseType)
                return 1;
            switch (baseType)
            {
                case ActiveUniformType.Bool:
                    return (uint)(type - ActiveUniformType.Bool) + 1;
                case ActiveUniformType.Double:
                    return (uint)(type - ActiveUniformType.DoubleVec2) + 2;
                case ActiveUniformType.Float:
                    return (uint)(type - ActiveUniformType.FloatVec2) + 2;
                case ActiveUniformType.Int:
                    return (uint)(type - ActiveUniformType.IntVec2) + 2;
                case ActiveUniformType.UnsignedInt:
                    return (uint)(type - ActiveUniformType.UnsignedIntVec2) + 2;
            }

            return 1;
        }
        public static uint getElementSize(ActiveUniformType type)
        {
            ActiveUniformType baseType = getBaseType(type);
            uint baseSize = getBaseSize(baseType);
            uint elementCount = getElementCount(type, baseType);
            return baseSize * elementCount;
        }
        public static uint getColumnCount(ActiveUniformType type)
        {
            switch (type)
            {
                case ActiveUniformType.FloatMat2:
                    return getElementCount(ActiveUniformType.FloatVec2, ActiveUniformType.Float);
                case ActiveUniformType.FloatMat3:
                    return getElementCount(ActiveUniformType.FloatVec3, ActiveUniformType.Float);
                case ActiveUniformType.FloatMat4:
                    return getElementCount(ActiveUniformType.FloatVec4, ActiveUniformType.Float);
                case ActiveUniformType.FloatMat2x3:
                    return getElementCount(ActiveUniformType.FloatVec2, ActiveUniformType.Float);
                case ActiveUniformType.FloatMat2x4:
                    return getElementCount(ActiveUniformType.FloatVec2, ActiveUniformType.Float);
                case ActiveUniformType.FloatMat3x2:
                    return getElementCount(ActiveUniformType.FloatVec3, ActiveUniformType.Float);
                case ActiveUniformType.FloatMat3x4:
                    return getElementCount(ActiveUniformType.FloatVec3, ActiveUniformType.Float);
                case ActiveUniformType.FloatMat4x2:
                    return getElementCount(ActiveUniformType.FloatVec4, ActiveUniformType.Float);
                case ActiveUniformType.FloatMat4x3:
                    return getElementCount(ActiveUniformType.FloatVec4, ActiveUniformType.Float);
            }
            if (getBaseType(type) == type)
                return 1;
            if (type <= ActiveUniformType.BoolVec4 && type >= ActiveUniformType.BoolVec2)
                return (uint)(type - ActiveUniformType.BoolVec2) + 2;
            if (type <= ActiveUniformType.DoubleVec4 && type >= ActiveUniformType.DoubleVec2)
                return (uint)(type - ActiveUniformType.DoubleVec2) + 2;
            if (type <= ActiveUniformType.FloatVec4 && type >= ActiveUniformType.FloatVec2)
                return (uint)(type - ActiveUniformType.FloatVec2) + 2;
            if (type <= ActiveUniformType.IntVec4 && type >= ActiveUniformType.IntVec2)
                return (uint)(type - ActiveUniformType.IntVec2) + 2;
            if (type <= ActiveUniformType.UnsignedIntVec4 && type >= ActiveUniformType.UnsignedIntVec2)
                return (uint)(type - ActiveUniformType.UnsignedIntVec2) + 2;

            return 1;
        }
    }
}
