using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum TileType
{
    BorderTile,
    ThinRhomb,
    ThickRhomb
};

public class PenroseTiles
{
    public static float thickRhombWidth = Mathf.Cos(36 * Mathf.Deg2Rad);
    public static float thickRhombHeight = Mathf.Sin(36 * Mathf.Deg2Rad);
    public static float thinRhombWidth = Mathf.Cos(18 * Mathf.Deg2Rad);
    public static float thinRhombHeight = Mathf.Sin(18 * Mathf.Deg2Rad);

    public static int DetermineRotation(PentaCoord pentaCoord, Pentagrid pentagrid)
    {
        float degree = DetermineDegree(pentaCoord, pentagrid);
        int uGrid = pentaCoord.u / pentagrid.GetGridDensity();
        switch (degree)
        {
            case 0:
                return 0;
            case 1:
                return ParallelToGridVector((5 + uGrid - 2) % 5, pentagrid);
            case 2:
                return PerpendicularToGridVector((5 + uGrid - 4) % 5, pentagrid);
            default:
                throw new IndexOutOfRangeException("Invalid IntersectionCoefficient in DetermineRotation method.");
        }
    }

    public static int ParallelToGridVector(int i, Pentagrid pentagrid)
    {
        Vector3 gridVector;
        try
        {
            gridVector = pentagrid.GetGridVector(i);
        }
        catch (IndexOutOfRangeException e)
        {
            Console.WriteLine(e.Message);
            throw new IndexOutOfRangeException("Attempted to access gridVector " + i + " in ParallelToGridVector method.");
        }
        float arctanAngle = Mathf.Atan2(gridVector.z, gridVector.x) * Mathf.Rad2Deg;
        if (arctanAngle >= 0)
        {
            return Mathf.RoundToInt(arctanAngle);
        }
        else
        {
            return Mathf.RoundToInt(360 + arctanAngle);
        }
    }

    public static int PerpendicularToGridVector(int i, Pentagrid pentagrid)
    {
        Vector3 gridVector;
        try
        {
            gridVector = pentagrid.GetGridVector(i);
        }
        catch (IndexOutOfRangeException e)
        {
            Console.WriteLine(e.Message);
            throw new IndexOutOfRangeException("Attempted to access gridVector " + i + " in PerpendicularToGridVector method.");
        }
        float arctanAngle = Mathf.Atan2(gridVector.z, gridVector.x) * Mathf.Rad2Deg;
        if (arctanAngle >= 0)
        {
            return Mathf.RoundToInt(arctanAngle + 90);
        }
        else
        {
            return Mathf.RoundToInt((360 + arctanAngle + 90) % 360);
        }
    }

    public static float DetermineDegree(PentaCoord pentaCoord, Pentagrid pentagrid)
    {
        int gridLineCount = pentagrid.GetGridDensity();
        int digitA = pentaCoord.u / gridLineCount;
        int digitB = pentaCoord.v / gridLineCount;
        return -Mathf.Abs(Mathf.Abs(digitA - digitB) - 2.5f) + 2.5f;
    }

    public static float DetermineDegree(int a, int b, Pentagrid pentagrid)
    {
        int gridLineCount = pentagrid.GetGridDensity();
        int digitA = a / gridLineCount;
        int digitB = b / gridLineCount;
        return -Mathf.Abs(Mathf.Abs(digitA - digitB) - 2.5f) + 2.5f;
    }

    public static TileType DetermineType(PentaCoord pentaCoord, Pentagrid pentagrid)
    {
        float degree = DetermineDegree(pentaCoord, pentagrid);
        switch (degree)
        {
            case 0:
                return TileType.BorderTile;
            case 1:
                return TileType.ThickRhomb;
            case 2:
                return TileType.ThinRhomb;
            default:
                throw new IndexOutOfRangeException("Invalid IntersectionDegree in DetermineType method.");
        }
    }

