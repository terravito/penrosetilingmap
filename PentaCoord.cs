using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public struct PentaCoord : IEquatable<PentaCoord>
{
    public int u;
    public int v;
    public Pentagrid pentagrid;

    public PentaCoord(int a, int b, Pentagrid pentagrid)
    {
        u = a;
        v = b;
        this.pentagrid = pentagrid;
    }

    public PentaCoord Arranged()
    {
        TileType tileType = PenroseTiles.DetermineType(u, v, pentagrid);
        int min = Mathf.Min(new int[] { u, v });
        int max = Mathf.Max(new int[] { u, v });
        int gridDensity = pentagrid.GetGridDensity();
        int a;
        int b;
        if (tileType == TileType.ThinRhomb && (max >= 3 * gridDensity && min < 1 * gridDensity || max >= 4 * gridDensity && min < 2 * gridDensity))
        {
            a = max;
            b = min;
            return new PentaCoord(a, b, pentagrid);
        }
        if (tileType == TileType.ThickRhomb && max >= 4 * gridDensity && min < 1 * gridDensity)
        {
            a = max;
            b = min;
            return new PentaCoord(a, b, pentagrid);
        }
        a = min;
        b = max;
        return new PentaCoord(a, b, pentagrid);
    }

    public PentaCoord Inverted()
    {
        return new PentaCoord(v, u, pentagrid);
    }

    public bool Equals(PentaCoord other)
    {
        return u == other.u && v == other.v;
    }

    public override string ToString()
    {
        return "(" + u + ", " + v + ")";
    }

    public static PentaCoord zero = new (0,0, TilingController.GetTiling().GetPentagrid());
}

public class PentaCoordEqualityComparer : IEqualityComparer<PentaCoord>
{
    public bool Equals(PentaCoord pentaCoordA, PentaCoord pentaCoordB)
    {
        return pentaCoordA.Equals(pentaCoordB) || pentaCoordA.Equals(pentaCoordB.Inverted());
    }

    public int GetHashCode(PentaCoord pentaCoord)
    {
        string hash = pentaCoord.u + "|" + pentaCoord.v;
        return hash.GetHashCode();
    }
}
