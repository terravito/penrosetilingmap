using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Pentagrid", menuName = "ScriptableObjects/Utilities/Pentagrid", order = 1)]
public class Pentagrid : ScriptableObject
{
    public TileSpawner tileSpawner;

    [Header("Grid Options")]
    [Space]
    [Range(1, 50)] public int gridDensity = 10;
    [Range(1, 50)] public int gridExpanse = 1;
    [Range(0.2f, 2f)] public float gridOffset = 0.2f;
    [Range(0, 99)] public int highlightLine = 0;

    [Header("Serialized Fields")]
    [Space]
    [SerializeField] private int numIntersections = 0;
    [SerializeField] private List<PentaCoord> originIntersections;

    [Header("Events")]
    [Space]
    public UnityEvent Start;
    public UnityEvent EditorUpdate;

    private float gridSpacing = 1f;
    private float gridRadius = 1f;
    private float gridExtent = 10;
    private int gridTurnover = 5;

    private Vector3[] gridVectors = null;
    private Vector3[,] gridLineOrigins = null;
    private Vector3[] playAreaCorners = null;
    private Dictionary<PentaCoord, List<PentaCoord>> adjacencyTable;
    private Dictionary<PentaCoord, Vector3> intersectionLookup;

    private List<Vector3> vertexBuffer;
    private List<Color> colorBuffer;

    private int lastGridDensity;
    private int lastGridSize;
    private float lastGridOffset;
    private bool pentagridDataHasChanged;
    private int lastHighlightLine;

    public void OnStart()
    {
        vertexBuffer = new();
        colorBuffer = new();
        UpdatePentagridData();
    }

    public void OnEditorUpdate()
    {
        pentagridDataHasChanged = false;
        if (gridDensity != lastGridDensity || gridExpanse != lastGridSize || gridOffset != lastGridOffset)
        {
            pentagridDataHasChanged = true;
            UpdatePentagridData();
        }
        lastGridDensity = gridDensity;
        lastGridSize = gridExpanse;
        lastGridOffset = gridOffset;
        if (highlightLine != lastHighlightLine)
            GraphData();
        lastHighlightLine = highlightLine;
    }

    public bool HasChanged()
    {
        return pentagridDataHasChanged;
    }

    public void UpdatePentagridData()
    {
        ComputeGridVectors();
        ComputeGridLineOrigins();
        ComputePlayAreaCorners();
        InitializeAdjacencyMatrix();
        CountIntersections();
        GraphData();
    }

    public bool HasPentagridData()
    {
        return adjacencyTable != null;
    }

    void CountIntersections()
    {
        numIntersections = adjacencyTable.Count / 2;
        if (numIntersections == 0)
            originIntersections = new();
    }

    void ComputeGridVectors()
    {
        gridVectors = new Vector3[5];
        gridVectors[0] = new Vector3(0, 0, 1);
        for (int i = 1; i < 5; i++)
        {
            gridVectors[i] = Geometry.RotateByAngleXZ(gridVectors[i - 1], 72);
        }
    }

    void ComputeGridLineOrigins()
    {
        gridLineOrigins = new Vector3[5, gridDensity]; 
        gridRadius = gridExpanse / (2 * Mathf.Cos(54 * Mathf.Deg2Rad));
        gridSpacing = 2 * gridRadius / gridDensity;
        gridTurnover = Mathf.CeilToInt((gridRadius - gridOffset) / gridSpacing);
        for (int i = 0; i < 5; i++)
        {
            Vector3 gridLineOffset = gridOffset * gridVectors[i];
            for (int j = 0; j < gridDensity; j++)
            {
                if (j < gridTurnover)
                    gridLineOrigins[i, j] = gridLineOffset + j * gridSpacing * gridVectors[i];
                else
                    gridLineOrigins[i, j] = gridLineOffset - (j - gridTurnover + 1) * gridSpacing * gridVectors[i];
                continue;
            }
        }
    }

