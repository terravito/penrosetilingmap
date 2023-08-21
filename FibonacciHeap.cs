using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class HeapNode<TId, TKey> : IComparable<HeapNode<TId, TKey>> where TId : IEquatable<TId> where TKey : IComparable
{
    public TId id { get; private set; }
    public  TKey key { get; private set; }
    private HeapNode<TId, TKey> parent;
    private List<HeapNode<TId, TKey>> children;
    public bool flagged { get; private set; }

    public HeapNode(TId id, TKey key)
    {
        this.id = id;
        this.key = key;
        children = new();
    }

    public int CompareTo(HeapNode<TId, TKey> otherNode)
    {
        return key.CompareTo(otherNode.key);
    }

    public void SetParent(HeapNode<TId, TKey> otherNode)
    {
        parent = otherNode;
    }

    public HeapNode<TId, TKey> GetParent()
    {
        return parent;
    }

    public void MakeRoot()
    {
        parent = null;
    }

    public bool IsRoot()
    {
        return parent == null;
    }

    public void AddChild(HeapNode<TId, TKey> child)
    {
        children.Add(child);
    }

    public void RemoveChild(HeapNode<TId, TKey> child)
    {
        children.Remove(child);
    }

    public List<HeapNode<TId, TKey>> GetChildren()
    {
        return children;
    }

    public int GetDegree()
    {
        return children.Count;
    }

    public void SetKey(TKey key)
    {
        this.key = key;
    }

    public void Flag()
    {
        flagged = true;
    }

    public void ClearFlag()
    {
        flagged = false;
    }

    public override string ToString()
    {
        return id.ToString() + " | " + key.ToString();
    }
}

public class FibonacciHeap<TId, TEq, TKey> where TId: IEquatable<TId> where TEq: IEqualityComparer<TId> where TKey : IComparable
{
    public int Count { get; private set;}

    private List<HeapNode<TId, TKey>> rootList;
    private HeapNode<TId, TKey> min;
    private Dictionary<TId, HeapNode<TId, TKey>> heapNodeLookup;

    public FibonacciHeap() 
    {
        rootList = new();
        Count = 0;
        heapNodeLookup = new();
    }

    public FibonacciHeap(TEq equalityComparer)
    {
        rootList = new();
        Count = 0;
        heapNodeLookup = new(equalityComparer);
    }

    public FibonacciHeap(TId id, TKey key)
    {
        rootList = new();
        HeapNode<TId, TKey> node = new(id, key);
        heapNodeLookup.Add(id, node);
        rootList.Add(node);
        Count = 1;
    }

    public void Insert(TId id, TKey key)
    {
        HeapNode<TId, TKey> node = new(id, key);
        heapNodeLookup.Add(id, node);
        rootList.Add(node);
        Count++;
        if (min == null || node.CompareTo(min) < 0)
            min = node;
    }

    public TId InspectMin()
    {
        return min.id;
    }

    public HeapNode<TId, TKey> InspectNode(TId id)
    {
        return heapNodeLookup[id];
    }

    public bool ContainsItem(TId id)
    {
        return heapNodeLookup.ContainsKey(id);
    }

    public TId ExtractMin()
    {
        if (Count == 0) 
            throw new IndexOutOfRangeException("Attempting to extract min from empty heap.");
        foreach (HeapNode<TId, TKey> child in min.GetChildren())
        {
            child.MakeRoot();
            rootList.Add(child);
        }
        rootList.Remove(min);
        heapNodeLookup.Remove(min.id);
        Count--;
        HeapNode<TId, TKey> extract = min;
        if (Count > 0) 
            CleanUp();
        return extract.id;
    }

    public void DecreaseKey(TId id, TKey key)
    {
        if (!heapNodeLookup.ContainsKey(id))
            return;
        HeapNode<TId, TKey> node = heapNodeLookup[id];
        node.SetKey(key);
        MinCheck(node);
        if (!node.IsRoot() && node.CompareTo(node.GetParent()) < 0)
            Trim(node);
    }