    public static TileType DetermineType(int a, int b, Pentagrid pentagrid)
    {
        float degree = DetermineDegree(a, b, pentagrid);
        switch (degree)
        {
            case 0:
                return TileType.BorderTile;
            case 1:
                return TileType.ThickRhomb;
            case 2:
                return TileType.ThinRhomb;
            default:
                throw new IndexOutOfRangeException("Invalid IntersectionDegree in DetermineType method.");
        }
    }

    //public static bool PentaCoordEqual(Vector2Int pentaCoordA, Vector2Int pentaCoordB)
    //{
    //    bool same = pentaCoordA == pentaCoordB;
    //    bool opposite = (pentaCoordA.x == pentaCoordB.y) && (pentaCoordA.y == pentaCoordB.x);
    //    return same || opposite;
    //}

    public static int DetermineOrder(PentaCoord pentaCoord, Pentagrid pentagrid)
    {
        switch (DetermineDegree(pentaCoord, pentagrid))
        {
            case 1:
                return (pentaCoord.u / pentagrid.GetGridDensity()) % 5;
            case 2:
                return (pentaCoord.u / pentagrid.GetGridDensity() + 1) % 5;
            default:
                throw new IndexOutOfRangeException("Invalid IntersectionDegree in DetermineParity method.");
        }
    }

    public static Vector3 DetermineRelativePositionOf(PentaCoord adjacency, PentaCoord pentaCoord, Vector3 position, List<PentaCoord> adjacencies, Pentagrid pentagrid)
    {
        float distanceTo = DetermineDistanceBetween(adjacency, pentaCoord, pentagrid);
        int angularOffsetTo = DetermineAngularOffsetTo(adjacency, pentaCoord, DetermineReferenceAngleOf(adjacency, pentaCoord, pentagrid), adjacencies, pentagrid);
        Vector3 displacementVector = DetermineDisplacementVectorTo(distanceTo, angularOffsetTo, pentaCoord, pentagrid);
        return position + displacementVector;
    }

    public static List<Vector3> DetermineVerticesOfAdjacency(PentaCoord adjacency, PentaCoord pentaCoord, Vector3 position, List<PentaCoord> adjacencies, Pentagrid pentagrid)
    {
        List<Vector3> result = new();
        TileType specType = DetermineType(adjacency, pentagrid);
        float rhombWidth;
        float rhombHeight;
        switch (specType)
        {
            case TileType.ThinRhomb:
                rhombWidth = thinRhombWidth;
                rhombHeight = thinRhombHeight;
                break;
            case TileType.ThickRhomb:
                rhombWidth = thickRhombWidth;
                rhombHeight = thickRhombHeight;
                break;
            default:
                throw new IndexOutOfRangeException("Attempting to access TileType which does not exist within DetermineVerticesOfAdjacency method.");
        }
        Vector3 specOrigin = DetermineRelativePositionOf(adjacency, pentaCoord, position, adjacencies, pentagrid);
        int specRot = DetermineRotation(adjacency, pentagrid);
        result.Add(specOrigin + rhombWidth * Geometry.RotateByAngleXZ(Vector3.right, specRot));
        result.Add(specOrigin + rhombHeight * Geometry.RotateByAngleXZ(Vector3.forward, specRot));
        result.Add(specOrigin + rhombWidth * Geometry.RotateByAngleXZ(Vector3.left, specRot));
        result.Add(specOrigin + rhombHeight * Geometry.RotateByAngleXZ(Vector3.back, specRot));
        return result;
    }

    public static List<Vector3> DetermineRawVerticesOf(PentaCoord pentaCoord, Pentagrid pentagrid)
    {
        List<Vector3> result = new();
        TileType specType = DetermineType(pentaCoord, pentagrid);
        float rhombWidth;
        float rhombHeight;
        switch (specType)
        {
            case TileType.ThinRhomb:
                rhombWidth = thinRhombWidth;
                rhombHeight = thinRhombHeight;
                break;
            case TileType.ThickRhomb:
                rhombWidth = thickRhombWidth;
                rhombHeight = thickRhombHeight;
                break;
            default:
                throw new IndexOutOfRangeException("Attempting to access TileType which does not exist within DetermineRawVertices method.");
        }
        Vector3 specOrigin = Vector3.zero;
        int specRot = DetermineRotation(pentaCoord, pentagrid);
        result.Add(specOrigin + rhombWidth * Geometry.RotateByAngleXZ(Vector3.right, specRot));
        result.Add(specOrigin + rhombHeight * Geometry.RotateByAngleXZ(Vector3.forward, specRot));
        result.Add(specOrigin + rhombWidth * Geometry.RotateByAngleXZ(Vector3.left, specRot));
        result.Add(specOrigin + rhombHeight * Geometry.RotateByAngleXZ(Vector3.back, specRot));
        return result;
    }

