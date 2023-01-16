using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class RLGameRecord
{
    public string date;
    public string description;
    public List<TimeStep> observations;

    [Serializable]
    public class TimeStep {
        public StateSpace state;
        public float[] stateTensor;
        public float[] action;

        public TimeStep() {
            state = new StateSpace();
            stateTensor = new float[0];
            action = new float[0];
        }
    }
}
