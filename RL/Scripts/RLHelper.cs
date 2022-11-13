using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UFE3D;

public class RLHelper : SingletonMonoBehaviour<RLHelper>
{
    public RLAgent p1Brain { get; private set; }
    public RLAgent p2Brain { get; private set; }

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
    }

    void Update()
    {
        // For battles to continue automatically, select "Repeat Battle" right after the battle ends
        if(UFE.currentScreen is VersusModeAfterBattleScreen) {
            var repeatBattleButton = GameObject.Find("Button_Repeat_Battle");
            if (repeatBattleButton != null) repeatBattleButton.GetComponent<Button>().onClick.Invoke();
        }


        var p1 = UFE.GetPlayer1ControlsScript();
        var p2 = UFE.GetPlayer2ControlsScript();
        if(p1 != null) {
            // Debug.Log(p1.currentState);
        }
        if(p2 != null) {
            // Debug.Log(p2.normalizedDistance);
        }

        UFE.OnRoundEnds += (ControlsScript winner, ControlsScript loser) => {
            //Debug.Log(winner + " won!");
        };
    }
}
