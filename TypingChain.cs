using System.Collections.Generic;
using System;

public class TypingChain : IEquatable<TypingChain>
{
    public List<TerrainType> terrainTypes { get; private set; }
    public List<FeatureType> featureTypes { get; private set; }

    public TypingChain()
    {
        terrainTypes = new();
        featureTypes = new();
    }

    public TypingChain(TypingChain toCopy)
    {
        terrainTypes = new(toCopy.terrainTypes);
        featureTypes = new(toCopy.featureTypes);
    }

    public bool Equals(TypingChain other)
    {
        if (terrainTypes.Count != other.terrainTypes.Count)
            return false;
        List<int> potentialStarts;
        if (other.terrainTypes[0] != TerrainType.None)
            potentialStarts = IndicesOf(other.terrainTypes[0]);
        else
            potentialStarts = IndicesOf(other.featureTypes[0]);
        if (potentialStarts.Count == 0)
            return false;
        bool sequenceMatched = true;
        foreach (int index in potentialStarts)
        {
            sequenceMatched = true;
            for (int i = 0; i < terrainTypes.Count; i++)
            {
                if (terrainTypes[(index + i) % terrainTypes.Count] != other.terrainTypes[i] || featureTypes[(index + i) % featureTypes.Count] != other.featureTypes[i])
                {
                    sequenceMatched = false;
                    break;
                }
            }
            if (sequenceMatched)
                break;
        }
        return sequenceMatched;
    }

    private List<int> IndicesOf(TerrainType specTerrain)
    {
        List<int> indices = new();
        for (int i = 0; i < terrainTypes.Count; i++)
        {
            if (terrainTypes[i] == specTerrain)
                indices.Add(i);
        }
        return indices;
    }

    private List<int> IndicesOf(FeatureType specFeature)
    {
        List<int> indices = new();
        for (int i = 0; i < terrainTypes.Count; i++)
        {
            if (featureTypes[i] == specFeature)
                indices.Add(i);
        }
        return indices;
    }

    public override string ToString()
    {
        string message = "";
        for (int i = 0; i < terrainTypes.Count; i++)
            message += CodeSelector(terrainTypes[i]) + CodeSelector(featureTypes[i]);
        return message;
    }

    private string CodeSelector(TerrainType terrainType)
    {
        switch (terrainType)
        {
            case TerrainType.Grassland:
                return "G";
            case TerrainType.Desert:
                return "D";
            case TerrainType.Tundra:
                return "T";
            default:
                return "";
        }
    }

    private string CodeSelector(FeatureType featureType)
    {
        switch (featureType)
        {
            case FeatureType.Hillside:
                return "H";
            case FeatureType.Riverland:
                return "R";
            case FeatureType.Forest:
                return "F";
            default:
                return "";
        }
    }

    public bool ConformsTo(TypingChain partialTyping)
    {
        for (int i = 0; i < terrainTypes.Count; i++)
        {
            if (partialTyping.terrainTypes[i] != terrainTypes[i] && partialTyping.terrainTypes[i] != TerrainType.None)
                return false;
            if (partialTyping.featureTypes[i] != featureTypes[i] && partialTyping.featureTypes[i] != FeatureType.None)
                return false;
        }
        return true;
    }

    public bool IsComplete()
    {
        for (int i = 0; i < terrainTypes.Count; i++)
            if (terrainTypes[i] == TerrainType.None && featureTypes[i] == FeatureType.None)
                return false;
        return true;
    }

    public static TypingChain DecodeConstruction(List<VertexConfiguration> construction)
    {
        TypingChain constructionTyping = new();
        foreach (VertexConfiguration vertConfig in construction)
        {
            for (int i = 0; i < vertConfig.primitiveChain.links.Count; i++)
            {
                constructionTyping.terrainTypes.Add(vertConfig.typingChain.terrainTypes[i]);
                constructionTyping.featureTypes.Add(vertConfig.typingChain.featureTypes[i]);
            }
        }
        return constructionTyping;
    }
    public static List<TypingChain> DecodeConstructions(List<List<VertexConfiguration>> constructions)
    {
        List<TypingChain> constructionTypings = new();
        foreach (List<VertexConfiguration> construction in constructions)
            constructionTypings.Add(DecodeConstruction(construction));
        return constructionTypings;
    }
}
