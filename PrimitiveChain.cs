using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum Primitive 
{
    TkTk1,
    TkTk4,
    TkTn0,
    TkTn1,
    TkTn2,
    TkTn4,
    TnTn2,
    TnTn3,
    TnTk0,
    TnTk1,
    TnTk3,
    TnTk4
};

[CreateAssetMenu(fileName = "PrimitiveChain", menuName = "ScriptableObjects/DataContainers/PrimitiveChain")]
public class PrimitiveChain : ScriptableObject, IComparable<PrimitiveChain>, IEquatable<PrimitiveChain>
{
    public bool allThinRhombs = false;
    public List<Primitive> links;

    public void Copy(PrimitiveChain toCopy)
    {
        links = new(toCopy.links);
    }

    public bool IdentifiesAs(List<PentaCoord> cycle, Pentagrid pentagrid)
    {
        if (cycle.Count != links.Count + 1)
            return false;
        Primitive inspectedPrimitive = ClassifyPrimitive(cycle[0], cycle[1], pentagrid);
        List<int> potentialStarts = IndicesOf(inspectedPrimitive);
        if (potentialStarts.Count == 0)
            return false;
        foreach (int index in potentialStarts)
        {
            bool cyclePassed = true;
            for (int i = 0; i < links.Count - 1; i++)
            {
                inspectedPrimitive = ClassifyPrimitive(cycle[i + 1], cycle[i + 2], pentagrid);
                if (inspectedPrimitive != links[(index + i + 1) % links.Count])
                {
                    cyclePassed = false;
                    break;
                }
            }
            if (cyclePassed)
                return true;
        }
        return false;
    }

    public bool Contains(List<PentaCoord> walk, Pentagrid pentagrid)
    {
        if (walk.Count == 1)
        {
            if (PenroseTiles.DetermineType(walk[0], pentagrid) == TileType.ThickRhomb && allThinRhombs)
                return false;
            return true;
        }
        if (walk.Count > links.Count + 1)
            return false;
        Primitive inspectedPrimitive = ClassifyPrimitive(walk[0], walk[1], pentagrid);
        List<int> potentialStarts = IndicesOf(inspectedPrimitive);
        if (potentialStarts.Count == 0)
            return false;
        foreach (int index in potentialStarts)
        {
            bool walkPassed = true;
            for (int i = 0; i < walk.Count - 3; i++)
            {
                inspectedPrimitive = ClassifyPrimitive(walk[i + 1], walk[i + 2], pentagrid);
                if (inspectedPrimitive != links[(index + i + 1) % links.Count])
                {
                    walkPassed = false;
                    break;
                }
            }
            if (walkPassed)
                return true;
        }
        return false;
    }

    private List<int> IndicesOf(Primitive primitive)
    {
        List<int> indices = new();
        for (int i = 0; i < links.Count; i++)
        {
            if (links[i] == primitive)
                indices.Add(i);
        }
        return indices;
    }

    public PrimitiveChain Subdivide(int indexFromInclusive, int indexToExclusive)
    {
        PrimitiveChain subdivision = CreateInstance<PrimitiveChain>();
        subdivision.links = new();
        for (int i = indexFromInclusive; i < indexToExclusive; i++)
            subdivision.links.Add(links[i]);
        return subdivision;
    }

    public List<PentaCoord> AlignCycleToChain(List<PentaCoord> unalignedCycle, Pentagrid pentagrid)
    {
        if (unalignedCycle.Count != links.Count + 1)
            throw new IndexOutOfRangeException("Attempting to align cycle " + GeneralUtilities.CollectionToString(unalignedCycle) + " to chain " + ToString() + " when they differ in length.");
        Primitive inspectedPrimitive = ClassifyPrimitive(unalignedCycle[0], unalignedCycle[1], pentagrid);
        List<int> potentialStarts = IndicesOf(inspectedPrimitive);
        if (potentialStarts.Count == 0)
            throw new IndexOutOfRangeException("Attemping to align cycle " + GeneralUtilities.CollectionToString(unalignedCycle) + " to chain " + ToString() + " when they do not identify.");
        int indexMatch = 0;
        foreach (int index in potentialStarts)
        {
            bool cyclePassed = true;
            for (int i = 0; i < links.Count - 1; i++)
            {
                inspectedPrimitive = ClassifyPrimitive(unalignedCycle[i + 1], unalignedCycle[i + 2], pentagrid);
                if (inspectedPrimitive != links[(index + i + 1) % links.Count])
                {
                    cyclePassed = false;
                    break;
                }
            }
            if (cyclePassed)
            {
                indexMatch = index;
                break;
            }
        }
        List<PentaCoord> alignedCycle = new();
        for (int i = 0; i < unalignedCycle.Count; i++)
        {
            if ((unalignedCycle.Count + i - indexMatch - 1) % unalignedCycle.Count == unalignedCycle.Count - 1)
                continue;
            alignedCycle.Add(unalignedCycle[(unalignedCycle.Count + i - indexMatch - 1) % unalignedCycle.Count]);
        }
        alignedCycle.Add(unalignedCycle[(unalignedCycle.Count - indexMatch - 1) % unalignedCycle.Count]);
        return alignedCycle;
    }

