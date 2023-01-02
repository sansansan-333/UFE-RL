using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using UFE3D;

public class StateSpace: ISpace
{
    public SelfState selfState;
    public OpponentState opponentState;
    public GameState gameState;

    public readonly int maxMovementFrames = 60;

    public StateSpace() {
        selfState = new SelfState();
        opponentState = new OpponentState();
        gameState = new GameState();
    }

    public void SetValues(int player) {
        var myCScript = UFE.GetControlsScript(player);
        var opCScript = UFE.GetControlsScript(player == 1 ? 2 : 1);
        if (myCScript == null || opCScript == null) return;

        var history = RLHelper.Instance.history;
        var playerState = player == 1 ? history.p1State : history.p2State;

        // selfState.hp = (float)myCScript.currentLifePoints;
        selfState.isJumping = (myCScript.currentState == PossibleStates.BackJump || myCScript.currentState == PossibleStates.NeutralJump || myCScript.currentState == PossibleStates.ForwardJump)
                                && myCScript.currentSubState != SubStates.Stunned;
        selfState.frameAdvantage = Mathf.Sign(UFEUtility.CalcFrameAdvantage(player));
        selfState.lastMove = ActionSpace.GetTensorFromMove(RLUtility.GetCurrentMove(player));
        selfState.movingBackDuration = (float)Mathf.Min(maxMovementFrames, playerState.movingBackFrames) / maxMovementFrames;
        selfState.movingForwardDuration = (float)Mathf.Min(maxMovementFrames, playerState.movingForwardFrames) / maxMovementFrames;
        selfState.crouchingBackDuration = (float)Mathf.Min(maxMovementFrames, playerState.crouchingBackFrames) / maxMovementFrames;

        opponentState.isBlocking = opCScript.isBlocking;
        opponentState.isDown = opCScript.currentState == PossibleStates.Down;
        opponentState.isJumping = (opCScript.currentState == PossibleStates.BackJump || opCScript.currentState == PossibleStates.NeutralJump || opCScript.currentState == PossibleStates.ForwardJump)
                                && opCScript.currentSubState != SubStates.Stunned;

        gameState.normalizedDistance = (float)myCScript.normalizedDistance;
    }

    public float[] GetTensor() {
        List<float> tensor = new List<float>();
        tensor.AddRange(selfState.GetTensor());
        tensor.AddRange(opponentState.GetTensor());
        tensor.AddRange(gameState.GetTensor());

        return tensor.ToArray();
    }

    public int GetLength() {
        return selfState.GetLength() + opponentState.GetLength() + gameState.GetLength();
    }

    public class SelfState: ISpace {
        public bool isJumping;
        public float frameAdvantage;
        public float[] lastMove; // one hot vector
        public float movingBackDuration;
        public float movingForwardDuration;
        public float crouchingBackDuration;

        public float[] GetTensor() {
            List<float> tensor = new List<float>();
            tensor.Add(isJumping ? 1 : 0);
            tensor.Add(frameAdvantage);
            tensor.AddRange(lastMove);
            tensor.Add(movingBackDuration);
            tensor.Add(movingForwardDuration);
            tensor.Add(crouchingBackDuration);

            return tensor.ToArray();
        }

        public int GetLength() {
            return 5 + lastMove.Length;
        }
    }

    public class OpponentState: ISpace {
        public bool isDown;
        public bool isJumping;
        public bool isBlocking;

        public float[] GetTensor() {
            return new float[] {
                isDown ? 1 : 0,
                isJumping ? 1 : 0,
                isBlocking ? 1 : 0,
            };
        }

        public int GetLength() {
            return 3;
        }
    }

    public class GameState: ISpace {
        public float normalizedDistance;

        public float[] GetTensor() {
            return new float[] {
                normalizedDistance,
            };
        }

        public int GetLength() {
            return 1;
        }
    }
}