    void ComputePlayAreaCorners()
    {
        playAreaCorners = new Vector3[5];
        for (int i = 0; i < 5; i++)
        {
            playAreaCorners[i] = gridRadius * Geometry.RotateByAngleXZ(gridVectors[i], 36);
        }
    }

    Vector3 FindIntersection(int i, int j, int k, int l)
    {
        Vector3 directionA = Geometry.PerpendicularXZ(gridVectors[i]);
        Vector3 directionB = Geometry.PerpendicularXZ(gridVectors[k]);
        float parameterA = (directionB.z * (gridLineOrigins[i, j].x - gridLineOrigins[k, l].x) - directionB.x * (gridLineOrigins[i, j].z - gridLineOrigins[k, l].z)) / (directionB.x * directionA.z - directionB.z * directionA.x);
        Vector3 intersection = gridLineOrigins[i, j] + parameterA * directionA;
        return intersection;
    }

    List<PentaCoord> ListIntersections(int i, int j)
    {
        List<PentaCoord> intersections = new();
        for (int k = 0; k < 5; k++)
        {
            if (k == i) 
                continue;
            for (int l = 0; l < gridDensity; l++)
            {
                Vector3 intersection = FindIntersection(i, j, k, l);
                DistinguishOrigin(i, j, k, l, intersection.sqrMagnitude);
                if (PointWithinPlayArea(intersection))
                {
                    PentaCoord pentaCoord = new PentaCoord(i * gridDensity + j, k * gridDensity + l, this);
                    intersections.Add(pentaCoord);
                    intersectionLookup.Add(pentaCoord, intersection);
                }
            }
        }
        return intersections;
    }

    bool PointWithinPlayArea(Vector3 point)
    {
        return Geometry.PointWithinPolygonXZ(point, playAreaCorners);
    }

    void DistinguishOrigin(int i, int j, int k, int l, float sqrMagnitude)
    {
        if (sqrMagnitude == 0) 
            return;
        if (originIntersections.Count < i + 1)
        {
            PentaCoord pentaCoord = new PentaCoord(i * gridDensity + j, k * gridDensity + l, this);
            originIntersections.Add(pentaCoord);
            return;
        }
        int m = originIntersections[i].u / gridDensity;
        int n = originIntersections[i].u % gridDensity;
        int o = originIntersections[i].v / gridDensity;
        int p = originIntersections[i].v % gridDensity;
        float prevSqrMagnitude = FindIntersection(m, n, o, p).sqrMagnitude;
        if (sqrMagnitude < prevSqrMagnitude) 
        {
            PentaCoord pentaCoord = new PentaCoord(i * gridDensity + j, k * gridDensity + l, this);
            if (!OriginIntersectionAlreadyAccounted(pentaCoord)) 
                originIntersections[i] = pentaCoord.Arranged(); 
        }
    }

    bool OriginIntersectionAlreadyAccounted(PentaCoord pentaCoord)
    {
        if (originIntersections.IndexOf(pentaCoord) == -1)
            return false;
        return true;
    }

    List<PentaCoord> SortIntersectionsFromEndpoint(List<PentaCoord> list, int leftIndex, int rightIndex, Vector3 endpoint)
    {
        int i = leftIndex;
        int j = rightIndex;
        float pivotDistance = DistanceFromEndpointSqr(list[leftIndex], endpoint);

        while (i <= j)
        {
            while (DistanceFromEndpointSqr(list[i], endpoint) < pivotDistance)
                i++;

            while (DistanceFromEndpointSqr(list[j], endpoint) > pivotDistance)
                j--;

            if (i <= j)
            {
                PentaCoord temp = list[i];
                list[i] = list[j];
                list[j] = temp;
                i++;
                j--;
            }
        }

        if (leftIndex < j)
            SortIntersectionsFromEndpoint(list, leftIndex, j, endpoint);

        if (i < rightIndex)
            SortIntersectionsFromEndpoint(list, i, rightIndex, endpoint);

        return list;
    }

