using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;


public class PentagridVertexData
{
    public Dictionary<VertexInd, VertexConfiguration> vertexConfigurationTable { get; private set; }
    public Dictionary<PentaCoord, List<VertexInd>> tileVertexTable { get; private set; }
    public Dictionary<PrimitiveChain, int> chainCountingTable { get; private set; }
    public HashSet<PrimitiveChain> identifiedChains { get; private set; }
    public HashSet<PrimitiveChain> unidentifiedChains { get; private set; }

    public PentagridVertexData(List<PrimitiveChain> chains)
    {
        vertexConfigurationTable = new(new VertexIndEqualityComparer());
        tileVertexTable = new(new PentaCoordEqualityComparer());
        chainCountingTable = new();
        identifiedChains = new();
        unidentifiedChains = new(chains);
    }
}

[CreateAssetMenu(fileName = "PrimitiveChainSequencer", menuName = "ScriptableObjects/Utilities/PrimitiveChainSequencer")]
[ExecuteInEditMode]
public class PrimitiveChainSequencer : ScriptableObject
{
    public Pentagrid outputPentagrid;
    public GameObject inputTiling;
    public bool limitSearchTime = true;
    public bool logVertexData = true;
    [Range(0.01f, 360f)] public double maxSearchTime = 60;
    [Range(0, 32)] public int targetChainIndex = 0;
    public List<PrimitiveChain> chains;

    private PentagridVertexData outputGrid;
    private PentagridVertexData inputGrid;

    private double searchStartTime;

    public void LoadChainsInAssetDatabase()
    {
        string[] chainAssetNames = AssetDatabase.FindAssets("t:PrimitiveChain", new string[] { "Assets/Resources/Utilities/Primitive Chains" });
        chains = new();
        if (chainAssetNames.Length == 0)
            return;
        foreach (string chainName in chainAssetNames)
            chains.Add(AssetDatabase.LoadAssetAtPath<PrimitiveChain>(AssetDatabase.GUIDToAssetPath(chainName)));
    }

    public void SortAllChainsByLength()
    {
        GeneralUtilities.QuickSort<List<PrimitiveChain>, PrimitiveChain>(chains, 0, chains.Count - 1);
    }

    public void SortChainsByLengthUpToTarget()
    {
        GeneralUtilities.QuickSort<List<PrimitiveChain>, PrimitiveChain>(chains, 0, targetChainIndex);
    }

    public void SelectFirstChain()
    {
        searchStartTime = EditorApplication.timeSinceStartup;
        if (!outputPentagrid.HasPentagridData())
            outputPentagrid.UpdatePentagridData();
        PrimitiveChain targetChain = chains[targetChainIndex];
        Debug.Log("Searching for first occurrence of " + targetChain + " to add to selection...");
        List<PentaCoord> identifiedCycle = null;
        foreach (TileController tile in TilingController.GetTiling().GetActiveTiles().Values)
        {
            List<List<PentaCoord>> allCycles = AllCyclesAlignedWithChain(tile.GetPentaCoord(), targetChain);
            foreach (List<PentaCoord> cycle in allCycles)
            {
                if (targetChain.IdentifiesAs(cycle, outputPentagrid))
                {
                    identifiedCycle = cycle;
                    break;
                }
            }
            if (identifiedCycle != null && VertexPropertySatisfiedBy(identifiedCycle, TilingController.GetTiling()))
                break;
            else
                identifiedCycle = null;
        }
        if (identifiedCycle == null)
        {
            Debug.Log(targetChain + " not found.");
            Debug.Log("Total search time: " + (EditorApplication.timeSinceStartup - searchStartTime));
            return; 
        }
        Debug.Log("Found " + targetChain + " with cycle: " + GeneralUtilities.CollectionToString(identifiedCycle));
        Debug.Log("Total search time: " + (EditorApplication.timeSinceStartup - searchStartTime));
        GameObject[] tilesToSelect = new GameObject[targetChain.links.Count];
        for (int i = 0; i < tilesToSelect.Length; i++)
        {
            tilesToSelect[i] = TilingController.GetTiling().GetTile(identifiedCycle[i]).gameObject;
        }
        Selection.objects = tilesToSelect;
        VertexInd indices = outputPentagrid.DetermineVertexIndOfFace(identifiedCycle);
        Debug.Log("Selected vertex has indices " + indices.ToString());
    }

