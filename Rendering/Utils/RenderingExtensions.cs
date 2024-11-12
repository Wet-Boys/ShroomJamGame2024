using System;
using System.Linq;
using System.Numerics;
using Godot;
using Vector3 = Godot.Vector3;

namespace ShroomJamGame.Rendering.Utils;

public static class RenderingExtensions
{
    public static Rid IndexBufferCreate(this RenderingDevice renderingDevice, uint[] indices)
    {
        var data = indices.ToByteArray();
        return renderingDevice.IndexBufferCreate((uint)indices.Length, RenderingDevice.IndexBufferFormat.Uint32, data);
    }

    public static (Rid indexBuffer, Rid indexArray) IndexArrayCreate(this RenderingDevice renderingDevice, int[] indices, uint indexOffset = 0)
    {
        var uintIndices = indices.Select(i => (uint)i).ToArray();
        return renderingDevice.IndexArrayCreate(uintIndices, indexOffset);
    }

    public static (Rid indexBuffer, Rid indexArray) IndexArrayCreate(this RenderingDevice renderingDevice, uint[] indices, uint indexOffset = 0)
    {
        var indexBuffer = renderingDevice.IndexBufferCreate(indices);
        if (!indexBuffer.IsValid)
            throw new InvalidOperationException("Index buffer is invalid!");
        
        var indexArray = renderingDevice.IndexArrayCreate(indexBuffer, indexOffset, (uint)indices.Length);
        if (!indexArray.IsValid)
            throw new InvalidOperationException("Index array is invalid!");
        
        return (indexBuffer, indexArray);
    }
    
    public static Rid VertexBufferCreate(this RenderingDevice renderingDevice, Vector3[] vertices)
    {
        var data = vertices.ToByteArray();
        return renderingDevice.VertexBufferCreate((uint)data.Length, data);
    }
    
    public static (Rid vertexBuffer, Rid vertexArray) VertexArrayCreate(this RenderingDevice renderingDevice, Vector3[] vertices, long vertexFormat)
    {
        var vertexBuffer = renderingDevice.VertexBufferCreate(vertices);
        if (!vertexBuffer.IsValid)
            throw new InvalidOperationException("Vertex buffer is invalid!");
        
        var vertexArray = renderingDevice.VertexArrayCreate((uint)vertices.Length, vertexFormat, [vertexBuffer]);
        if (!vertexArray.IsValid)
            throw new InvalidOperationException("Vertex array is invalid!");
        
        return (vertexBuffer, vertexArray);
    }
    
    public static void ComputeListSetPushConstant(this RenderingDevice renderingDevice, long computeList, params byte[][] pushConstants)
    {
        var data = pushConstants.SelectMany(data => data).ToArray();
        renderingDevice.ComputeListSetPushConstant(computeList, data, (uint)data.Length);
    }
    
    public static void DrawListSetPushConstant(this RenderingDevice renderingDevice, long drawList, params byte[][] pushConstants)
    {
        var data = pushConstants.SelectMany(data => data).ToArray();
        if (data.Length % 16 != 0)
        {
            var remainingLength = 16 - data.Length % 16;
            var padding = new byte[remainingLength];
            
            data = data.Concat(padding).ToArray();
        }
        
        renderingDevice.DrawListSetPushConstant(drawList, data, (uint)data.Length);
    }
    
    public static Rid UniformBufferCreate(this RenderingDevice renderingDevice, Projection projection)
    {
        return renderingDevice.UniformBufferCreate(projection.AsMatrix4x4());
    }
    
    public static Rid UniformBufferCreate(this RenderingDevice renderingDevice, Matrix4x4 matrix)
    {
        var data = matrix.ToByteArray();
        return renderingDevice.UniformBufferCreate((uint)data.Length, data);
    }

    public static Rid UniformBufferCreate(this RenderingDevice renderingDevice, params byte[][] uniformData)
    {
        var data = uniformData.SelectMany(data => data).ToArray();
        if (data.Length % 16 != 0)
        {
            var remainingLength = 16 - data.Length % 16;
            var padding = new byte[remainingLength];
            
            data = data.Concat(padding).ToArray();
        }
        
        return renderingDevice.UniformBufferCreate((uint)data.Length, data);
    }
    
    public static byte[] ToByteArray(this Vector3[] vectors) => vectors.ToFloatArray().ToByteArray();

    public static byte[] ToByteArray(this Matrix4x4 matrix) => matrix.ToFloatArray().ToByteArray();
    
    public static byte[] ToByteArray(this Projection projection) => projection.AsMatrix4x4().ToByteArray();
    
    public static byte[] ToByteArray(this Color color) => color.ToFloatArray().ToByteArray();
    
    public static byte[] ToByteArray(this double[] values)
    {
        var bytes = new byte[values.Length * sizeof(double)];

        for (int i = 0; i < bytes.Length; i += 8)
        {
            var doubleBytes = BitConverter.GetBytes(values[i / 8]);
            
            bytes[i] = doubleBytes[0];
            bytes[i + 1] = doubleBytes[1];
            bytes[i + 2] = doubleBytes[2];
            bytes[i + 3] = doubleBytes[3];
            bytes[i + 4] = doubleBytes[4];
            bytes[i + 5] = doubleBytes[5];
            bytes[i + 6] = doubleBytes[6];
            bytes[i + 7] = doubleBytes[7];
        }
        
        return bytes;
    }

    public static byte[] ToByteArray(this float[] values)
    {
        var bytes = new byte[values.Length * sizeof(float)];

        for (int i = 0; i < bytes.Length; i += 4)
        {
            var floatBytes = BitConverter.GetBytes(values[i / 4]);
            
            bytes[i] = floatBytes[0];
            bytes[i + 1] = floatBytes[1];
            bytes[i + 2] = floatBytes[2];
            bytes[i + 3] = floatBytes[3];
        }
        
        return bytes;
    }

    public static byte[] ToByteArray(this uint[] values)
    {
        var bytes = new byte[values.Length * sizeof(int)];

        for (int i = 0; i < bytes.Length; i += 4)
        {
            var intBytes = BitConverter.GetBytes(values[i / 4]);
            
            bytes[i] = intBytes[0];
            bytes[i + 1] = intBytes[1];
            bytes[i + 2] = intBytes[2];
            bytes[i + 3] = intBytes[3];
        }
        
        return bytes;
    }

    public static float[] ToFloatArray(this Vector3[] vectors) => vectors.Select(ToFloatArray).SelectMany(f => f).ToArray();

    public static float[] ToFloatArray(this Vector3 vector) => [vector.X, vector.Y, vector.Z];

    public static float[] ToFloatArray(this Matrix4x4 matrix)
    {
        return [
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
            matrix.M41, matrix.M42, matrix.M43, matrix.M44
        ];
    }

    public static float[] ToFloatArray(this Color color) => [color.R, color.G, color.B, color.A];
}