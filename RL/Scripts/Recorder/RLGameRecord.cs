using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class RLGameRecord
{
    // unique id
    public string UUID;
    // date
    public string date;
    // description
    public string description;
    // frame data
    public List<FrameData> frames;

    [Serializable]
    public class FrameData {
        public int currentFrame;
        public GameState gameState;
        public CharacterState p1GameState;
        public CharacterState p2GameState;
        public string p1Action;
        public string p2Action;

        public float[] p1StateVector;
        public float[] p2StateVector;
        public float[] p1ActionVector;
        public float[] p2ActionVector;
    }

    [Serializable]
    public class GameState {
        public float normalizedDistance;
    }

    [Serializable]
    public class CharacterState {
        public int life;
        public bool isDown;
        public bool isJumping;
        public bool isBlocking;
        public int frameAdvantage;
    }
}
