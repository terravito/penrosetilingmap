using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GeneralUtilities
{
    public static TCollection QuickSort<TCollection, TComparable>(TCollection collection, int leftIndex, int rightIndex) 
        where TCollection : IList
        where TComparable : IComparable<TComparable>
    {
        int i = leftIndex;
        int j = rightIndex;
        TComparable pivot = (TComparable)collection[leftIndex];

        while (i <= j)
        {
            TComparable inspectedElement = (TComparable)collection[i];
            while (inspectedElement.CompareTo(pivot) < 0)
            {
                i++;
                inspectedElement = (TComparable)collection[i];
            }

            inspectedElement = (TComparable)collection[j];
            while (inspectedElement.CompareTo(pivot) > 0)
            {
                j--;
                inspectedElement = (TComparable)collection[j];
            }

            if (i <= j)
            {
                TComparable temp = (TComparable)collection[i];
                collection[i] = collection[j];
                collection[j] = temp;
                i++;
                j--;
            }
        }

        if (leftIndex < j)
            QuickSort<TCollection, TComparable>(collection, leftIndex, j);

        if (i < rightIndex)
            QuickSort<TCollection, TComparable>(collection, i, rightIndex);

        return collection;
    }

    public static List<T> ReverseList<T>(List<T> list)
    {
        List<T> reversed = new();
        for (int i = list.Count - 1; i >= 0; i--)
        {
            reversed.Add(list[i]);
        }
        return reversed;
    }

    public static string CollectionToString<T> (T collection) where T : IEnumerable
    {
        string message = "";
        foreach (object obj in collection)
            message += obj.ToString() + " ";
        return message;
    }
}
