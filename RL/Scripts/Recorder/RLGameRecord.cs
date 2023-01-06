using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class RLGameRecord
{
    public string date;
    public string description;
    public List<Observation> observations;

    [Serializable]
    public class Observation {
        public StateSpace state;
        public float[] tensor;
    }
}
