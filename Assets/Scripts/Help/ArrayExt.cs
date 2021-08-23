using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ArrayExt
{
    public static T[] GetRow<T>(this T[,] array, int row)
    {
        return Enumerable.Range(0, array.GetLength(1))
                .Select(x => array[row, x])
                .ToArray();
    }
}
