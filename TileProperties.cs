using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public enum TerrainType
{
    None,
    Grassland,
    Desert,
    Tundra
}

[Serializable]
public enum FeatureType
{
    None,
    Hillside,
    Riverland,
    Forest
}

[ExecuteInEditMode]
public class TileProperties : MonoBehaviour
{
    public TileController controller;
    public LineRenderer lineRendr;
    public MeshFilter meshFilter;
    public MeshCollider meshCollider;
    public MeshRenderer meshRendr;
    public TileType tileType;
    public TerrainType terrainType;
    public MaterialList terrainMatList;
    public FeatureType featureType;
    public MaterialList featureMatList;
    public MaterialList simpleMatList;

    [Header("Serialized Fields")]
    [Space]
    [SerializeField][Range(0, 359)] private int rotation;

    private Mesh mesh;
    private TerrainType lastTerrainType;
    private FeatureType lastFeatureType;

    // Start is called before the first frame update
    void Start()
    {
        transform.rotation = Quaternion.Euler(-90, -rotation, 0);
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        mesh = new Mesh();
        SetMeshVertices();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
        SetBorderVertices();
    }

    void Awake()
    {
        transform.rotation = Quaternion.Euler(-90, -rotation, 0);
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        mesh = new Mesh();
        SetMeshVertices();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
        SetBorderVertices();
        lastTerrainType = terrainType;
        lastFeatureType = featureType;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(-90, -rotation, 0);
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        SetMeshVertices();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
        SetBorderVertices();
        if (terrainType != lastTerrainType)
            SetTerrain(terrainType);
        if (featureType != lastFeatureType)
            SetFeature(featureType);
        lastTerrainType = terrainType;
        lastFeatureType = featureType;
    }

