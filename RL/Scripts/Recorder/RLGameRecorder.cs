using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UFE3D;
using System.Linq;

public class RLGameRecorder : GameRecorder {
    private RLGameRecord record;
    private int frameRoundStarted;
    private readonly int player = 1;

    protected override void InitializeRecord(int frameRoundStarted) {
        this.frameRoundStarted = frameRoundStarted;

        record = new RLGameRecord();
        record.date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        record.description = UFEExtension.Instance.ExtensionInfo.description;
        record.observations = new List<RLGameRecord.TimeStep>();
    }

    protected override void RecordCurrentFrame(int currentFrame) {
        if(IsDecisionMade()) {
            var observation = new RLGameRecord.TimeStep();
            observation.state.SetValues(player);
            observation.stateTensor = observation.state.ToTensor();
            observation.action = ActionSpace.GetTensorFromMove(RLUtility.GetCurrentMove(player));
            record.observations.Add(observation);
        }
    }

    // returns true if the player did some action in the current frame
    private bool IsDecisionMade() {
        var cScript = UFE.GetControlsScript(player);
        var opCScript = UFE.GetControlsScript(player == 1 ? 2 : 1);

        // if game has not started yet
        if (UFE.config.lockInputs || UFE.config.lockMovements || cScript == null || opCScript == null) return false;

        var history = RLHelper.Instance.history;
        var frameBuffer = history.GetFrameBuffer(player);

        // if last move was basic move
        if (RLUtility.IsBasicMove(frameBuffer[0].aiMove)) {
            return true;
        // if the player used different move from the previous frame
        } else if(frameBuffer[0].aiMove != frameBuffer[1].aiMove) {
            return true;
        }

        return false;
    }

    protected override void SaveRecord(string savePath) {
        string json = JsonUtility.ToJson(record, true);
        string fileName = $"game_recording_{record.date}.json";

        if (savePath[savePath.Length - 1] != '/') savePath += "/";

        if (Directory.Exists(savePath)) {
            File.WriteAllText(savePath + fileName, json);
        }
        else {
            Debug.LogError("Couldn't save a game recording because the folder does not exist.");
        }
    }
}
