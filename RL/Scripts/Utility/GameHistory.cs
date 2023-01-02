using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AIMove = ActionSpace.AIMove;

[DefaultExecutionOrder(300)]
public class GameHistory : MonoBehaviour
{
    public int length = 60;

    public ObjectBuffer<FrameData> p1FrameBuffer;
    public ObjectBuffer<FrameData> p2FrameBuffer;
    public PlayerState p1State;
    public PlayerState p2State;



    void Start()
    {
        p1FrameBuffer = new ObjectBuffer<FrameData>(length);
        p2FrameBuffer = new ObjectBuffer<FrameData>(length);
    }

    void FixedUpdate()
    {
        var p1FrameData = new FrameData {
            aiMove = RLUtility.GetCurrentMove(1),
        };
        var p2FrameData = new FrameData {
            aiMove = RLUtility.GetCurrentMove(2),
        };
        p1FrameBuffer.Add(p1FrameData);
        p2FrameBuffer.Add(p2FrameData);

        UpdatePlayerState(1);
        UpdatePlayerState(2);
    }

    private void UpdatePlayerState(int player) {
        var playerState = new PlayerState();
        var frameBuffer = player == 1 ? p1FrameBuffer : p2FrameBuffer;

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
        public AIMove aiMove;
    }

    public class PlayerState {
        public int movingBackFrames;
        public int movingForwardFrames;
        public int crouchingBackFrames;
    }
}
