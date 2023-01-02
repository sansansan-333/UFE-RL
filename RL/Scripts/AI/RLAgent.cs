using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

using AIMove = ActionSpace.AIMove;

public class RLAgent : Agent
{
    private StateSpace observation;
    public ActionSpace action;
    public bool isNewActionSet { get; private set; }
    private RLAI rlAI;

    // rewards
    private readonly float initialHP = 1000;
    private readonly float maxDamage = 90;
    private float prevHP;
    private float prevOpHP;
    private bool episodeEndFlag;

    // inference
    private bool inference;
    private InferenceSession session;
    [PathAttribute] public string modelPath;
    private List<NamedOnnxValue> modelInputs;

    // debug
    private DataLogger logger = new DataLogger();
    private List<AIMove> moveBuffer;
    int i = 0;

    public void SetRLAI(RLAI rlAI) {
        this.rlAI = rlAI;
    }

    void Start()
    {
        observation = new StateSpace();
        action = new ActionSpace();
        isNewActionSet = false;

        Academy.Instance.OnEnvironmentReset += EnvironmentReset;

        if (Academy.Instance.IsCommunicatorOn) {
            inference = false;
        } else {
            if (File.Exists(modelPath)) {
                session = new InferenceSession(modelPath);
                inference = true;
            }
            else {
                Debug.LogWarning("Model was not found at the specified path. The game continues without sending any output to the agent.");
            }
        }

        moveBuffer = new List<AIMove> {
            AIMove.Neutral,
            AIMove.Neutral,
            AIMove.Neutral,
            AIMove.Neutral,
            AIMove.Neutral,
            AIMove.Neutral,
            AIMove.Neutral,
            AIMove.Neutral,
            AIMove.Neutral,
            AIMove.Neutral,
            AIMove.Neutral,
            AIMove.Neutral,
            AIMove.Neutral,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Back
        };
    }

    public void EnvironmentReset() {

    }

    public override void OnEpisodeBegin() {
        prevHP = initialHP;
        prevOpHP = initialHP;
    }

    private void FixedUpdate() {
        if (rlAI != null && rlAI.cScript != null && rlAI.opCScript != null && !episodeEndFlag) {
            // episode ends when someone wins
            episodeEndFlag = rlAI.cScript.isDead || rlAI.opCScript.isDead;
        }
    }

    public override void CollectObservations(VectorSensor sensor) {
        if (rlAI == null) return;

        observation = rlAI.storedObservation;

        if(inference) {
            string inputName = "dense_input";
            var inputDimension = session.InputMetadata[inputName].Dimensions;
            inputDimension[0] = 1; // batch size is 1 during inference

            float[] inputArray = observation.GetTensor();
            var inputTensor = new DenseTensor<float>(inputArray, inputDimension);
            modelInputs = new List<NamedOnnxValue> {
                NamedOnnxValue.CreateFromTensor(inputName, inputTensor),
            };
        } else {
            sensor.AddObservation(observation.GetTensor());

            Debug.Log(observation.GetLength());
        }
    }

    public override void OnActionReceived(ActionBuffers actions) {
        if(inference) {
            // get output from Q-network and translate it to action
            var result = session.Run(modelInputs).ToList()[0];
            float[] modelOutput = result.AsTensor<float>().ToArray();
            float QMax = modelOutput.Max();
            action.move = (AIMove)modelOutput.ToList().IndexOf(QMax);
        } else {
            action.move = ActionSpace.GetMoveFromTensor(actions.ContinuousActions.ToArray());
            /*
            if(i < moveBuffer.Count) {
                action.move = moveBuffer[i];
                i++;
            } else {
                action.move = AIMove.Neutral;
            }
            */

            SetReward(CalculateReward());
            if (episodeEndFlag) {
                EndEpisode();
                episodeEndFlag = false;
            }
        }
    }

    public float CalculateReward() {
        float reward = 0;

        float curHp = (float)rlAI.cScript.currentLifePoints;
        float curOpHp = (float)rlAI.opCScript.currentLifePoints;
        float hpReward = ((curHp - prevHP) - (curOpHp - prevOpHP)) / maxDamage;

        if(-1 <= hpReward && hpReward <= 1) { // prevent from getting irregular reward value when winning or losing (workaround)
            reward += hpReward;
        }
        

        prevHP = curHp;
        prevOpHP = curOpHp;

        return reward;
    }
}