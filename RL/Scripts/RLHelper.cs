using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Unity.MLAgents.SideChannels;
using Unity.MLAgents;

using UFE3D;

public class RLHelper : SingletonMonoBehaviour<RLHelper>
{
    public RLAgent p1Brain { get; private set; }
    public RLAgent p2Brain { get; private set; }

    private DataLogger logger = new DataLogger();

    public GameHistory history;
    public StatisticsSideChannel statsChannel;

    void Awake()
    {
        p1Brain = GameObject.Find("p1Dammy").GetComponent<RLAgent>();
        p2Brain = GameObject.Find("p2Dammy").GetComponent<RLAgent>();

        // Destroy Agent if corresponding AI engine is not used
        if (UFEExtension.Instance.ExtensionInfo.overrideAI) {
            if (UFEExtension.Instance.ExtensionInfo.p1AIEngine.GetSystemType() != typeof(RLAI)) {
                Destroy(p1Brain.gameObject);
            }
            if (UFEExtension.Instance.ExtensionInfo.p2AIEngine.GetSystemType() != typeof(RLAI)) {
                Destroy(p2Brain.gameObject);
            }
        }

        history = gameObject.GetComponent<GameHistory>();

        statsChannel = new StatisticsSideChannel();
        SideChannelManager.RegisterSideChannel(statsChannel);
        
    }

    void FixedUpdate()
    {
        // click "Repeat Battle" button right after the battle ends for the battle to continue automatically
        if(UFE.currentScreen is VersusModeAfterBattleScreen) {
            var repeatBattleButton = GameObject.Find("Button_Repeat_Battle");
            if (repeatBattleButton != null) repeatBattleButton.GetComponent<Button>().onClick.Invoke();
        }

        var p1 = UFE.GetPlayer1ControlsScript();
        var p2 = UFE.GetPlayer2ControlsScript();
        if(p1 != null) {
            // Debug.Log(history.p1State.crouchingBackFrames);
        }
        if(p2 != null) {
            // Debug.Log(p2.normalizedDistance);
        }

        UFE.OnRoundEnds += (ControlsScript winner, ControlsScript loser) => {
            //Debug.Log(winner + " won!");
        };
    }

    public void OnDestroy() {
        if (Academy.IsInitialized) {
            SideChannelManager.UnregisterSideChannel(statsChannel);
        }
    }
}
