using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISpace
{
    /// <summary>
    /// Returns all variables turned into one-dimentional tensor. 
    /// </summary>
    /// <remarks>
    /// This function should return a tensor that can immediately be used as an input of a model without any processing, such as normalization and trimming.
    /// </remarks>
    public float[] ToTensor();

    public int GetLength();
}
