using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[ExecuteInEditMode]
public class TilingController : MonoBehaviour
{
    public bool prefabMode = false;
    public Pentagrid pentagrid;
    public TileSpawner tileSpawner;
    public ConstraintsGenerator generator;

    public PentagridVertexData vertexData { get; private set; }
    public WaveFunctionPossiblitySpace possibilitySpace { get; private set; }

    [SerializeField] private int tileCount;

    private Dictionary<PentaCoord, TileController> activeTiles;

    private void Awake()
    {
        pentagrid.Start?.Invoke();
        if (!prefabMode)
            tileSpawner.Start?.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        if (activeTiles == null)
            InitializeActiveTiles();
        if (Application.isEditor && !prefabMode)
        {
            pentagrid.EditorUpdate?.Invoke();
            tileSpawner.EditorUpdate?.Invoke();
        }
        tileCount = activeTiles.Count;
    }

    public void AddActiveTile(PentaCoord pentaCoord, TileController tile)
    {
        if (activeTiles == null)
            InitializeActiveTiles();
        if (activeTiles.ContainsKey(pentaCoord))
            return;
        tile.transform.parent = transform;
        activeTiles.Add(pentaCoord.Arranged(), tile);
    }

    public TileController GetTile(PentaCoord pentaCoord)
    {
        if (activeTiles == null)
            InitializeActiveTiles();
        if (activeTiles.ContainsKey(pentaCoord))
            return activeTiles[pentaCoord];
        throw new IndexOutOfRangeException("Attempting to access PentaCoord " + pentaCoord.ToString() + " when it is not present in activeTiles");
    }

    public void InitializeActiveTiles()
    {
        activeTiles = new(new PentaCoordEqualityComparer());
        foreach (Transform child in transform)
        {
            TileController tile = child.GetComponent<TileController>();
            if (tile != null)
            {
                activeTiles.Add(tile.GetPentaCoord().Arranged(), tile);
            }
        }
    }

    public void DestroyActiveTiles()
    {
        if (prefabMode)
            return;
        List<PentaCoord> activeCoords = new(activeTiles.Keys);
        foreach (PentaCoord pentaCoord in activeCoords)
        {
            DestroyImmediate(activeTiles[pentaCoord].gameObject);
            activeTiles.Remove(pentaCoord);
        }
        activeTiles = new(new PentaCoordEqualityComparer());       
    }

    public Dictionary<PentaCoord, TileController> GetActiveTiles()
    {
        if (activeTiles == null)
            InitializeActiveTiles();
        return activeTiles;
    }

    public Pentagrid GetPentagrid()
    {
        return pentagrid;
    }

    public TileSpawner GetTileSpawner()
    {
        return tileSpawner;
    }

    public void SetPrefabMode()
    {
        prefabMode = true;
        foreach (Transform child in transform)
            child.GetComponent<TileController>().prefabMode = true;
    }

    public void ClearPrefabMode()
    {
        prefabMode = false;
        foreach (Transform child in transform)
            child.GetComponent<TileController>().prefabMode = false;
    }

    public void CascadePentagrid()
    {
        foreach (Transform child in transform)
        {
            TileController controller = child.GetComponent<TileController>();
            controller.pentagrid = pentagrid;
            controller.pentaCoord.pentagrid = pentagrid;
        }
    }

    public void EnableTiles()
    {
        foreach (Transform child in transform)
        {
            TileController controller = child.GetComponent<TileController>();
            TileProperties properties = child.GetComponent<TileProperties>();
            controller.enabled = true;
            controller.properties = properties;
            properties.enabled = true;
            properties.controller = controller;
        }
    }

    public static TilingController GetTiling()
    {
        TilingController tilingSearch = GameObject.FindGameObjectWithTag("Penrose Tiling").GetComponent<TilingController>();
        return tilingSearch;
    }

    public void GeneratePossiblitySpace()
    {
        possibilitySpace = generator.GeneratePossibilitySpace();
        vertexData = generator.GetOutputGrid();
    }
}
