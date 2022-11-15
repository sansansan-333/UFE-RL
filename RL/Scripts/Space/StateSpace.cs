using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using UFE3D;

public class StateSpace: ISpace
{
    public PlayerState selfState;
    public PlayerState opponentState;
    public GameState gameState;

    public StateSpace() {
        selfState = new PlayerState();
        opponentState = new PlayerState();
        gameState = new GameState();
    }

    public void SetValues(int player) {
        var myCScript = UFE.GetControlsScript(player);
        var opCScript = UFE.GetControlsScript(player == 1 ? 2 : 1);
        if (myCScript == null || opCScript == null) return;

        selfState.hp = (float)myCScript.currentLifePoints;
        selfState.isBlocking = myCScript.isBlocking;
        selfState.isDown = myCScript.currentState == PossibleStates.Down;
        selfState.isJumping = (myCScript.currentState == PossibleStates.BackJump || myCScript.currentState == PossibleStates.NeutralJump || myCScript.currentState == PossibleStates.ForwardJump)
                                && myCScript.currentSubState != SubStates.Stunned;

        opponentState.hp = (float)opCScript.currentLifePoints;
        opponentState.isBlocking = opCScript.isBlocking;
        opponentState.isDown = opCScript.currentState == PossibleStates.Down;
        opponentState.isJumping = (opCScript.currentState == PossibleStates.BackJump || opCScript.currentState == PossibleStates.NeutralJump || opCScript.currentState == PossibleStates.ForwardJump)
                                && opCScript.currentSubState != SubStates.Stunned;

        gameState.normalizedDistance = (float)myCScript.normalizedDistance;
        gameState.frameAdvangtage = UFEUtility.CalcFrameAdvantage(player);
    }

    public float[] GetTensor() {
        List<float> tensor = new List<float>();
        tensor.AddRange(selfState.GetTensor());
        tensor.AddRange(opponentState.GetTensor());
        tensor.AddRange(gameState.GetTensor());

        return tensor.ToArray();
    }

    public class PlayerState: ISpace {
        public float hp;
        public bool isDown;
        public bool isJumping;
        public bool isBlocking;

        public static readonly float maxHP = 1000;

        public float[] GetTensor() {
            return new float[] { 
                hp / maxHP,
                isDown ? 1 : 0,
                isJumping ? 1 : 0,
                isBlocking ? 1 : 0,
            };
        }
    }

    public class GameState: ISpace {
        public float normalizedDistance;
        public float frameAdvangtage;

        public float[] GetTensor() {
            return new float[] {
                normalizedDistance,
                frameAdvangtage < 0 ? -1 : frameAdvangtage == 0 ? 0 : 1 // -1 or 0 or 1
            };
        }
    }
}