    public void Remove(TId id)
    {
        if (!heapNodeLookup.ContainsKey(id))
            return;
        HeapNode<TId, TKey> node = heapNodeLookup[id];
        if (!node.IsRoot())
            Trim(node); 
        foreach (HeapNode<TId, TKey> child in node.GetChildren())
        {
            child.MakeRoot();
            rootList.Add(child);
        }
        rootList.Remove(node);
        heapNodeLookup.Remove(node.id);
        Count--;
        if (Count > 0)
            CleanUp();
    }

    private void Trim(HeapNode<TId, TKey> node)
    {
        if (node.IsRoot())
            throw new IndexOutOfRangeException("Attempting to trim a root with Id: " + node.id);
        HeapNode<TId, TKey> parent = node.GetParent();
        if (!parent.flagged)
        {
            if (!parent.IsRoot())
                parent.Flag();
            parent.RemoveChild(node);
            MakeRoot(node);
        } else
        {
            parent.RemoveChild(node);
            MakeRoot(node);
            Trim(parent);
        }
    }

    private void MakeRoot(HeapNode<TId, TKey> node)
    {
        node.MakeRoot();
        rootList.Add(node);
        node.ClearFlag();
    }

    private void MinCheck(HeapNode<TId, TKey> node)
    {
        if (min == null || node.CompareTo(min) < 0)
            min = node;
    }

    public void MergeWithHeap(FibonacciHeap<TId, TEq, TKey> fibonacciHeap)
    {
        rootList.AddRange(fibonacciHeap.GetRootList());
    }

    public List<HeapNode<TId, TKey>> GetRootList()
    {
        return rootList;
    }

    private void CleanUp()
    {
        int maxDegree = Mathf.FloorToInt(Mathf.Log(Count) / Mathf.Log(2)) + 1;
        HeapNode<TId, TKey>[] rootArray = new HeapNode<TId, TKey>[maxDegree + 2];
        foreach (HeapNode<TId, TKey> root in rootList)
        {
            int index = root.GetDegree();
            BubbleRight(index, root, rootArray);
        }
        ResetRootList(rootArray);
    }

    private void BubbleRight(int index, HeapNode<TId, TKey> node, HeapNode<TId, TKey>[] nodeArray)
    {
        HeapNode<TId, TKey> toMerge;
        try
        {
            toMerge = nodeArray[index];
        } catch (IndexOutOfRangeException e)
        {
            Console.WriteLine(e.Message);
            throw new IndexOutOfRangeException("Attempting to access node at index " + index + " within nodeArray of size " + nodeArray.Length);
        }
        if (toMerge == null)
        {
            nodeArray[index] = node;
            return;
        }
        nodeArray[index] = null;
        BubbleRight(index + 1, MergeTrees(node, toMerge), nodeArray);
    }

    private HeapNode<TId, TKey> MergeTrees(HeapNode<TId, TKey> rootA, HeapNode<TId, TKey> rootB)
    {
        HeapNode<TId, TKey> newRoot;
        if (rootA.CompareTo(rootB) < 0)
        {
            newRoot = rootA;
            rootA.AddChild(rootB);
            rootB.SetParent(rootA);
        }
        else
        {
            newRoot = rootB;
            rootB.AddChild(rootA);
            rootA.SetParent(rootB);
        }
        return newRoot;
    }

    private void ResetRootList(HeapNode<TId, TKey>[] rootArray)
    {
        rootList = new();
        HeapNode<TId, TKey> tempMin = FirstNonNull(rootArray);
        foreach (HeapNode<TId, TKey> root in rootArray)
        {
            if (root == null)
                continue;
            root.ClearFlag();
            rootList.Add(root);
            if (root.CompareTo(tempMin) < 0)
                tempMin = root;
        }
        min = tempMin;
    }

    private HeapNode<TId, TKey> FirstNonNull(HeapNode<TId, TKey>[] array)
    {
        foreach (HeapNode<TId, TKey> node in array)
            if (node != null)
                return node;
        throw new IndexOutOfRangeException("Array full of nulls passed into FirstNonNull() method.");
    }
}

