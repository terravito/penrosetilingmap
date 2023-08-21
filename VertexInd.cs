using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct VertexInd : IEquatable<VertexInd> 
{
    public int a;
    public int b;
    public int c;
    public int d;
    public int e;

    public VertexInd(int a, int b, int c, int d, int e)
    {
        this.a = a;
        this.b = b;
        this.c = c;
        this.d = d;
        this.e = e;
    }

    public bool Equals(VertexInd other)
    {
        return a == other.a && b == other.b && c == other.c && d == other.d && e == other.e;
    }

    public override string ToString()
    {
        return "(" + a + "," + b + "," + c + "," + d + "," + e + ")";
    }

    public static VertexInd zero = new(0, 0, 0, 0, 0);
}

public class VertexIndEqualityComparer : IEqualityComparer<VertexInd>
{
    public bool Equals(VertexInd vertexIndA, VertexInd vertexIndB)
    {
        return vertexIndA.Equals(vertexIndB);
    }

    public int GetHashCode(VertexInd vertexInd)
    {
        string hash = vertexInd.a + "|" + vertexInd.b + "|" + vertexInd.c + "|" + vertexInd.d + "|" + vertexInd.e;
        return hash.GetHashCode();
    }
}
