using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Geometry
{
    public static Vector3 RotateByAngleXZ(Vector3 vector, int angle)
    {
        float angleRad = angle * Mathf.Deg2Rad;
        float xResult = vector.x * Mathf.Cos(angleRad) - vector.z * Mathf.Sin(angleRad);
        float zResult = vector.x * Mathf.Sin(angleRad) + vector.z * Mathf.Cos(angleRad);
        return new Vector3(xResult, 0, zResult);
    }

    public static Vector3 PerpendicularXZ(Vector3 vector)
    {
        Vector2 perp = Vector2.Perpendicular(new Vector2(vector.x, vector.z));
        return new Vector3(perp.x, 0, perp.y);
    }

    public static bool PointWithinPolygonXZ(Vector3 point, Vector3[] polygon)
    {
        float angleSum = 0;
        for (int i = 0; i < polygon.Length; i++)
        {
            angleSum += Vector3.Angle(polygon[i] - point, polygon[(i + 1) % polygon.Length] - point);
        }
        return Mathf.RoundToInt(angleSum) == 360;
    }

    public static bool EpsilonEqual(float valueA, float valueB, float epsilon)
    {
        return Mathf.Abs(valueA - valueB) < epsilon;
    }

    public static bool PointIsPolygonVertexXZ(Vector3 point, Vector3[] polygon)
    {
        foreach (Vector3 vector in polygon)
        {
            if (vector == point) 
                return true;
        }
        return false;
    }

    public static Vector3[] PolygonIntersectXZ(Vector3 origin, Vector3 direction, float extent, Vector3[] polygon)
    {
        Vector3[] result = new Vector3[2];
        int accounted = 0;
        for (int i = 0; i < polygon.Length; i++)
        {
            if (accounted == 2) 
                break;
            Vector3 borderDirection = polygon[(i + 1) % polygon.Length] - polygon[i];
            float borderExtent = borderDirection.magnitude / 2;
            borderDirection = borderDirection.normalized;
            Vector3 midpoint = polygon[i] + 0.5f * (polygon[(i + 1) % polygon.Length] - polygon[i]);
            if (Vector3.Dot(direction, borderDirection) == 0 && Vector3.Dot(direction, midpoint - origin) == 0)
            {
                result[0] = midpoint + borderExtent * borderDirection;
                result[1] = midpoint - borderExtent * borderDirection;
                break;
            }
            Vector3 intersection = FindIntersectionXZ(origin, direction, extent, midpoint, borderDirection, borderExtent);
            if (intersection != Vector3.zero)
            {
                result[accounted] = intersection;
                accounted++;
            }
        }
        return result;
    }

    public static Vector3 FindIntersectionXZ(Vector3 originA, Vector3 directionA, float extentA, Vector3 originB, Vector3 directionB, float extentB)
    {
        float parameterA = (directionB.z * (originA.x - originB.x) - directionB.x * (originA.z - originB.z)) / (directionB.x * directionA.z - directionB.z * directionA.x);
        float parameterB = (originA.x - originB.x + directionA.x * parameterA) / directionB.x;
        Vector3 intersection = originA + parameterA * directionA;
        if (Mathf.Abs(parameterA) < extentA && Mathf.Abs(parameterB) < extentB) 
            return intersection;
        return Vector3.zero;
    }

    public static List<Vector3> VectorListXZToRect(List<Vector3> vectorList, Rect rect)
    {
        List<Vector3> result = new();
        if (vectorList.Count == 0)
            return result;
        float boundsHeight = GetVerticalExpanseXZ(vectorList);
        foreach (Vector3 vector in vectorList)
            result.Add(ConvertVectorXZToRect(vector, rect, boundsHeight));
        return result;
    }

    public static Vector3 ConvertVectorXZToRect(Vector3 vector, Rect rect, float boundsHeight)
    {
        float scaleFactor = -rect.height / (boundsHeight + 10f);
        Vector3 rectOrigin = (Vector3)(rect.center - rect.position);
        Vector3 vectorXY = new Vector3(vector.x, vector.z + 0.5f);
        return rectOrigin + scaleFactor * vectorXY;
    }

    public static float GetVerticalExpanseXZ(List<Vector3> vectorList)
    {
        if (vectorList.Count == 0)
            return 0;
        float min = vectorList[0].z;
        float max = vectorList[0].z;
        foreach (Vector3 vector in vectorList)
        {
            if (vector.z < min)
                min = vector.z;
            if (vector.z > max)
                max = vector.z;
        }
        return max - min;
    }

    public static List<Vector3> StripOutUncommonVectorsXZ(List<Vector3> listA, List<Vector3> listB)
    {
        List<Vector3> result = new();
        foreach (Vector3 vectorA in listA)
            foreach (Vector3 vectorB in listB)
                if (EpsilonEqual(vectorA, vectorB, 0.001f) && result.IndexOf(vectorB) == -1)
                    result.Add(vectorB);
        return result;
    }

    public static bool EpsilonEqual(Vector3 vectorA, Vector3 vectorB, float epsilon)
    {
        return (vectorA - vectorB).magnitude < epsilon;
    }

    public static Vector3 PolygonCentroidXZ(List<Vector3> vertices)
    {
        float signedArea = 0;
        for (int i = 0; i < vertices.Count; i++)
            signedArea += vertices[i].x * vertices[(i + 1) % vertices.Count].z - vertices[(i + 1) % vertices.Count].x * vertices[i].z;
        signedArea /= 2;
        float centroidX = 0;
        float centroidZ = 0;
        for (int i = 0; i < vertices.Count; i++)
        {
            centroidX += (vertices[i].x + vertices[(i + 1) % vertices.Count].x) * (vertices[i].x * vertices[(i + 1) % vertices.Count].z - vertices[(i + 1) % vertices.Count].x * vertices[i].z);
            centroidZ += (vertices[i].z + vertices[(i + 1) % vertices.Count].z) * (vertices[i].x * vertices[(i + 1) % vertices.Count].z - vertices[(i + 1) % vertices.Count].x * vertices[i].z);
        }
        centroidX /= 6 * signedArea;
        centroidZ /= 6 * signedArea;
        return new Vector3(centroidX, 0, centroidZ);
    }
}