    float DistanceFromEndpointSqr(PentaCoord gridSpaceIntersection, Vector3 endpoint)
    {
        int i = gridSpaceIntersection.u / gridDensity;
        int j = gridSpaceIntersection.u % gridDensity;
        int k = gridSpaceIntersection.v / gridDensity;
        int l = gridSpaceIntersection.v % gridDensity;
        Vector3 intersection = FindIntersection(i, j, k , l);
        return (intersection - endpoint).sqrMagnitude;
    }

    void InitializeAdjacencyMatrix()
    {
        adjacencyTable = new(new PentaCoordEqualityComparer());
        intersectionLookup = new(new PentaCoordEqualityComparer());
        originIntersections = new();
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < gridDensity; j++)
            {
                List<PentaCoord> intersectionList = ListIntersections(i, j);
                if (intersectionList.Count == 0) 
                    continue;
                Vector3 endpoint = gridLineOrigins[i, j] - Geometry.PerpendicularXZ(gridVectors[i]) * gridExtent;
                intersectionList = SortIntersectionsFromEndpoint(intersectionList, 0, intersectionList.Count - 1, endpoint);
                AttachAdjacencies(intersectionList);
            }
        }
    }

    void AttachAdjacencies(List<PentaCoord> intersectionArray)
    {
        for (int i = 0; i < intersectionArray.Count; i++)
        {
            if (i != 0) 
                AttachAdjacency(intersectionArray[i - 1], intersectionArray[i]);
            if (i == 0)
                AttachAdjacency(PentaCoord.zero, intersectionArray[i]);
            if (i == 0) 
                AttachAdjacency(intersectionArray[i + 1], intersectionArray[i]);
            if (i != intersectionArray.Count - 1 && i != 0) 
                AttachAdjacency(intersectionArray[i + 1], intersectionArray[i]);
            if (i == intersectionArray.Count - 1)
                AttachAdjacency(PentaCoord.zero, intersectionArray[i]);
        }
    }

    void AttachAdjacency(PentaCoord adjacency, PentaCoord pentaCoord)
    {
        if (!adjacencyTable.ContainsKey(pentaCoord))
            adjacencyTable.Add(pentaCoord, new());
        List<PentaCoord> adjacencies = adjacencyTable[pentaCoord];
        adjacencies.Add(adjacency);
        if (adjacencies.Count > 2)
            throw new IndexOutOfRangeException("Attempted to attach more than 2 adjacencies to pentaCoord " + pentaCoord.ToString());
    }

    public void GraphData()
    {
        vertexBuffer = new();
        colorBuffer = new();
        if (gridVectors != null && gridLineOrigins != null)
        {
            if (true)
            {
                gridExtent = 2 * gridRadius;
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < gridDensity; j++)
                    {
                        Vector3[] lineBounds = Geometry.PolygonIntersectXZ(gridLineOrigins[i, j], Geometry.PerpendicularXZ(gridVectors[i]), gridExtent, playAreaCorners);
                        if (lineBounds[0] != Vector3.zero && lineBounds[1] != Vector3.zero)
                        {
                            if (i * gridDensity + j != (highlightLine % (5 * gridDensity)))
                                colorBuffer.Add(ColorSelector(i));
                            else
                                colorBuffer.Add(Color.cyan);
                            vertexBuffer.Add(lineBounds[0]);
                            vertexBuffer.Add(lineBounds[1]);
                        }                            
                    }
                }
            }
            if (true)
            {
                for (int i = 0; i < 5; i++)
                {
                    colorBuffer.Add(Color.white);
                    vertexBuffer.Add(playAreaCorners[i]);
                    vertexBuffer.Add(playAreaCorners[(i + 1) % 5]);
                }
            }
        }
    }

    Color ColorSelector(int i)
    {
        switch (i)
        {
            case 0:
                return Color.blue;
            case 1:
                return Color.red;
            case 2:
                return Color.yellow;
            case 3:
                return Color.green;
            case 4:
                return Color.magenta;
            default:
                return Color.black;
        }
    }

    public List<PentaCoord> GetAdjacencies(PentaCoord pentaCoord)
    {
        List<PentaCoord> adjacencies = new();
        for (int k = 0; k < 2; k++)
        {
            adjacencies.Add(adjacencyTable[pentaCoord][k].Arranged());
            adjacencies.Add(adjacencyTable[pentaCoord.Inverted()][k].Arranged());
        }
        return adjacencies;
    }

    public Vector3 GetGridVector(int i)
    {
        return gridVectors[i];
    }

    public int GetGridDensity()
    {
        return gridDensity;
    }

    public int GetGridExpanse()
    {
        return gridExpanse;
    }

    public float GetGridOffset()
    {
        return gridOffset;
    }

    public int GetGridTurnover()
    {
        return gridTurnover;
    }

    public int GetNumIntersections()
    {
        return numIntersections;
    }

    public PentaCoord GetRandomOriginIntersection()
    {
        return originIntersections[UnityEngine.Random.Range(0, 5)];
    }

    public PentaCoord GetOriginIntersection(int i)
    {
        return originIntersections[i];
    }

    public List<PentaCoord> GetOriginIntersections()
    {
        return originIntersections;
    }

    public List<Vector3> GetVertexBuffer()
    {
        return vertexBuffer;
    }

    public List<Color> GetColorBuffer()
    {
        return colorBuffer;
    }

    public VertexInd DetermineVertexIndOfFace(List<PentaCoord> cycle)
    {
        List<Vector3> intersections = new();
        for (int i = 0; i < cycle.Count - 1; i++)
        {
            if (intersectionLookup.ContainsKey(cycle[i]))
                intersections.Add(intersectionLookup[cycle[i]]);
            else
                throw new IndexOutOfRangeException("Attempting to lookup intersection with non-existent pentaCoord key in DetermineVertexIndOfFace method");
        }
        Vector3 centroid = Geometry.PolygonCentroidXZ(intersections);
        int[] indices = new int[5];
        for (int i = 0; i < 5; i++)
        {
            Vector3 lineDirection = Geometry.RotateByAngleXZ(gridVectors[i], 90);
            int vectorIndex = i;
            for (int j = 0; j < gridDensity + 1; j++)
            {
                Vector3 crossA = Vector3.zero;
                Vector3 crossB = Vector3.zero;
                int gridIndex = 0;
                if (j < gridTurnover - 1)
                {
                    crossA = Vector3.Cross(lineDirection, centroid - gridLineOrigins[i, j]);
                    crossB = Vector3.Cross(lineDirection, centroid - gridLineOrigins[i, j + 1]);
                    gridIndex = j;
                }
                else if (j == gridTurnover - 1)
                {
                    if (Vector3.Dot(gridVectors[i], centroid - gridLineOrigins[i,j]) >= 0)
                    {
                        indices[vectorIndex] = gridTurnover - 1;
                        break;
                    }
                }
                else if (j == gridTurnover)
                {
                    crossA = Vector3.Cross(lineDirection, centroid - gridLineOrigins[i, 0]);
                    crossB = Vector3.Cross(lineDirection, centroid - gridLineOrigins[i, gridTurnover]);
                    gridIndex = gridTurnover;
                }
                else if (j < gridDensity - 1)
                {
                    crossA = Vector3.Cross(lineDirection, centroid - gridLineOrigins[i, j]);
                    crossB = Vector3.Cross(lineDirection, centroid - gridLineOrigins[i, j + 1]);
                    gridIndex = j + 1;
                }
                else
                {
                    indices[vectorIndex] = NormalizeGridIndex(gridDensity);
                    break;
                }
                if (crossA.y * crossB.y < 0)
                {
                    indices[vectorIndex] = NormalizeGridIndex(gridIndex);
                    break;
                }
            }
        }
        return ConvertToVertexInd(indices);
    }

    public int NormalizeGridIndex(int gridNumber)
    {
        int normalizedGridIndex = gridNumber;
        if (gridNumber > gridTurnover - 1)
            normalizedGridIndex = -1 - (gridNumber - gridTurnover);
        return normalizedGridIndex;
    }

    private VertexInd ConvertToVertexInd(int[] indices)
    {
        return new(indices[0], indices[1], indices[2], indices[3], indices[4]);
    }
}
