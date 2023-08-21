using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveFunctionConstraints
{
    public Dictionary<VertexConfiguration, List<VertexConfiguration>> adjacencyConstraints { get; private set; }
    public Dictionary<PrimitiveChain, List<TypingChain>> configurationConstraints { get; private set; }
    public List<VertexConfiguration> subConfigurationConstraints { get; private set; }
    public Dictionary<int, int> subConfigurationIndexLookup { get; private set; }

    public WaveFunctionConstraints()
    {
        adjacencyConstraints = new(new VertexConfigurationEqualityComparer());
        configurationConstraints = new();
        subConfigurationConstraints = new();
    }

    public void SortSubConfigurationConstraints()
    {
        subConfigurationConstraints = GeneralUtilities.QuickSort<List<VertexConfiguration>, VertexConfiguration>(subConfigurationConstraints, 0, subConfigurationConstraints.Count - 1);
        subConfigurationIndexLookup = new();
        for (int i = 0; i < subConfigurationConstraints.Count; i++)
            if (!subConfigurationIndexLookup.ContainsKey(subConfigurationConstraints[i].primitiveChain.links.Count))
                subConfigurationIndexLookup.Add(subConfigurationConstraints[i].primitiveChain.links.Count, i);
    }

    public List<List<VertexConfiguration>> DetermineConstructions(PrimitiveChain chain) 
    { 
        List<List<VertexConfiguration>> constructions = new();
        HashSet<string> constructedSequences = new();
        if (subConfigurationIndexLookup.ContainsKey(chain.links.Count))
        {
            int startingIndex = subConfigurationIndexLookup[chain.links.Count];
            int endingIndex;
            if (subConfigurationIndexLookup.ContainsKey(chain.links.Count + 1))
                endingIndex = subConfigurationIndexLookup[chain.links.Count + 1];
            else
                endingIndex = subConfigurationConstraints.Count - 1;
            for (int i = startingIndex; i < endingIndex; i++)
            {
                if (subConfigurationConstraints[i].primitiveChain.Equals(chain))
                {
                    List<VertexConfiguration> newConstruction = new();
                    newConstruction.Add(subConfigurationConstraints[i]);
                    constructions.Add(newConstruction);
                }
            }
        }
        for (int i = 0; i < chain.links.Count - 1; i++)
        {
            PrimitiveChain leftSubdivide = chain.Subdivide(0, i + 1);
            List<List<VertexConfiguration>> leftConstructions = DetermineConstructions(leftSubdivide);
            PrimitiveChain rightSubdivide = chain.Subdivide(i + 1, chain.links.Count);
            List<List<VertexConfiguration>> rightConstructions = DetermineConstructions(rightSubdivide);
            if (leftConstructions == null || rightConstructions == null)
                continue;
            foreach (List<VertexConfiguration> leftConstruction in leftConstructions)
            {
                foreach (List<VertexConfiguration> rightConstruction in rightConstructions)
                {
                    List<VertexConfiguration> newConstruction = new(leftConstruction);
                    newConstruction.AddRange(rightConstruction);
                    string constructionSequence = VertexConfiguration.DetermineSequenceCode(newConstruction);
                    if (!constructedSequences.Contains(constructionSequence))
                    {
                        constructions.Add(newConstruction);
                        constructedSequences.Add(constructionSequence);
                    }
                }
            }
        }
        if (constructions.Count == 0)
            return null;
        return constructions;
    }

    public TypingChain YieldRandomReconstruction(VertexConfiguration vertConfig)
    {
        TypingChain reconstructedTyping = new();
        for (int i = 0; i < vertConfig.primitiveChain.links.Count; i++)
        {
            if (vertConfig.typingChain.terrainTypes[i] == TerrainType.None && vertConfig.typingChain.featureTypes[i] == FeatureType.None)
            {
                PrimitiveChain primitiveChain = ScriptableObject.CreateInstance<PrimitiveChain>();
                primitiveChain.links = new();
                primitiveChain.links.Add(vertConfig.primitiveChain.links[i]);
                List<PentaCoord> cycle = new();
                cycle.Add(vertConfig.cycle[i]);
                VertexConfiguration blankConfig = new(primitiveChain, cycle);
                while (i + 1 < vertConfig.primitiveChain.links.Count && 
                    (vertConfig.typingChain.terrainTypes[i + 1] == TerrainType.None && vertConfig.typingChain.featureTypes[i + 1] == FeatureType.None))
                {
                    i++;
                    blankConfig.primitiveChain.links.Add(vertConfig.primitiveChain.links[i]);
                    blankConfig.cycle.Add(vertConfig.cycle[i]);
                }
                TypingChain randomTyping = YieldRandomTyping(blankConfig);
                reconstructedTyping.terrainTypes.AddRange(randomTyping.terrainTypes);
                reconstructedTyping.featureTypes.AddRange(randomTyping.featureTypes);
            } else
            {
                reconstructedTyping.terrainTypes.Add(vertConfig.typingChain.terrainTypes[i]);
                reconstructedTyping.featureTypes.Add(vertConfig.typingChain.featureTypes[i]);
            }
        }
        return reconstructedTyping;
    }

    private TypingChain YieldRandomTyping(VertexConfiguration vertConfig)
    {
        List<VertexConfiguration> constrainedConfigs = new();
        if (!subConfigurationIndexLookup.ContainsKey(vertConfig.primitiveChain.links.Count))
            return YieldRandomlyConstructedTyping(vertConfig);
        int startingIndex = subConfigurationIndexLookup[vertConfig.primitiveChain.links.Count];
        int endingIndex;
        if (subConfigurationIndexLookup.ContainsKey(vertConfig.primitiveChain.links.Count + 1))
            endingIndex = subConfigurationIndexLookup[vertConfig.primitiveChain.links.Count + 1];
        else
            endingIndex = subConfigurationConstraints.Count - 1;
        for (int i = startingIndex; i < endingIndex; i++)
        {
            if (subConfigurationConstraints[i].primitiveChain.Equals(vertConfig.primitiveChain))
                constrainedConfigs.Add(subConfigurationConstraints[i]);
        }
        int chosenConfigIndex = Random.Range(0, constrainedConfigs.Count);
        if (constrainedConfigs.Count > 0)
            return constrainedConfigs[chosenConfigIndex].typingChain;
        else
            return YieldRandomlyConstructedTyping(vertConfig);
    }

    private TypingChain YieldRandomlyConstructedTyping(VertexConfiguration vertConfig)
    {
        List<List<VertexConfiguration>> constructions = DetermineConstructions(vertConfig.primitiveChain);
        if (constructions.Count == 0)
            throw new System.ArgumentOutOfRangeException("Vertex configuration " + vertConfig + " is not constructable.");
        return TypingChain.DecodeConstruction(constructions[RandomConstructionIndexOfMaxLength(constructions)]);
    }

    public int RandomConstructionIndexOfMaxLength(List<List<VertexConfiguration>> constructions)
    {
        List<int> potentialIndices = new();
        int longestLength = 0;
        for (int i = 0; i < constructions.Count; i++)
        {
            foreach (VertexConfiguration vertConfig in constructions[i])
            {
                if (vertConfig.primitiveChain.links.Count > longestLength)
                {
                    potentialIndices = new();
                    longestLength = vertConfig.primitiveChain.links.Count;
                }
                if (vertConfig.primitiveChain.links.Count == longestLength)
                    potentialIndices.Add(i);

            }
        }
        return potentialIndices[Random.Range(0, potentialIndices.Count)];
    }
}

