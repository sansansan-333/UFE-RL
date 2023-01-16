using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[DefaultExecutionOrder(250)] // between UFE.cs and GameRecorder.cs
public class GameHistory : MonoBehaviour
{
    public int length = 60;

    private ObjectBuffer<FrameData> p1FrameBuffer;
    private ObjectBuffer<FrameData> p2FrameBuffer;
    private PlayerState p1State;
    private PlayerState p2State;

    public ObjectBuffer<FrameData> GetFrameBuffer(int player) {
        if (player == 1) return p1FrameBuffer;
        else if (player == 2) return p2FrameBuffer;
        else throw new ArgumentException();
    }

    public PlayerState GetPlayerState(int player) {
        if (player == 1) return p1State;
        else if (player == 2) return p2State;
        else throw new ArgumentException();
    }

    void Start()
    {
        p1FrameBuffer = new ObjectBuffer<FrameData>(length);
        p2FrameBuffer = new ObjectBuffer<FrameData>(length);
    }

    void FixedUpdate()
    {
        UpdateFrameBuffer(1);
        UpdateFrameBuffer(2);

        UpdatePlayerState(1);
        UpdatePlayerState(2);
    }

    private void UpdateFrameBuffer(int player) {
        var cScript = UFE.GetControlsScript(player);
        var opCScript = UFE.GetControlsScript(player == 1 ? 2 : 1);
        if (cScript == null || opCScript == null) return;

        int currentFrame = (int)UFE.currentFrame - 1;
        var frameData = new FrameData();
        var frameBuffer = player == 1 ? p1FrameBuffer : p2FrameBuffer;
        var opFrameBuffer = player == 1 ? p2FrameBuffer : p1FrameBuffer;
        FrameData prevFrameData = new FrameData(), prevOpFrameData = new FrameData();
        for (int i = 0; i < frameBuffer.Count; i++) {
            if(frameBuffer[i].frame == currentFrame - 1) {
                prevFrameData = frameBuffer[i];
            }
        }
        for (int i = 0; i < opFrameBuffer.Count; i++) {
            if (opFrameBuffer[i].frame == currentFrame - 1) {
                prevOpFrameData = opFrameBuffer[i];
            }
        }

        frameData.frame = currentFrame;
        frameData.hp = (float)cScript.currentLifePoints;
        frameData.aiMove = RLUtility.GetCurrentMove(player);
        if(frameBuffer.Count > 0 && prevFrameData.hp != 0 && prevOpFrameData.hp != 0) {
            frameData.damageDealt = prevOpFrameData.hp - (float)opCScript.currentLifePoints;
            frameData.damageTaken = prevFrameData.hp - (float)cScript.currentLifePoints;
        } else {
            frameData.damageDealt = 0;
            frameData.damageTaken = 0;
        }
        frameData.blocking = cScript.currentSubState == SubStates.Blocking;

        frameBuffer.Add(frameData);
    }

    private void UpdatePlayerState(int player) {
        var playerState = new PlayerState();
        var frameBuffer = player == 1 ? p1FrameBuffer : p2FrameBuffer;

        if (frameBuffer.Count <= 0) return;

        playerState.movingBackFrames = 0;
        for (int i = 0; i < length; i++) {
            if (frameBuffer[i].aiMove == AIMove.Back) {
                playerState.movingBackFrames++;
            } else {
                break;
            }
        }

        playerState.movingForwardFrames = 0;
        for (int i = 0; i < length; i++) {
            if (frameBuffer[i].aiMove == AIMove.Forward) {
                playerState.movingForwardFrames++;
            } else {
                break;
            }
        }

        playerState.crouchingBackFrames = 0;
        for (int i = 0; i < length; i++) {
            if (frameBuffer[i].aiMove == AIMove.Crouch_Back) {
                playerState.crouchingBackFrames++;
            } else {
                break;
            }
        }

        if (player == 1) {
            p1State = playerState;
        } else {
            p2State = playerState;
        }
    }

    public class FrameData {
        public int frame;
        public float hp;
        public AIMove aiMove;
        public float damageDealt;
        public float damageTaken;
        public bool blocking;
    }

    public class PlayerState {
        public int movingBackFrames;
        public int movingForwardFrames;
        public int crouchingBackFrames;
    }
}
