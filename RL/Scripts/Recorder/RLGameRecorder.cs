using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UFE3D;
using System.Linq;

using FrameData = RLGameRecord.FrameData;
using AIMove = ActionSpace.AIMove;

public class RLGameRecorder : GameRecorder
{
    private RLGameRecord record;
    private int frameRoundStarted;

    protected override void InitializeRecord(int frameRoundStarted) {
        this.frameRoundStarted = frameRoundStarted;

        record = new RLGameRecord();
        record.UUID = Guid.NewGuid().ToString();
        record.date = DateTime.Now.ToString();
        record.description = UFEExtension.Instance.ExtensionInfo.description;
        record.frames = new List<FrameData>();
    }

    protected override void RecordCurrentFrame(int currentFrame) {
        var history = UFE.fluxCapacitor.History;
        var pair = new KeyValuePair<FluxStates, FluxFrameInput>();

        if (!history.TryGetStateAndInput(currentFrame, out pair)) {
            Debug.LogError("Failed to get current game state.");
            return;
        }

        var p1CharacterStateHistory = pair.Key.allCharacterStates.First(elem => elem.playerNum == 1);
        var p2CharacterStateHistory = pair.Key.allCharacterStates.First(elem => elem.playerNum == 2);

        FrameData frame = new FrameData();

        frame.currentFrame = currentFrame - frameRoundStarted;

        frame.gameState = new RLGameRecord.GameState {
            normalizedDistance = (float)p1CharacterStateHistory.normalizedDistance
        };

        frame.p1GameState = new RLGameRecord.CharacterState {
            life = (int)p1CharacterStateHistory.life,
            isDown = p1CharacterStateHistory.currentState == PossibleStates.Down,
            isJumping = (p1CharacterStateHistory.currentState == PossibleStates.BackJump || p1CharacterStateHistory.currentState == PossibleStates.NeutralJump || p1CharacterStateHistory.currentState == PossibleStates.ForwardJump)
                                && p1CharacterStateHistory.currentSubState != SubStates.Stunned,
            isBlocking = p1CharacterStateHistory.isBlocking,
            frameAdvantage = UFEUtility.CalcFrameAdvantage(1),
        };
        frame.p2GameState = new RLGameRecord.CharacterState {
            life = (int)p2CharacterStateHistory.life,
            isDown = p2CharacterStateHistory.currentState == PossibleStates.Down,
            isJumping = (p2CharacterStateHistory.currentState == PossibleStates.BackJump || p2CharacterStateHistory.currentState == PossibleStates.NeutralJump || p2CharacterStateHistory.currentState == PossibleStates.ForwardJump)
                                && p2CharacterStateHistory.currentSubState != SubStates.Stunned,
            isBlocking = p2CharacterStateHistory.isBlocking,
            frameAdvantage = UFEUtility.CalcFrameAdvantage(2),
        };

        frame.p1Action = RLUtility.GetCurrentMove(1).ToString();
        frame.p2Action = RLUtility.GetCurrentMove(2).ToString();

        StateSpace state = new StateSpace();
        state.SetValues(1);
        frame.p1StateVector = state.GetTensor();
        state.SetValues(2);
        frame.p2StateVector = state.GetTensor();

        int actionSize = Enum.GetNames(typeof(AIMove)).Length;
        float[] p1ActionVector = new float[actionSize];
        float[] p2ActionVector = new float[actionSize];
        p1ActionVector[(int)RLUtility.GetCurrentMove(1)] = 1;
        p2ActionVector[(int)RLUtility.GetCurrentMove(2)] = 1;
        frame.p1ActionVector = p1ActionVector;
        frame.p2ActionVector = p2ActionVector;

        record.frames.Add(frame);
    }

    protected override void SaveRecord(string savePath) {
        string json = JsonUtility.ToJson(record, true);
        string fileName = $"game_recording_{record.UUID}.json";

        if (savePath[savePath.Length - 1] != '/') savePath += "/";

        if (Directory.Exists(savePath)) {
            File.WriteAllText(savePath + fileName, json);
        }
        else {
            Debug.LogError("Couldn't save a game recording because the folder does not exist.");
        }
    }
}