    void SetMeshVertices()
    {
        int[] triangles = { 0, 1, 2, 2, 3, 0 };
        Vector3[] normals =
        {
            Vector3.forward,
            Vector3.forward,
            Vector3.forward,
            Vector3.forward
        };
        float rhombWidth = 0;
        float rhombHeight = 0;
        switch (tileType)
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
                break;
        }
        Vector3[] vertices =
        {
            new Vector3(rhombWidth, 0, 0),
            new Vector3(0, rhombHeight, 0),
            new Vector3(-rhombWidth, 0, 0),
            new Vector3(0, -rhombHeight, 0)
        };
        Vector2 uvOrigin = new Vector2(0.5f, 0.5f);
        Vector2[] uv =
        {
            uvOrigin + rhombWidth / 2 * Vector2.right,
            uvOrigin + rhombHeight / 2 * Vector2.up,
            uvOrigin - rhombWidth / 2 * Vector2.right,
            uvOrigin - rhombHeight / 2 * Vector2.up,
        };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv;
    }

    void SetBorderVertices()
    {
        Vector3[] newPositions = new Vector3[5];
        Vector2[] vertexTrans = new Vector2[4];
        switch (tileType)
        {
            case TileType.ThinRhomb:
                vertexTrans[0] = new Vector2(PenroseTiles.thinRhombWidth, 0);
                vertexTrans[1] = new Vector2(0, PenroseTiles.thinRhombHeight);
                vertexTrans[2] = new Vector2(-PenroseTiles.thinRhombWidth, 0);
                vertexTrans[3] = new Vector2(0, -PenroseTiles.thinRhombHeight);
                break;
            case TileType.ThickRhomb:
                vertexTrans[0] = new Vector2(PenroseTiles.thickRhombWidth, 0);
                vertexTrans[1] = new Vector2(0, PenroseTiles.thickRhombHeight);
                vertexTrans[2] = new Vector2(-PenroseTiles.thickRhombWidth, 0);
                vertexTrans[3] = new Vector2(0, -PenroseTiles.thickRhombHeight);
                break;
            default:
                break;
        }
        for (int i = 0; i < 4; i++)
        {
            float xTrans = vertexTrans[i].x;
            float yTrans = vertexTrans[i].y;
            vertexTrans[i].x = xTrans * Mathf.Cos(rotation * Mathf.Deg2Rad) - yTrans * Mathf.Sin(rotation * Mathf.Deg2Rad);
            vertexTrans[i].y = xTrans * Mathf.Sin(rotation * Mathf.Deg2Rad) + yTrans * Mathf.Cos(rotation * Mathf.Deg2Rad);
            newPositions[i] = transform.position + new Vector3(vertexTrans[i].x, 0, vertexTrans[i].y);
        }
        newPositions[4] = newPositions[0];
        lineRendr.SetPositions(newPositions);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    public int GetRotation()
    {
        return rotation;
    }

    public void SetRotation(int rotation)
    {
        this.rotation = rotation;
    }

    public TileType GetTileType() 
    { 
        return tileType; 
    }

    public void SetType(TileType tileType)
    {
        this.tileType = tileType;
    }

    public void SetTerrain(TerrainType terrainType)
    {
        if (tileType == TileType.ThinRhomb) throw new IndexOutOfRangeException("Attempting to set TerrainType of " + terrainType + " to ThinRhomb tile.");
        this.terrainType = terrainType;
        switch (terrainType)
        {
            case TerrainType.Grassland:
                meshRendr.material = terrainMatList.materialList[0];
                return;
            case TerrainType.Desert:
                meshRendr.material = terrainMatList.materialList[1];
                return;
            case TerrainType.Tundra:
                meshRendr.material = terrainMatList.materialList[2];
                return;
            default:
                return;
        }
    } 

    public void SetFeature(FeatureType featureType)
    {
        if (tileType == TileType.ThickRhomb) throw new IndexOutOfRangeException("Attempting to set FeatureType of " + featureType + " to ThickRhomb tile.");
        this.featureType = featureType;
        switch (featureType)
        {
            case FeatureType.Hillside:
                meshRendr.material = featureMatList.materialList[0];
                return;
            case FeatureType.Riverland:
                meshRendr.material = featureMatList.materialList[1];
                return;
            case FeatureType.Forest:
                meshRendr.material = featureMatList.materialList[2];
                return;
            default:
                return;
        }
    }

    public void SetupTile(PentaCoord pentaCoord, Vector3 position)
    {
        SetPosition(position);
        SetRotation(PenroseTiles.DetermineRotation(pentaCoord, controller.pentagrid));
        SetType(PenroseTiles.DetermineType(pentaCoord, controller.pentagrid));
        switch (tileType)
        {
            case TileType.ThinRhomb:
                SetFeature(RandomBaseFeature());
                break;
            case TileType.ThickRhomb:
                SetTerrain(RandomBaseTerrain());
                break;
            default:
                break;
        }
    }

    public void SetupTile(PentaCoord pentaCoord, Vector3 position, TerrainType terrainType, FeatureType featureType)
    {
        SetPosition(position);
        SetRotation(PenroseTiles.DetermineRotation(pentaCoord, controller.pentagrid));
        SetType(PenroseTiles.DetermineType(pentaCoord, controller.pentagrid));
        switch (tileType)
        {
            case TileType.ThinRhomb:
                SetFeature(featureType);
                break;
            case TileType.ThickRhomb:
                SetTerrain(terrainType);
                break;
            default:
                break;
        }
    }

    public void SetupTile(PentaCoord pentaCoord, Vector3 position, bool parityPlacement)
    {
        SetupTile(pentaCoord, position);
        if (!parityPlacement)
        {
            return;
        }
        int parity = PenroseTiles.DetermineOrder(pentaCoord, controller.pentagrid);
        featureType = FeatureType.None;
        terrainType = TerrainType.None;
        meshRendr.material = simpleMatList.materialList[parity];
    }

    public static TerrainType RandomBaseTerrain()
    {
        int rand = UnityEngine.Random.Range(1, 4);
        return (TerrainType)rand;
    }

    public static FeatureType RandomBaseFeature()
    {
        int rand = UnityEngine.Random.Range(1, 4);
        return (FeatureType)rand;
    }
}