    public void SelectAllChains()
    {
        searchStartTime = EditorApplication.timeSinceStartup;
        if (!outputPentagrid.HasPentagridData())
            outputPentagrid.UpdatePentagridData();
        PrimitiveChain targetChain = chains[targetChainIndex];
        Debug.Log("Searching for all occurences of " + targetChain + " to add to selection...");
        List<List<PentaCoord>> identifiedCycles = new();
        identifiedCycles.Add(null);
        int numIdentified = 0;
        foreach (TileController tile in TilingController.GetTiling().GetActiveTiles().Values)
        {
            List<List<PentaCoord>> allCycles = AllCyclesAlignedWithChain(tile.GetPentaCoord(), targetChain);
            foreach (List<PentaCoord> cycle in allCycles)
            {
                if (targetChain.IdentifiesAs(cycle, outputPentagrid))
                {
                    identifiedCycles[numIdentified] = cycle;
                    break;
                }
            }
            if (identifiedCycles[numIdentified] != null && VertexPropertySatisfiedBy(identifiedCycles[numIdentified], TilingController.GetTiling()))
            {
                numIdentified++;
                identifiedCycles.Add(null);
            }
            else
                identifiedCycles[numIdentified] = null;
        }
        if (identifiedCycles[0] == null)
        {
            Debug.Log(targetChain + " not found.");
            Debug.Log("Total search time: " + (EditorApplication.timeSinceStartup - searchStartTime));
            return;
        }
        Debug.Log("Total search time: " + (EditorApplication.timeSinceStartup - searchStartTime));
        GameObject[] tilesToSelect = new GameObject[numIdentified * targetChain.links.Count];
        for (int i = 0; i < numIdentified; i++)
        {
            List<PentaCoord> identifiedCycle = identifiedCycles[i];
            for (int j = 0; j < targetChain.links.Count; j++)
            {
                tilesToSelect[i * targetChain.links.Count + j] = TilingController.GetTiling().GetTile(identifiedCycle[j]).gameObject;
            }
        }
        Selection.objects = tilesToSelect;
    }

    public PentagridVertexData SequenceOutput()
    {
        searchStartTime = EditorApplication.timeSinceStartup;
        outputGrid = new(chains);
        if (!outputPentagrid.HasPentagridData())
            outputPentagrid.UpdatePentagridData();
        foreach (TileController tile in TilingController.GetTiling().GetActiveTiles().Values)
            IdentifyAllChainsIncluding(tile, TilingController.GetTiling(), outputGrid);
        if (logVertexData)
        {
            LogLastAccount(outputGrid);
            Debug.Log("Total search time: " + (EditorApplication.timeSinceStartup - searchStartTime));
        }
        return outputGrid;
    }

    public PentagridVertexData SequenceInput()
    {
        searchStartTime = EditorApplication.timeSinceStartup;
        inputGrid = new(chains);
        if (!inputTiling.GetComponent<TilingController>().pentagrid.HasPentagridData())
            inputTiling.GetComponent<TilingController>().pentagrid.UpdatePentagridData();
        foreach (Transform tile in inputTiling.transform)
            IdentifyAllChainsIncluding(tile.GetComponent<TileController>(), inputTiling.GetComponent<TilingController>(), inputGrid);
        if (logVertexData)
        {
            LogLastAccount(inputGrid);
            Debug.Log("Total search time: " + (EditorApplication.timeSinceStartup - searchStartTime));
        }
        return inputGrid;
    }

