using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WaveFunctionNode : IComparable<WaveFunctionNode>
{
    public int entropy { get; private set;}

    public WaveFunctionNode()
    {
        entropy = 0;
    }

    public WaveFunctionNode(int entropy)
    {
        this.entropy = entropy;
    }

    public void SetEntropy(int entropy)
    {
        this.entropy = entropy;
    }

    public int CompareTo(WaveFunctionNode other)
    {
        return entropy.CompareTo(other.entropy);
    }
}

public class WaveFunctionCollapse
{

}
