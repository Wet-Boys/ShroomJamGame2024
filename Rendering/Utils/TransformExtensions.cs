using System.Numerics;
using Godot;

namespace ShroomJamGame.Rendering.Utils;

public static class TransformExtensions
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Get Model Matrix in Godot's ordering
    /// </summary>
    /// <returns></returns>
    public static Matrix4x4 GetModelMatrix(this Node3D node)
    {
        var pos = node.Position;
        var rot = node.Quaternion;
        var scale = node.Scale;

        var translationMat = new Matrix4x4(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            pos.X, pos.Y, pos.Z, 1
        );
        
        var num1 = 2f / rot.LengthSquared();
        var num2 = rot.X * num1;
        var num3 = rot.Y * num1;
        var num4 = rot.Z * num1;
        var num5 = rot.W * num2;
        var num6 = rot.W * num3;
        var num7 = rot.W * num4;
        var num8 = rot.X * num2;
        var num9 = rot.X * num3;
        var num10 = rot.X * num4;
        var num11 = rot.Y * num3;
        var num12 = rot.Y * num4;
        var num13 = rot.Z * num4;

        // var rotMat = new Matrix4x4(
        //     (float) (1.0 - (num11 + (double) num13)), num9 - num7, num10 + num6, 0,
        //     num9 + num7, (float) (1.0 - (num8 + (double) num13)), num12 - num5, 0,
        //     num10 - num6, num12 + num5, (float) (1.0 - (num8 + (double) num11)), 0,
        //     0, 0, 0, 1
        // );
        
        var rotMat = new Matrix4x4(
            (float) (1.0 - (num11 + (double) num13)), num9 + num7, num10 - num6, 0,
            num9 - num7, (float) (1.0 - (num8 + (double) num13)), num12 + num5, 0,
            num10 + num6, num12 - num5, (float) (1.0 - (num8 + (double) num11)), 0,
            0, 0, 0, 1
        );

        var scaleMat = new Matrix4x4(
            scale.X, 0, 0, 0,
            0, scale.Y, 0, 0,
            0, 0, scale.Z, 0,
            0, 0, 0, 1
        );
        
        return translationMat * rotMat * scaleMat;
    }
    
    // ReSharper disable once InconsistentNaming
    public static Matrix4x4 AsMatrix4x4(this Projection projection)
    {
        return new Matrix4x4(
            projection.X.X, projection.X.Y, projection.X.Z, projection.X.W,
            projection.Y.X, projection.Y.Y, projection.Y.Z, projection.Y.W,
            projection.Z.X, projection.Z.Y, projection.Z.Z, projection.Z.W,
            projection.W.X, projection.W.Y, projection.W.Z, projection.W.W
        );
    }
}