public class WaveFunctionPossiblitySpace
{
    public Dictionary<VertexInd, List<TypingChain>> conformations { get; private set; }
    public Dictionary<VertexInd, List<List<VertexConfiguration>>> constructions { get; private set; }
    public Dictionary<VertexInd, int> entropyTable { get; private set; }
    public FibonacciHeap<VertexInd, VertexIndEqualityComparer, int> entropyQueue { get; private set; }
    public WaveFunctionConstraints constraints { get; private set; }

    public WaveFunctionPossiblitySpace(WaveFunctionConstraints constraints)
    {
        conformations = new(new VertexIndEqualityComparer());
        constructions = new(new VertexIndEqualityComparer());
        entropyTable = new();
        entropyQueue = new();
        this.constraints = constraints;
    }
}

[CreateAssetMenu(fileName = "ConstraintsGenerator", menuName = "ScriptableObjects/Utilities/ConstraintsGenerator")]
public class ConstraintsGenerator : ScriptableObject
{
    public PrimitiveChainSequencer sequencer;
    public PentagridVertexData inputGrid;
    public PentagridVertexData outputGrid;

    private WaveFunctionConstraints constraints;
    private WaveFunctionPossiblitySpace possibilitySpace;

    public void AcquireInputPentagridVertexData()
    {
        inputGrid = sequencer.SequenceInput();
    }

