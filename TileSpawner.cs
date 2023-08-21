using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "TileSpawner", menuName = "ScriptableObjects/Utilities/TileSpawner", order = 1)]
public class TileSpawner : ScriptableObject
{
    public Pentagrid pentagrid;

    public GameObject tilePrefab;
    public bool randomStartingTile = true;
    public bool orderedTextures = true;
    public bool randomTextures = true;
    public TerrainType terrainType;
    public FeatureType featureType;

    [Header("Serialized Fields")]
    [Space]
    [SerializeField] private PentaCoord startingTilePentaCoord;
    [SerializeField] private int tileCount = 0;

    [Header("Events")]
    [Space]
    public UnityEvent Start;
    public UnityEvent EditorUpdate;

    public void OnStart()
    {
        SelectNewStartingTile();
        BuildTiling();
    }

    public void OnEditorUpdate()
    {
        if (pentagrid.HasChanged())
        {
            BuildTiling();
            UpdateTileCount();
        }
    }

    void UpdateTileCount()
    {
        tileCount = TilingController.GetTiling().GetActiveTiles().Count;
    }

    void SelectNewStartingTile()
    {
        if (randomStartingTile)
            startingTilePentaCoord = pentagrid.GetRandomOriginIntersection();
        else if (pentagrid.GetNumIntersections() == 0)
            throw new IndexOutOfRangeException("Pentagrid has no intersections within play area.");
        else
            startingTilePentaCoord = pentagrid.GetOriginIntersection(0);
        if (PenroseTiles.DetermineDegree(startingTilePentaCoord, pentagrid) == 0)
            throw new IndexOutOfRangeException("Starting tile pentagrid coordinates " + startingTilePentaCoord.ToString() + " are invalid.");
    }

    void SpawnTile(PentaCoord pentaCoord, Vector3 position)
    {
        if (!ValidPlacement(pentaCoord))
            return;
        TileController newTile = Instantiate(tilePrefab).GetComponent<TileController>();
        newTile.pentagrid = pentagrid;
        newTile.SetPentaCoord(pentaCoord);
        if (randomTextures || orderedTextures)
            newTile.GetProperties().SetupTile(pentaCoord, position, orderedTextures);
        else
            newTile.GetProperties().SetupTile(pentaCoord, position, terrainType, featureType);
        TilingController.GetTiling().AddActiveTile(pentaCoord, newTile);
    }

    public bool ValidPlacement(PentaCoord pentaCoord)
    {
        if (PenroseTiles.DetermineDegree(pentaCoord, pentagrid) == 0)
            return false;
        if (TilingController.GetTiling().GetActiveTiles().ContainsKey(pentaCoord))
            return false;
        return true;
    }

    //private void SpawnAllTiles(PentaCoord pentaCoord)
    //{
    //    TileController tile = PenroseTiles.GetTiling().GetTile(pentaCoord);
    //    foreach (PentaCoord adjacency in tile.GetAdjacencies())
    //    {
    //        if (adjacency.Equals(PentaCoord.zero))
    //            continue;
    //        TileData placeTile = tile.AdjacentTileData(adjacency);
    //        if (placeTile != null)
    //        {
    //            SpawnTile(placeTile.pentaCoord, placeTile.position);
    //            SpawnAllTiles(adjacency);
    //        }
    //    }
    //}

    private void SpawnAllTiles()
    {
        List<List<PentaCoord>> allWalks = ScopeUpWalks(startingTilePentaCoord, new());
        while (allWalks.Count > 0)
        {
            allWalks = ScopeUpWalks(startingTilePentaCoord, allWalks);
            foreach (List<PentaCoord> walk in allWalks)
            {
                TileData placeTile = TilingController.GetTiling().GetTile(walk[walk.Count - 2]).AdjacentTileData(walk[walk.Count - 1]);
                if (!placeTile.Equals(TileData.zero))
                    SpawnTile(placeTile.pentaCoord, placeTile.position);
            }
        }
    }

    private List<List<PentaCoord>> ScopeUpWalks(PentaCoord pentaCoord, List<List<PentaCoord>> currentScope) 
    {
        List<List<PentaCoord>> newScope = new();
        if (currentScope.Count == 0)
        {
            newScope.Add(new());
            newScope[0].Add(pentaCoord);
            return newScope;
        }

        foreach (List<PentaCoord> walk in currentScope)
        {
            foreach (PentaCoord adjacency in pentagrid.GetAdjacencies(walk[walk.Count - 1]))
            {
                if (ValidPlacement(adjacency))
                {
                    bool alreadyVisiting = false;
                    foreach (List<PentaCoord> newWalk in newScope)
                    {
                        if (newWalk[newWalk.Count - 1].Equals(adjacency))
                        {
                            alreadyVisiting = true;
                            break;
                        }
                    }
                    if (!alreadyVisiting)
                    {
                        List<PentaCoord> addWalk = new(walk);
                        addWalk.Add(adjacency);
                        newScope.Add(addWalk);
                    }
                }
            }
        }
        return newScope;
    }

    public void BuildTiling()
    {
        DestroyTiling();
        if (!pentagrid.HasPentagridData())
            pentagrid.UpdatePentagridData();
        SelectNewStartingTile();
        SpawnTile(startingTilePentaCoord, Vector3.zero);
        SpawnAllTiles();
        UpdateTileCount();
    }

    public void DestroyTiling()
    {
        if (TilingController.GetTiling().GetActiveTiles().Count != 0)
            TilingController.GetTiling().DestroyActiveTiles();
        UpdateTileCount();
    }
}