    private void IdentifyAllChainsIncluding(TileController tile, TilingController tiling, PentagridVertexData gridData)
    {
        if (EditorApplication.timeSinceStartup - searchStartTime > maxSearchTime && limitSearchTime)
        {
            LogLastAccount(gridData);
            Debug.Log("Total search time: " + (EditorApplication.timeSinceStartup - searchStartTime));
            throw new TimeoutException("Search exceeded maximum search time.");
        }
        if (gridData.tileVertexTable.ContainsKey(tile.GetPentaCoord()) && gridData.tileVertexTable[tile.GetPentaCoord()].Count > 3)
            return;
        List<List<PentaCoord>> allCycles = ScopeUpCycles(tile.GetPentaCoord(), new(), tiling);
        allCycles = ScopeUpCycles(tile.GetPentaCoord(), allCycles, tiling);
        allCycles = ScopeUpCycles(tile.GetPentaCoord(), allCycles, tiling);
        int longestIdentified = GetMaximumChainLength(gridData.identifiedChains);
        if (gridData.identifiedChains.Count != 0)
        {
            for (int i = 3; i < longestIdentified + 1; i++)
            {
                HashSet<PrimitiveChain> potentialChains = ChainsUpToLength(i + 1, gridData.identifiedChains);
                allCycles = ScopeUpCycles(tile.GetPentaCoord(), allCycles, tiling);
                foreach (List<PentaCoord> cycle in allCycles)
                {
                    foreach (PrimitiveChain chain in potentialChains)
                    {
                        if (chain.IdentifiesAs(cycle, tiling.pentagrid) && VertexPropertySatisfiedBy(cycle, tiling))
                        {
                            AccountVertexData(cycle, chain, tiling, gridData);
                            break;
                        }
                    }
                }
                if (gridData.tileVertexTable.ContainsKey(tile.GetPentaCoord()) && gridData.tileVertexTable[tile.GetPentaCoord()].Count > 3)
                    return;
            }
        }
        if (gridData.tileVertexTable.ContainsKey(tile.GetPentaCoord()) && gridData.tileVertexTable[tile.GetPentaCoord()].Count > 3)
            return;
        gridData.unidentifiedChains.ExceptWith(gridData.identifiedChains);
        int longestUnidentified = GetMaximumChainLength(gridData.unidentifiedChains);
        for (int i = 3; i < longestUnidentified + 1; i++)
        { 
            HashSet<PrimitiveChain> potentialChains = ChainsUpToLength(i + 1, gridData.unidentifiedChains);
            if (i > longestIdentified)
                allCycles = ScopeUpCycles(tile.GetPentaCoord(), allCycles, tiling);
            foreach (List<PentaCoord> cycle in allCycles)
            {
                foreach (PrimitiveChain chain in potentialChains)
                {
                    if (chain.IdentifiesAs(cycle, tiling.pentagrid) && VertexPropertySatisfiedBy(cycle, tiling))
                    {
                        AccountVertexData(cycle, chain, tiling, gridData);
                        break;
                    }
                }
            }
            if (gridData.tileVertexTable.ContainsKey(tile.GetPentaCoord()) && gridData.tileVertexTable[tile.GetPentaCoord()].Count > 3)
                return;
        }
    }

    private HashSet<PrimitiveChain> ChainsUpToLength(int length, HashSet<PrimitiveChain> chains)
    {
        HashSet<PrimitiveChain> exclusion = new(chains);
        foreach (PrimitiveChain chain in chains)
            if (chain.links.Count > length)
                exclusion.Remove(chain);
        return exclusion;
    }

    private List<List<PentaCoord>> ScopeUpCycles(PentaCoord pentaCoord, List<List<PentaCoord>> currentScope, TilingController tiling)
    {
        List<List<PentaCoord>> newScope = new();
        if (currentScope.Count == 0)
        {
            newScope = new();
            newScope.Add(new());
            newScope[0].Add(pentaCoord);
            return newScope;
        }
        foreach (List<PentaCoord> walk in currentScope)
        {
            if (walk.Count > 3 && walk[walk.Count - 1].Equals(pentaCoord)) 
            {
                newScope.Add(walk);
                continue; 
            }
            foreach (PentaCoord adjacency in tiling.pentagrid.GetAdjacencies(walk[walk.Count - 1]))
            {
                if (adjacency.Equals(PentaCoord.zero) || (adjacency.Equals(pentaCoord) && walk.Count == 2) || (walk.IndexOf(adjacency) != - 1 && !adjacency.Equals(pentaCoord)))
                    continue;
                List<PentaCoord> potentialCycle = new(walk);
                potentialCycle.Add(adjacency);
                if (VertexPropertySatisfiedBy(potentialCycle, tiling))
                    newScope.Add(potentialCycle);
            }
        }
        return newScope;
    }

