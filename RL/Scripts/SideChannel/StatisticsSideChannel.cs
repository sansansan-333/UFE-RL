using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.SideChannels;
using System.Text;
using System;

public class StatisticsSideChannel : SideChannel {
    private float agentWinCount;
    private float opponentWinCount;

    public StatisticsSideChannel() {
        ChannelId = new Guid("ad041eb0-4a51-451f-a363-bb3a1a5d47bc");
    }

    protected override void OnMessageReceived(IncomingMessage msg) {
        var receivedString = msg.ReadString();
        Debug.Log("From Python : " + receivedString);
    }

    public void SendStatisticalDataToPython(bool agentWon) {
        if(agentWon) {
            agentWinCount++;
        } else {
            opponentWinCount++;
        }

        using (var msgOut = new OutgoingMessage()) {
            msgOut.WriteFloatList(new float[2] { 
                agentWinCount, 
                opponentWinCount 
            });
            QueueMessageToSend(msgOut);
        }
    }
}