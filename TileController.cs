using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public struct TileData : IEquatable<TileData>
{
    public PentaCoord pentaCoord { get; private set; }
    public Vector3 position { get; private set; }

    public TileData(PentaCoord pentaCoord, Vector3 position)
    {
        this.pentaCoord = pentaCoord;
        this.position = position;
    }

    public static TileData zero = new TileData(PentaCoord.zero, Vector3.zero);

    public bool Equals(TileData other)
    {
        return pentaCoord.Equals(other.pentaCoord) && position.Equals(other.position);
    }
}

[Serializable]
[ExecuteInEditMode]
public class TileController : MonoBehaviour
{
    public TileProperties properties;
    public Pentagrid pentagrid;
    public bool prefabMode = false;
    public PentaCoord pentaCoord;

    [Header("Serialized Fields")]
    [Space]
    [SerializeField] private List<PentaCoord> adjacencies;

    [Header("Test Point Parameters")]
    [Space]
    public bool displayTestPointResults = true;
    public PentaCoord testCoord;
    [Range(0, 3)] public int testPointIndex = 0;
    public TileType resultType;
    public int resultRotation;
    public float resultDistance;
    public int resultReferenceAngle;
    public int resultAngularOffset;

    void Awake()
    {
        if (prefabMode)
            return;
        if (!pentagrid.HasPentagridData())
            pentagrid.UpdatePentagridData();
        AcquireAdjacenciesFromGrid();
    }

    // Update is called once per frame
    void Update()
    {
        if (TilingController.GetTiling().GetPentagrid() != null && !prefabMode)
        {
            if (!TilingController.GetTiling().GetPentagrid().HasPentagridData())
                TilingController.GetTiling().GetPentagrid().UpdatePentagridData();
            if (adjacencies == null || adjacencies.Count == 0)
                AcquireAdjacenciesFromGrid();
            if (displayTestPointResults)
                ComputeTestPointResults();
        }
        
    }

    void AcquireAdjacenciesFromGrid()
    {
        if (PenroseTiles.DetermineDegree(pentaCoord, pentagrid) != 0) 
            adjacencies = TilingController.GetTiling().GetPentagrid().GetAdjacencies(pentaCoord);
    }

    void ComputeTestPointResults()
    {
        testCoord = new PentaCoord(adjacencies[testPointIndex].u, adjacencies[testPointIndex].v, pentagrid);
        if (!testCoord.Equals(PentaCoord.zero) && adjacencies != null && adjacencies.Count > 0)
        {
            AcquireAdjacenciesFromGrid();
            properties.SetRotation(PenroseTiles.DetermineRotation(pentaCoord, pentagrid));
            resultRotation = PenroseTiles.DetermineRotation(testCoord, pentagrid);
            resultType = PenroseTiles.DetermineType(testCoord, pentagrid);
            resultDistance = PenroseTiles.DetermineDistanceBetween(testCoord, pentaCoord, pentagrid);
            resultReferenceAngle = PenroseTiles.DetermineReferenceAngleOf(testCoord, pentaCoord, pentagrid);
            resultAngularOffset = PenroseTiles.DetermineAngularOffsetTo(testCoord, pentaCoord, resultReferenceAngle, adjacencies, pentagrid);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (displayTestPointResults && resultType != TileType.BorderTile)
        {
            Gizmos.color = Color.gray;
            Vector3 displacementVector = PenroseTiles.DetermineDisplacementVectorTo(resultDistance, resultAngularOffset, pentaCoord, pentagrid);
            Vector3 tilePosition = transform.position + displacementVector;
            Gizmos.DrawLine(transform.position, tilePosition);
            float rhombWidth;
            float rhombHeight;
            switch (resultType)
            {
                case TileType.ThinRhomb:
                    rhombWidth = PenroseTiles.thinRhombWidth;
                    rhombHeight = PenroseTiles.thinRhombHeight;
                    break;
                case TileType.ThickRhomb:
                    rhombWidth = PenroseTiles.thickRhombWidth;
                    rhombHeight = PenroseTiles.thickRhombHeight;
                    break;
                default:
                    throw new IndexOutOfRangeException("Attempting to access TileType which does not exist within OnDrawGizmosSelected method.");
            }
            Gizmos.color = Color.black;
            Gizmos.DrawLine(transform.position, transform.position + PenroseTiles.thinRhombWidth * Geometry.RotateByAngleXZ(Vector3.right, properties.GetRotation()));
            Gizmos.DrawLine(tilePosition, tilePosition + rhombWidth * Geometry.RotateByAngleXZ(Vector3.right, resultRotation));
            Gizmos.color = Color.white;
            Gizmos.DrawLine(tilePosition + rhombWidth * Geometry.RotateByAngleXZ(Vector3.right, resultRotation), tilePosition + rhombHeight * Geometry.RotateByAngleXZ(Vector3.forward, resultRotation));
            Gizmos.DrawLine(tilePosition + rhombHeight * Geometry.RotateByAngleXZ(Vector3.forward, resultRotation), tilePosition + rhombWidth * Geometry.RotateByAngleXZ(Vector3.left, resultRotation));
            Gizmos.DrawLine(tilePosition + rhombWidth * Geometry.RotateByAngleXZ(Vector3.left, resultRotation), tilePosition + rhombHeight * Geometry.RotateByAngleXZ(Vector3.back, resultRotation));
            Gizmos.DrawLine(tilePosition + rhombHeight * Geometry.RotateByAngleXZ(Vector3.back, resultRotation), tilePosition + rhombWidth * Geometry.RotateByAngleXZ(Vector3.right, resultRotation));
        }
    }

    public void SetPentaCoord(PentaCoord pentaCoord)
    {
        this.pentaCoord = pentaCoord;
        this.pentaCoord.pentagrid = pentagrid;
        AcquireAdjacenciesFromGrid();
    }

    public PentaCoord GetPentaCoord()
    {
        return pentaCoord;
    }

    public TileProperties GetProperties()
    {
        return properties;
    }

    public List<PentaCoord> GetAdjacencies()
    {
        return adjacencies;
    }

    public void UpdateAdjacencies()
    {
        AcquireAdjacenciesFromGrid();
    }

    public TileData AdjacentTileData(PentaCoord adjacency)
    {
        if (!TilingController.GetTiling().GetTileSpawner().ValidPlacement(adjacency)) 
            return TileData.zero;
        if (IsAdjacentTo(adjacency)) 
            return new TileData(adjacency, PenroseTiles.DetermineRelativePositionOf(adjacency, pentaCoord, transform.position, adjacencies, pentagrid));
        return TileData.zero;
    }

    public bool IsAdjacentTo(PentaCoord intersection)
    {
        foreach (PentaCoord adjacency in adjacencies)
            if (intersection.Equals(adjacency)) 
                return true;
        return false;
    }
}
