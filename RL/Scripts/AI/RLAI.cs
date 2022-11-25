using System.Collections;
using System.Collections.Generic;
using UFE3D;
using UnityEngine;
using System.Linq;
using System;

// gets input from ALAgent and sends them to UFE 
public class RLAI : BaseAI
{
    public RLAgent brain;

    // UFE info
    public int opponent;
    public ControlsScript cScript;
    public ControlsScript opCScript;

    // Input
    private int moveTick = 0;
    private ButtonPress[] currentFrameInput;
    private bool moveFinished = true;
    private int c = 0;

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

    
    public override void DoFixedUpdate() {
        if (cScript == null) {
            cScript = UFE.GetControlsScript(player);
        }
        if (opCScript == null) {
            opCScript = UFE.GetControlsScript(opponent);
        }
        if (cScript == null || opCScript == null) return;

        // decide input
        currentFrameInput = ActionSpace.simulatedInput[brain.action.move][moveTick];
        this.inputs.Clear();
        foreach (InputReferences input in this.inputReferences) {
            this.inputs[input] = this.ReadInput(input);
        }

        // step move tick
        // mark finished if tick reaches the end of current move
        moveTick++;
        if (moveTick >= ActionSpace.simulatedInput[brain.action.move].GetLength(0)) {
            moveFinished = true;
            moveTick = 0;
        }

        // request decision if current move has finished
        if (moveFinished) {
            brain.RequestDecision();
            moveFinished = false;
        }
    }

    public override InputEvents ReadInput(InputReferences inputReference) {
        if (inputReference.inputType == InputType.Button) {
            foreach(var button in currentFrameInput) {
                if (button == inputReference.engineRelatedButton) return new InputEvents(true);
            }
        }

        else if (inputReference.inputType == InputType.VerticalAxis) {
            float axis = 0;
            foreach(var button in currentFrameInput) {
                if (button == ButtonPress.Up) axis += 1;
                if (button == ButtonPress.Down) axis -= 1;
            }

            return new InputEvents(axis);
        }

        else if (inputReference.inputType == InputType.HorizontalAxis) {
            float axis = 0;
            foreach (var button in currentFrameInput) {
                if (button == ButtonPress.Forward) axis += 1;
                if (button == ButtonPress.Back) axis -= 1;
            }

            // (mirror == 1) -> character faces to the left
            // (mirror == -1) -> character faces to the right
            axis *= (cScript.mirror * -1);

            return new InputEvents(axis);
        }

        return InputEvents.Default;
    }
}