    private void AccountVertexData(List<PentaCoord> cycle, PrimitiveChain chain, TilingController tiling, PentagridVertexData gridData)
    {
        VertexInd indices = tiling.pentagrid.DetermineVertexIndOfFace(cycle);
        if (gridData.vertexConfigurationTable.ContainsKey(indices))
            return;
        gridData.vertexConfigurationTable.Add(indices, new(chain, chain.AlignCycleToChain(cycle, tiling.pentagrid)));
        if (!gridData.identifiedChains.Contains(chain))
        {
            gridData.identifiedChains.Add(chain);
            gridData.chainCountingTable.Add(chain, 0);
        }
        gridData.chainCountingTable[chain]++;
        Dictionary<VertexInd, int> potentialAdjacencies = new(new VertexIndEqualityComparer());
        for (int i = 0; i < cycle.Count - 1; i++)
        {
            if (!gridData.tileVertexTable.ContainsKey(cycle[i]))
            {
                gridData.tileVertexTable.Add(cycle[i], new());
            }
            foreach (VertexInd otherIndices in gridData.tileVertexTable[cycle[i]])
            {
                if (!potentialAdjacencies.ContainsKey(otherIndices))
                    potentialAdjacencies.Add(otherIndices, 0);
                potentialAdjacencies[otherIndices]++;
            }
            gridData.tileVertexTable[cycle[i]].Add(indices);
            gridData.vertexConfigurationTable[indices].typingChain.terrainTypes.Add(tiling.GetTile(cycle[i]).properties.terrainType);
            gridData.vertexConfigurationTable[indices].typingChain.featureTypes.Add(tiling.GetTile(cycle[i]).properties.featureType);
        }
        gridData.vertexConfigurationTable[indices].AlignChains();
        foreach (VertexInd otherIndices in potentialAdjacencies.Keys)
        {
            if (potentialAdjacencies[otherIndices] > 1)
            {
                gridData.vertexConfigurationTable[indices].adjacencies.Add(otherIndices);
                gridData.vertexConfigurationTable[otherIndices].adjacencies.Add(indices);
            }
        }
    }

    private void LogLastAccount(PentagridVertexData gridData)
    {
        if (gridData.identifiedChains.Count == 0)
            return;
        foreach (PrimitiveChain chain in gridData.identifiedChains)
            Debug.Log("Identified " + gridData.chainCountingTable[chain] + " vertices as " + chain);
        Debug.Log("Identified " + gridData.vertexConfigurationTable.Count + " unique vertices in total.");
    }

    private int GetMaximumChainLength(HashSet<PrimitiveChain> chains)
    {
        int max = 0;
        foreach (PrimitiveChain chain in chains)
            if (chain.links.Count > max)
                max = chain.links.Count;
        return max;
    }

    private List<List<PentaCoord>> AllWalksAlignedWithChainsOfLength(PentaCoord pentaCoord, int walkLength, PentaCoord startingPentaCoord, HashSet<PrimitiveChain> targetChains)
    {
        if (EditorApplication.timeSinceStartup - searchStartTime > maxSearchTime && limitSearchTime)
            throw new TimeoutException("Search exceeded maximum search time.");
        List<List<PentaCoord>> result = new();
        if (walkLength == 0)
        {
            result.Add(new List<PentaCoord>());
            result[0].Add(pentaCoord);
            return result;
        }

        foreach (PentaCoord adjacency in outputPentagrid.GetAdjacencies(pentaCoord))
        {
            if (adjacency.Equals(PentaCoord.zero))
                continue;
            foreach (List<PentaCoord> walk in AllWalksAlignedWithChainsOfLength(adjacency, walkLength - 1, startingPentaCoord, targetChains))
            {
                bool rejectWalk = true;
                foreach (PrimitiveChain chain in targetChains)
                    if (chain.Contains(walk, outputPentagrid))
                    {
                        rejectWalk = false;
                        break;
                    }
                if (walk.IndexOf(startingPentaCoord) < 0 || rejectWalk)
                    continue;
                walk.Add(pentaCoord);
                result.Add(walk);
            }
        }

        return result;
    }

