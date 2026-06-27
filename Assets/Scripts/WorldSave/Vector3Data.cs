using System;
using UnityEngine;

[Serializable]
public struct Vector3Data
{
    public float X, Y, Z;

    public Vector3Data(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Vector3Data(Vector3 vector3)
    {
        X = vector3.x;
        Y = vector3.y;
        Z = vector3.z;
    }

    public Vector3 Vector3()
    {
        return new Vector3(X, Y, Z);
    }

    public static Vector3Data Zero()
    {
        return new Vector3Data(0f, 0f, 0f);
    }
}