using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VertexConfiguration : IEquatable<VertexConfiguration>, IComparable<VertexConfiguration>
{
    public PrimitiveChain primitiveChain { get; private set; }
    public TypingChain typingChain { get; private set; }
    public List<VertexInd> adjacencies { get; private set; }
    public List<PentaCoord> cycle { get; private set; }

    public VertexConfiguration(PrimitiveChain primitiveChain, List<PentaCoord> cycle)
    {
        this.primitiveChain = primitiveChain;
        typingChain = new();
        adjacencies = new();
        this.cycle = cycle;
    }

    public VertexConfiguration(PrimitiveChain primitiveChain, TypingChain typingChain, List<PentaCoord> cycle)
    {
        this.primitiveChain = primitiveChain;
        this.typingChain = new(typingChain);
        adjacencies = new();
        this.cycle = cycle;
    }

    public VertexConfiguration(VertexConfiguration vertConfig)
    {
        primitiveChain = ScriptableObject.CreateInstance<PrimitiveChain>();
        primitiveChain.Copy(vertConfig.primitiveChain);
        typingChain = new(vertConfig.typingChain);
        adjacencies = new(vertConfig.adjacencies);
        cycle = new(vertConfig.cycle);
    }

    public void Set(TypingChain typingChain)
    {
        this.typingChain = typingChain;
    }

    public void AlignChains()
    {
        List<TileType> typeSequence = new();
        foreach (Primitive primitive in primitiveChain.links)
            typeSequence.Add(PrimitiveChain.DetermineFromType(primitive));
        int indexOffset = 0;
        for (int i = 0; i < typeSequence.Count; i++)
        {
            bool sequencePassed = true;
            for (int j = 0; j < typeSequence.Count; j++)
            {
                if (typeSequence[j] == TileType.ThickRhomb && typingChain.terrainTypes[(i + j) % typingChain.terrainTypes.Count] == TerrainType.None ||
                    typeSequence[j] == TileType.ThinRhomb && typingChain.featureTypes[(i + j) % typingChain.featureTypes.Count] == FeatureType.None)
                {
                    sequencePassed = false;
                    break;
                }                    
            }
            if (sequencePassed)
            {
                indexOffset = i;
                break;
            }
        }
        if (indexOffset == 0)
                return;
        TypingChain alignedTypingChain = new();
        for (int i = 0; i < typingChain.terrainTypes.Count; i++)
        {
            alignedTypingChain.terrainTypes.Add(typingChain.terrainTypes[(indexOffset + i) % typingChain.terrainTypes.Count]);
            alignedTypingChain.featureTypes.Add(typingChain.featureTypes[(indexOffset + i) % typingChain.featureTypes.Count]);
        }
        typingChain = alignedTypingChain;
    }

    public void ClipAt(int index)
    {
        PrimitiveChain clippedPrimitiveChain = ScriptableObject.CreateInstance<PrimitiveChain>();
        clippedPrimitiveChain.links = new();
        TypingChain clippedTypingChain = new();
        for (int i = 1; i < primitiveChain.links.Count; i++)
        {
            clippedPrimitiveChain.links.Add(primitiveChain.links[(index + i) % primitiveChain.links.Count]);
            clippedTypingChain.terrainTypes.Add(typingChain.terrainTypes[(index + i) % typingChain.terrainTypes.Count]);
            clippedTypingChain.featureTypes.Add(typingChain.featureTypes[(index + i) % typingChain.featureTypes.Count]);
        }
        primitiveChain = clippedPrimitiveChain;
        typingChain = clippedTypingChain;
    }

    public bool Equals(VertexConfiguration other)
    {
        return primitiveChain.Equals(other.primitiveChain) && typingChain.Equals(other.typingChain);
    }

    public override string ToString()
    {
        return primitiveChain.ToString() + " | " + typingChain.ToString();
    }

    public static List<VertexConfiguration> DetermineSubConfigurations(VertexConfiguration vertConfig)
    {
        List<VertexConfiguration> subConfigs = new();
        for (int i = 0; i < vertConfig.primitiveChain.links.Count; i++)
        {
            VertexConfiguration subConfig = new(vertConfig);
            subConfig.ClipAt(i);
            if (subConfig.primitiveChain.links.Count == 0)
                continue;
            subConfigs.Add(subConfig);
            List<VertexConfiguration> subSubConfigs = DetermineSubConfigurations(subConfig);
            foreach (VertexConfiguration subSubConfig in subSubConfigs)
                if (subConfigs.IndexOf(subSubConfig) == -1)
                    subConfigs.Add(subSubConfig);
        }
        return subConfigs;
    }

    public static string DetermineSequenceCode(List<VertexConfiguration> vertConfigs)
    {
        string code = "";
        foreach (VertexConfiguration vertConfig in vertConfigs)
            code += vertConfig.typingChain.ToString();
        return code;
    }

    public int CompareTo(VertexConfiguration vertConfig)
    {
        return primitiveChain.CompareTo(vertConfig.primitiveChain);
    }
}

public class VertexConfigurationEqualityComparer : IEqualityComparer<VertexConfiguration>
{
    public bool Equals(VertexConfiguration vertConfigA, VertexConfiguration vertConfigB)
    {
        return vertConfigA.Equals(vertConfigB);
    }

    public int GetHashCode(VertexConfiguration vertConfig)
    {
        string hash = vertConfig.ToString();
        return hash.GetHashCode();
    }
}
