using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UFE3D;
using System;

public class RLUtility
{
    public static AIMove GetCurrentMove(int player) {
        var cScript = UFE.GetControlsScript(player);
        if (cScript == null) return AIMove.Neutral;

        if(cScript.currentMove == null) {
            // basic move
            if (cScript.currentBasicMove == BasicMoveReference.Idle) return AIMove.Neutral;
            if (cScript.currentBasicMove == BasicMoveReference.MoveForward) return AIMove.Forward;
            if (cScript.currentBasicMove == BasicMoveReference.MoveBack) return AIMove.Back;

            if (cScript.currentState == PossibleStates.NeutralJump) return AIMove.Jump_Vertical;
            if (cScript.currentState == PossibleStates.ForwardJump) return AIMove.Jump_Forward;
            if (cScript.currentState == PossibleStates.BackJump) return AIMove.Jump_Back;

            if (cScript.currentState == PossibleStates.Crouch && !cScript.potentialBlock) return AIMove.Crouch;
            if (cScript.currentState == PossibleStates.Crouch && cScript.potentialBlock) return AIMove.Crouch_Back;
        } else {
            // attack / special move
            // consider super jump as jump_vertical from basic move
            if (cScript.currentMove.moveName == "Super Jump") return AIMove.Jump_Vertical;

            if (cScript.currentMove.moveName == "Light Punch") return AIMove.Punch_Standing_Light;
            if (cScript.currentMove.moveName == "Heavy Punch") return AIMove.Punch_Standing_Heavy;
            if (cScript.currentMove.moveName == "Crouching Light Punch") return AIMove.Punch_Crouching_Light;
            if (cScript.currentMove.moveName == "Crouching Heavy Punch") return AIMove.Punch_Crouching_Heavy;
            if (cScript.currentMove.moveName == "Jumping Light Punch") return AIMove.Punch_Jumping_Light;
            if (cScript.currentMove.moveName == "Jumping Heavy Punch") return AIMove.Punch_Jumping_Heavy;

            if (cScript.currentMove.moveName == "Standing Light Kick") return AIMove.Kick_Standing_Light;
            if (cScript.currentMove.moveName == "Heavy Kick") return AIMove.Kick_Standing_Heavy;
            if (cScript.currentMove.moveName == "Crouching Light Kick") return AIMove.Kick_Crouching_Light;
            if (cScript.currentMove.moveName == "Crouching Heavy Kick") return AIMove.Kick_Crouching_Heavy;
            if (cScript.currentMove.moveName == "Jumping Light Kick") return AIMove.Kick_Jumping_Light;
            if (cScript.currentMove.moveName == "Jumping Heavy Kick") return AIMove.Kick_Jumping_Heavy;

            if (cScript.currentMove.moveName == "Dash Forward") return AIMove.Dash;
            if (cScript.currentMove.moveName == "Fire Ball Light") return AIMove.Fireball_Light;
            if (cScript.currentMove.moveName == "Fire Ball Heavy") return AIMove.Fireball_Heavy;
            if (cScript.currentMove.moveName == "Focus Start" || cScript.currentMove.moveName == "Focus Light" || cScript.currentMove.moveName == "Focus Medium" || cScript.currentMove.moveName == "Focus Unblockable") return AIMove.Focus;
            if (cScript.currentMove.moveName == "Wall Launcher") return AIMove.Wall_Launcher;
        }

        return AIMove.Neutral;
    }

    public static bool IsBasicMove(AIMove aiMove) {
        switch (aiMove) {
            case AIMove.Neutral:
            case AIMove.Forward:
            case AIMove.Back:
            case AIMove.Jump_Vertical:
            case AIMove.Jump_Forward:
            case AIMove.Jump_Back:
            case AIMove.Crouch:
            case AIMove.Crouch_Back:
                return true;
            default:
                return false;
        }
    }

    public static MoveInfo AIMoveToMoveInfo(AIMove aiMove) {
        if (IsBasicMove(aiMove)) return null;

        var character = UFE.GetPlayer1ControlsScript().myInfo;
        if(character.characterName != "Robot Kyle") {
            Debug.LogError("Choose Robot Kyle as player 1 character.");
            return null;
        }
        var cScript = UFE.GetPlayer1ControlsScript();
        if (cScript == null) return null;
        var moves = cScript.loadedMoves[0].attackMoves;

        switch (aiMove) {
            case AIMove.Punch_Standing_Light:
                return moves[8];
            case AIMove.Punch_Standing_Heavy:
                return moves[15];
            case AIMove.Punch_Crouching_Light:
                return moves[21];
            case AIMove.Punch_Crouching_Heavy:
                return moves[13];
            case AIMove.Punch_Jumping_Light:
                return moves[17];
            case AIMove.Punch_Jumping_Heavy:
                return moves[9];
            case AIMove.Kick_Standing_Light:
                return moves[19];
            case AIMove.Kick_Standing_Heavy:
                return moves[11];
            case AIMove.Kick_Crouching_Light:
                return moves[10];
            case AIMove.Kick_Crouching_Heavy:
                return moves[12];
            case AIMove.Kick_Jumping_Light:
                return moves[16];
            case AIMove.Kick_Jumping_Heavy:
                return moves[18];
            case AIMove.Dash:
                return moves[4];
            case AIMove.Fireball_Light:
                return moves[0];
            case AIMove.Fireball_Heavy:
                return moves[2];
            case AIMove.Focus:
                return moves[20]; // Focus Start
            case AIMove.Wall_Launcher:
                return moves[7];
            default:
                return null;
        }
    }

    public static int[] ValidateAIMoves(int player) {
        var aiMoves = Enum.GetValues(typeof(AIMove));
        int[] isTransitionable = new int[aiMoves.Length];

        foreach(AIMove move in aiMoves) {
            isTransitionable[(int)move] = ValidateAIMove(player, move);
        }

        return isTransitionable;
    }


    /// <summary>
    /// Checks if a player can perform the specified AIMove in the current frame.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="aiMove"></param>
    /// <returns>1 if it's possible. Otherwise 0.</returns>
    public static int ValidateAIMove(int player, AIMove aiMove) {
        var cScript = UFE.GetControlsScript(player);
        if (cScript == null) return 0;

        if (aiMove == AIMove.Neutral) return 1;

        if (IsBasicMove(aiMove)) {
            if (cScript.currentMove == null && cScript.storedMove == null) return 1;
        } else {
            var moveInfo = AIMoveToMoveInfo(aiMove);
            if (cScript.storedMove != null) return 0;
            else if (cScript.MoveSet.ValidateMoveExecution(moveInfo)) return 1;
        }

        return 0;
    }
}
