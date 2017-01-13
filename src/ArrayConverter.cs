using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrayConverter {
    public static Color ToColor(int[] values)
    {
        if (values.Length == 4)
            return new Color(values[0], values[1], values[2], values[3]);
        else if (values.Length == 3)
            return new Color(values[0], values[1], values[2]);
        throw new System.ArgumentException("Invalid array length");
    }

    public static Matrix4x4 ToMatrix4x4(float[] data)
    {
        if (data.Length != 16)
            throw new System.ArgumentException("Data argument is not a Matrix4x4");
        Matrix4x4 res = new Matrix4x4();
        res.m00 = data[0]; res.m01 = data[4]; res.m02 = data[8]; res.m03 = data[12];
        res.m10 = data[1]; res.m11 = data[5]; res.m12 = data[9]; res.m13 = data[13];
        res.m20 = data[2]; res.m21 = data[6]; res.m22 = data[10]; res.m23 = data[14];
        res.m30 = data[3]; res.m31 = data[7]; res.m32 = data[11]; res.m33 = data[15];
        return res;
    }

    public static Vector2 ToVector2(float[] data)
    {
        if (data.Length != 2)
            throw new System.ArgumentException("Data argument is not a Vector2");
        return new Vector2(data[0], data[1]);
    }

    public static Vector3 ToVector3(float[] data)
    {
        if (data.Length != 3)
            throw new System.ArgumentException("Data argument is not a Vector3");
        return new Vector3(data[0], data[1], data[2]);
    }

    public static Vector4 ToVector4(float[] data)
    {
        if (data.Length != 4)
            throw new System.ArgumentException("Data argument is not a Vector4");
        return new Vector4(data[0], data[1], data[2], data[3]);
    }

    public static Quaternion ToQuaternion(float[] data)
    {
        if (data.Length != 4)
            throw new System.ArgumentException("Data argument is not a Quaternion");
        return new Quaternion(data[0], data[1], data[2], data[3]);
    }
}
