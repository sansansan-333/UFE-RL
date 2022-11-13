using System.Collections;
using System.Collections.Generic;
using UFE3D;
using UnityEngine;

// gets input from ALAgent and sends them to UFE 
public class RLAI : BaseAI
{
    public RLAgent brain;

    // UFE info
    public int opponent;
    public ControlsScript cScript;
    public ControlsScript opCScript;

    public override void Initialize(IEnumerable<InputReferences> inputReferences) {
        base.Initialize(inputReferences);

        if (player == 1) {
            brain = RLHelper.Instance.p1Brain;
            opponent = 2;
        }
        else {
            brain = RLHelper.Instance.p2Brain;
            opponent = 1;
        }
        brain.SetRLAI(this);
    }

    
    public override void DoUpdate() {
        if (cScript == null) {
            cScript = UFE.GetControlsScript(player);
        }
        if (opCScript == null) {
            opCScript = UFE.GetControlsScript(opponent);
        }
        if (cScript == null || opCScript == null) return;

        brain.RequestDecision();

        // now new actions are available
        // use them to decide input
        this.inputs.Clear();
        foreach (InputReferences input in this.inputReferences) {
            this.inputs[input] = this.ReadInput(input);
        }
    }

    public override InputEvents ReadInput(InputReferences inputReference) {
        if (!ActionSpace.usedButtons.Contains(inputReference.engineRelatedButton)) return InputEvents.Default;

        if (inputReference.inputType == InputType.Button) {
            if (brain.action.inputs[inputReference.engineRelatedButton] == 1) return new InputEvents(true);
        }

        else if (inputReference.inputType == InputType.VerticalAxis) {
            float axis = 0;
            if (brain.action.inputs[ButtonPress.Up] == 1) axis += 1;
            if (brain.action.inputs[ButtonPress.Down] == 1) axis -= 1;

            return new InputEvents(axis);
        }

        else if (inputReference.inputType == InputType.HorizontalAxis) {
            float axis = 0;
            if (brain.action.inputs[ButtonPress.Forward] == 1) axis += 1;
            if (brain.action.inputs[ButtonPress.Back] == 1) axis -= 1;

            // (mirror == 1) -> character faces to the left
            // (mirror == -1) -> character faces to the right
            axis *= (cScript.mirror * -1);

            return new InputEvents(axis);
        }

        return InputEvents.Default;
    }
}
