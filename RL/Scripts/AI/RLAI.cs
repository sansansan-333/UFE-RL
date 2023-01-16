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

    // move
    private int inputTick = 0;
    private ButtonPress[] currentFrameInput;
    private AIMove currentMove = AIMove.Neutral;
    private AIMove? storedMove = null;

    // decision
    public static readonly int reactionSpeed = 13;
    private bool decisionRequested = false;
    public StateSpace storedObservation { get; private set; }

    // debug
    private DataLogger logger = new DataLogger();

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

        storedObservation = new StateSpace();
    }

    
    public override void DoFixedUpdate() {
        if (UFE.config.lockInputs || UFE.config.lockMovements) return;

        if (cScript == null) cScript = UFE.GetControlsScript(player);
        if (opCScript == null) opCScript = UFE.GetControlsScript(opponent);
        if (cScript == null || opCScript == null) return;

        // get new move if any
        if (decisionRequested) {
            storedMove = brain.action.move;
        }

        // transition to stored move if possible
        if(storedMove is AIMove nextMove) {
            if(RLUtility.ValidateAIMove(player, nextMove) == 1 && inputTick >= ActionSpace.simulatedInput[currentMove].GetLength(0)) {
                currentMove = nextMove;
                storedMove = null;
                inputTick = 0;
            }
        }

        // decide input
        if (0 <= inputTick && inputTick < ActionSpace.simulatedInput[currentMove].GetLength(0)) {
            currentFrameInput = ActionSpace.simulatedInput[currentMove][inputTick];
        } else {
            currentFrameInput = ActionSpace.simulatedInput[AIMove.Neutral][0];
        }
        this.inputs.Clear();
        foreach (InputReferences input in this.inputReferences) {
            this.inputs[input] = this.ReadInput(input);
        }
        inputTick++;

        // request decision
        if (ReadyToDecideNextMove()) {
            brain.RequestDecision();
            storedObservation.SetValues(player);
            decisionRequested = true;
        } else {
            decisionRequested = false;
        }
    }

    private bool ReadyToDecideNextMove() {
        // if game has not started yet
        if (UFE.config.lockInputs || UFE.config.lockMovements || cScript == null || opCScript == null) return false;

        // if storedMove is not used yet
        if (storedMove != null) return false;

        if (RLUtility.IsBasicMove(RLUtility.GetCurrentMove(player))) {
            return true;
        } else if(cScript.currentMove != null && reactionSpeed - cScript.currentMove.currentFrame < 0) {
            return true;
        }

        return false;
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