    private List<List<PentaCoord>> AllCyclesAlignedWithChain(PentaCoord pentaCoord, PrimitiveChain targetChain)
    {
        List<List<PentaCoord>> result = AllWalksAlignedWithChainOfLength(pentaCoord, targetChain.links.Count, pentaCoord, targetChain); 
        //Debug.Log("Considering " + result.Count + " walks of length " + targetChain.links.Count + " originating from " + pentaCoord.ToString());
        return result;
    }

    private List<List<PentaCoord>> AllWalksAlignedWithChainOfLength(PentaCoord pentaCoord, int walkLength, PentaCoord startingPentaCoord, PrimitiveChain targetChain)
    {
        if (EditorApplication.timeSinceStartup - searchStartTime > maxSearchTime && limitSearchTime)
            throw new TimeoutException("Search exceeded maximum search time.");
        List<List<PentaCoord>> result = new();
        if (walkLength == 0)
        {
            result.Add(new List<PentaCoord>());
            result[0].Add(pentaCoord);
            return result;
        }

        foreach (PentaCoord adjacency in outputPentagrid.GetAdjacencies(pentaCoord))
        {
            if (adjacency.Equals(PentaCoord.zero))
                continue;
            foreach (List<PentaCoord> walk in AllWalksAlignedWithChainOfLength(adjacency, walkLength - 1, startingPentaCoord, targetChain))
            {
                if (walk.IndexOf(startingPentaCoord) < 0 || !targetChain.Contains(walk, outputPentagrid))
                    continue;
                walk.Add(pentaCoord);
                result.Add(walk);
            }
        }

        return result;
    }

    private bool VertexPropertySatisfiedBy(List<PentaCoord> cycle, TilingController tiling)
    {
        List<Vector3> vertices = PenroseTiles.DetermineRawVerticesOf(cycle[0], tiling.pentagrid);
        List<Vector3> vertexRecord = new();
        vertexRecord.AddRange(vertices);
        Vector3 lastPosition = Vector3.zero;
        for (int i  = 0; i < cycle.Count - 1; i++)
        {
            PentaCoord lastStep = cycle[i];
            PentaCoord thisStep = cycle[i + 1];
            List<PentaCoord> adjacencies = tiling.pentagrid.GetAdjacencies(lastStep);
            List<Vector3> adjacentVertices = PenroseTiles.DetermineVerticesOfAdjacency(thisStep, lastStep, lastPosition, adjacencies, tiling.pentagrid);
            vertexRecord.AddRange(adjacentVertices);
            vertices = Geometry.StripOutUncommonVectorsXZ(vertices, adjacentVertices);
            if (vertices.Count == 0)
                return false;
            lastPosition = PenroseTiles.DetermineRelativePositionOf(thisStep, lastStep, lastPosition, adjacencies, tiling.pentagrid);
        }
        if (vertices.Count > 2)
            return false;
        if (vertices.Count == 2)
            return true;
        return CyclesCounterclockwiseAround(vertices[0], cycle, tiling);
    }

    private bool CyclesCounterclockwiseAround(Vector3 vertexOffset, List<PentaCoord> cycle, TilingController tiling)
    {
        Vector3 midpointA = tiling.GetTile(cycle[0]).GetProperties().GetPosition();
        Vector3 midpointB = tiling.GetTile(cycle[1]).GetProperties().GetPosition();
        Vector3 vertexProper = midpointA + vertexOffset;
        Vector3 crossProduct = Vector3.Cross((midpointA - vertexProper), (midpointB - vertexProper));
        return crossProduct.y < 0;
    }
}