    public static float DetermineDistanceBetween(PentaCoord adjacency, PentaCoord pentaCoord, Pentagrid pentagrid)
    {
        TileType specType = DetermineType(adjacency, pentagrid);
        float specRot = DetermineRotation(adjacency, pentagrid);
        float rotationMod = (360 + specRot - DetermineRotation(pentaCoord, pentagrid)) % 360;
        switch (DetermineType(pentaCoord, pentagrid))
        {
            case TileType.ThinRhomb:
                if (specType == TileType.ThinRhomb)
                    return thinRhombWidth * Mathf.Sqrt(2 - 2 * Mathf.Cos(36 * Mathf.Deg2Rad));
                if (rotationMod <= 180)
                    return thickRhombWidth;
                else
                    return thinRhombWidth;
            case TileType.ThickRhomb:
                if (specType == TileType.ThickRhomb)
                    return thickRhombWidth * Mathf.Sqrt(2 - 2 * Mathf.Cos(72 * Mathf.Deg2Rad));
                if (rotationMod <= 180)
                    return thinRhombWidth;
                else
                    return thickRhombWidth;
            default:
                throw new IndexOutOfRangeException("Attempting to access invalid TileType within DetermineDistanceBetween method.");
        }
    }

    public static int DetermineReferenceAngleOf(PentaCoord adjacency, PentaCoord pentaCoord, Pentagrid pentagrid)
    {
        TileType specType = DetermineType(adjacency, pentagrid);
        int specRot = DetermineRotation(adjacency, pentagrid);
        int rotationMod = (360 + specRot - DetermineRotation(pentaCoord, pentagrid)) % 360;
        switch (specType)
        {
            case TileType.ThinRhomb:
                if (specType == DetermineType(pentaCoord, pentagrid) || rotationMod > 180)
                    return 72;
                else
                    return 18;
            case TileType.ThickRhomb:
                if (specType == DetermineType(pentaCoord, pentagrid) || rotationMod <= 180)
                    return 54;
                else
                    return 36;
            default:
                throw new IndexOutOfRangeException("Attempting to access invalid TileType within DetermineReferenceAngleOf method.");
        }
    }

    public static int DetermineAngularOffsetTo(PentaCoord adjacency, PentaCoord pentaCoord, int referenceAngle, List<PentaCoord> adjacencies, Pentagrid pentagrid)
    {
        int directionIndex = adjacencies.IndexOf(adjacency);
        if (directionIndex == -1)
            throw new IndexOutOfRangeException("Intersection point " + adjacency.ToString() + " was not found in adjacency list of " + pentaCoord.ToString());
        if (DetermineType(pentaCoord, pentagrid) == TileType.ThinRhomb)
            directionIndex = (directionIndex + 1) % 4;
        switch (directionIndex)
        {
            case 0:
                return referenceAngle;
            case 1:
                return 180 - referenceAngle;
            case 2:
                return 180 + referenceAngle;
            case 3:
                return 360 - referenceAngle;
            default:
                throw new IndexOutOfRangeException("Determined inconsistent direction index = " + directionIndex);
        }
    }

    public static Vector3 DetermineDisplacementVectorTo(float distance, int angularOffset, PentaCoord pentaCoord, Pentagrid pentagrid)
    {
        return distance * Geometry.RotateByAngleXZ(Vector3.right, DetermineRotation(pentaCoord, pentagrid) + angularOffset);
    }
}
