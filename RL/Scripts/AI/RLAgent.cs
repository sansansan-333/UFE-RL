using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.SideChannels;
using UnityEngine;

public class RLAgent : Agent
{
    private StateSpace observation;
    public ActionSpace action;
    public bool isNewActionSet { get; private set; }
    private RLAI rlAI;

    // rewards
    private readonly float initialHP = 1000;
    private readonly float maxDamage = 90;
    private int lastDecisionFrame = -1;
    private bool episodeEndFlag;
    private bool agentWon = false;

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

        // inference
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

    }

    private void FixedUpdate() {
        if (rlAI != null && rlAI.cScript != null && rlAI.opCScript != null && !episodeEndFlag) {
            // episode ends when someone wins
            episodeEndFlag = rlAI.cScript.isDead || rlAI.opCScript.isDead;
            agentWon = rlAI.opCScript.isDead;
        }
    }

    public override void CollectObservations(VectorSensor sensor) {
        if (rlAI == null) return;

        observation = rlAI.storedObservation;

        if(inference) {
            string inputName = "dense_input";
            var inputDimension = session.InputMetadata[inputName].Dimensions;
            inputDimension[0] = 1; // batch size is 1 during inference

            float[] inputArray = observation.ToTensor();
            var inputTensor = new DenseTensor<float>(inputArray, inputDimension);
            modelInputs = new List<NamedOnnxValue> {
                NamedOnnxValue.CreateFromTensor(inputName, inputTensor),
            };
        } else {
            sensor.AddObservation(observation.ToTensor());
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
                RLHelper.Instance.statsChannel.SendStatisticalDataToPython(agentWon);
            }
        }
    }

    public float CalculateReward() {
        float reward = 0;
        var frameBuffer = RLHelper.Instance.history.GetFrameBuffer(rlAI.player);

        // all damages taken and dealt from the last decision frame
        float hitDamageTaken = 0, chipDamageTaken = 0, damageDealt = 0;
        if(frameBuffer.Count > 0) {
            for (int i = 0; i < frameBuffer.Count; i++) {
                if (frameBuffer[i].frame == lastDecisionFrame) break;
                // todo
                if(frameBuffer[i].blocking) {
                    chipDamageTaken += frameBuffer[i].damageTaken / maxDamage;
                } else {
                    hitDamageTaken += frameBuffer[i].damageTaken / maxDamage;
                }
                damageDealt += frameBuffer[i].damageDealt / maxDamage;
            }
            lastDecisionFrame = frameBuffer[0].frame;
        }
        float hpReward = damageDealt - hitDamageTaken + chipDamageTaken / 2;

        reward += hpReward;

        return reward;
    }
}