    public static Primitive InvertPrimitive(Primitive primitive)
    {
        switch (primitive)
        {
            case Primitive.TkTk1:
                return Primitive.TkTk4;
            case Primitive.TkTk4:
                return Primitive.TkTk1;
            case Primitive.TkTn0:
                return Primitive.TnTk0;
            case Primitive.TkTn1:
                return Primitive.TnTk4;
            case Primitive.TkTn2:
                return Primitive.TnTk3;
            case Primitive.TkTn4:
                return Primitive.TnTk1;
            case Primitive.TnTn2:
                return Primitive.TnTn3;
            case Primitive.TnTn3:
                return Primitive.TnTn2;
            case Primitive.TnTk0:
                return Primitive.TkTn0;
            case Primitive.TnTk1:
                return Primitive.TkTn4;
            case Primitive.TnTk3:
                return Primitive.TkTn2;
            case Primitive.TnTk4:
                return Primitive.TkTn1;
            default:
                throw new IndexOutOfRangeException("Invalid Primitive passed into InvertPrimitive method");
        }
    }

    public static Primitive ClassifyPrimitive(PentaCoord pentaCoordFrom, PentaCoord pentaCoordTo, Pentagrid pentagrid)
    {
        int orderMod = (5 + PenroseTiles.DetermineOrder(pentaCoordTo, pentagrid) - PenroseTiles.DetermineOrder(pentaCoordFrom, pentagrid)) % 5;
        TileType fromType = PenroseTiles.DetermineType(pentaCoordFrom, pentagrid);
        TileType toType = PenroseTiles.DetermineType(pentaCoordTo, pentagrid);
        switch (fromType)
        {
            case TileType.ThinRhomb:
                switch (toType)
                {
                    case TileType.ThinRhomb:
                        switch (orderMod)
                        {
                            case 2:
                                return Primitive.TnTn2;
                            case 3:
                                return Primitive.TnTn3;
                            default:
                                throw new InvalidOperationException("Cannot classify " + fromType + " to " + toType + " with parity difference " + orderMod);
                        }
                    case TileType.ThickRhomb:
                        switch (orderMod)
                        {
                            case 0:
                                return Primitive.TnTk0;
                            case 1:
                                return Primitive.TnTk1;
                            case 3:
                                return Primitive.TnTk3;
                            case 4:
                                return Primitive.TnTk4;
                            default:
                                throw new InvalidOperationException("Cannot classify " + fromType + " to " + toType + " with parity difference " + orderMod);
                        }
                    default:
                        throw new IndexOutOfRangeException("Invalid toType in PrimitiveChain.Classify method");
                }
            case TileType.ThickRhomb:
                switch (toType)
                {
                    case TileType.ThinRhomb:
                        switch (orderMod)
                        {
                            case 0:
                                return Primitive.TkTn0;
                            case 1:
                                return Primitive.TkTn1;
                            case 2:
                                return Primitive.TkTn2;
                            case 4:
                                return Primitive.TkTn4;
                            default:
                                throw new InvalidOperationException("Cannot classify " + fromType + " to " + toType + " with parity difference " + orderMod);
                        }
                    case TileType.ThickRhomb:
                        switch (orderMod)
                        {
                            case 1:
                                return Primitive.TkTk1!;
                            case 4:
                                return Primitive.TkTk4;
                            default:
                                throw new InvalidOperationException("Cannot classify " + fromType + " to " + toType + " with parity difference " + orderMod);
                        }
                    default:
                        throw new IndexOutOfRangeException("Invalid toType in PrimitiveChain.Classify method");
                }
            default:
                throw new IndexOutOfRangeException("Invalid fromType in PrimitiveChain.Classify");
        }
    }

    public static TileType DetermineFromType(Primitive primitive)
    {
        switch (primitive)
        {
            case Primitive.TnTn2:
            case Primitive.TnTn3:
            case Primitive.TnTk0:
            case Primitive.TnTk1:
            case Primitive.TnTk3:
            case Primitive.TnTk4:
                return TileType.ThinRhomb;
            case Primitive.TkTk1:
            case Primitive.TkTk4:
            case Primitive.TkTn0:
            case Primitive.TkTn1:
            case Primitive.TkTn2:
            case Primitive.TkTn4:
                return TileType.ThickRhomb;
            default:
                throw new IndexOutOfRangeException("Invalid Primitve inspected in DetermineFromType method");
        }
    }

    public int CompareTo(PrimitiveChain otherChain)
    {
        if (otherChain == null || links.Count > otherChain.links.Count)
            return 1;
        if (links.Count == otherChain.links.Count)
            return 0;
        return -1;
    }

    public bool Equals(PrimitiveChain otherChain)
    {
        if (CompareTo(otherChain) != 0)
            return false;
        for (int i = 0; i < links.Count; i++)
            if (links[i] != otherChain.links[i])
                return false;
        return true;
    }

    public override string ToString()
    {
        string message = name + "(" + links[0].ToString();
        for (int i = 1; i < links.Count; i++)
            message += ">" + links[i].ToString();
        message += ")";
        return message;
    }
}

public class PrimitiveChainEqualityComparer : IEqualityComparer<PrimitiveChain>
{
    public bool Equals(PrimitiveChain chainA, PrimitiveChain chainB) 
    {
        return chainA.Equals(chainB);
    }

    public int GetHashCode(PrimitiveChain chain)
    {
        string chainString = chain.links[0].ToString();
        for (int i = 1; i < chain.links.Count; i++)
            chainString += "|" + chain.links[i].ToString();
        return chainString.GetHashCode();
    }
}