    public void GenerateContraints()
    {
        AcquireInputPentagridVertexData();
        constraints = new();
        foreach (VertexConfiguration vertConfig in inputGrid.vertexConfigurationTable.Values)
        {
            if (!constraints.adjacencyConstraints.ContainsKey(vertConfig))
                constraints.adjacencyConstraints.Add(vertConfig, new());
            foreach (VertexInd indices in vertConfig.adjacencies)
            {
                VertexConfiguration adjacentConfiguration = inputGrid.vertexConfigurationTable[indices];
                if (constraints.adjacencyConstraints[vertConfig].IndexOf(adjacentConfiguration) == -1)
                    constraints.adjacencyConstraints[vertConfig].Add(adjacentConfiguration);
            }
            if (!constraints.configurationConstraints.ContainsKey(vertConfig.primitiveChain))
                constraints.configurationConstraints.Add(vertConfig.primitiveChain, new());
            TypingChain vertTyping = new(vertConfig.typingChain);
            if (constraints.configurationConstraints[vertConfig.primitiveChain].IndexOf(vertTyping) == -1)
                constraints.configurationConstraints[vertConfig.primitiveChain].Add(vertTyping);
            foreach (VertexConfiguration subConfig in VertexConfiguration.DetermineSubConfigurations(vertConfig))
                if (constraints.subConfigurationConstraints.IndexOf(subConfig) == -1)
                    constraints.subConfigurationConstraints.Add(subConfig);
        }
        constraints.SortSubConfigurationConstraints();
        return;
    }

    public void AcquireOutputPentagridVertexData()
    {
        outputGrid = sequencer.SequenceOutput();
    }

    public WaveFunctionPossiblitySpace GeneratePossibilitySpace()
    {
        GenerateContraints();
        AcquireOutputPentagridVertexData();
        possibilitySpace = new(constraints);
        foreach (VertexInd indices in outputGrid.vertexConfigurationTable.Keys)
        {
            int entropy = CalculateInitialEntropy(indices);
            possibilitySpace.entropyTable.Add(indices, entropy);
            possibilitySpace.entropyQueue.Insert(indices, entropy);
        }
        return possibilitySpace;
    }

    private int CalculateInitialEntropy(VertexInd indices)
    {
        VertexConfiguration vertConfig = outputGrid.vertexConfigurationTable[indices];
        if (!constraints.configurationConstraints.ContainsKey(vertConfig.primitiveChain))
            return CalculateConstructionEntropy(indices);
        int constructionEntropy = CalculateConstructionEntropy(indices);
        List<TypingChain> possibleTypings = new();
        foreach (VertexInd adjacency in vertConfig.adjacencies)
        {
            PrimitiveChain adjacencyChain = outputGrid.vertexConfigurationTable[adjacency].primitiveChain;
            if (!constraints.configurationConstraints.ContainsKey(adjacencyChain))
                continue;
            foreach (TypingChain adjacencyTyping in constraints.configurationConstraints[adjacencyChain]) 
            {
                VertexConfiguration adjacencyConfig = new(adjacencyChain, adjacencyTyping, outputGrid.vertexConfigurationTable[adjacency].cycle);
                foreach (VertexConfiguration possibleConfig in constraints.adjacencyConstraints[adjacencyConfig])
                    if (possibleConfig.primitiveChain.Equals(vertConfig.primitiveChain) && possibleTypings.IndexOf(possibleConfig.typingChain) == -1)
                        possibleTypings.Add(possibleConfig.typingChain);
            }
        }
        if (possibleTypings.Count == 0)
            possibleTypings = new(constraints.configurationConstraints[vertConfig.primitiveChain]);
        possibilitySpace.conformations.Add(indices, possibleTypings);
        return possibleTypings.Count;
    }

    private int CalculateConstructionEntropy(VertexInd indices)
    {
        VertexConfiguration vertConfig = outputGrid.vertexConfigurationTable[indices];
        List<List<VertexConfiguration>> constructions = constraints.DetermineConstructions(vertConfig.primitiveChain);
        possibilitySpace.constructions.Add(indices, constructions);
        return constructions.Count;
    }

    public PentagridVertexData GetOutputGrid()
    {
        return outputGrid;
    }
}
