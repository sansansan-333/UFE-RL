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
    private float prevHP = StateSpace.PlayerState.maxHP;
    private float prevOpHP = StateSpace.PlayerState.maxHP;
    private bool wasSomeoneDead = false; // keep track that either the player or the opponent was dead in the last frame
    private int win = 0;

    // inference
    private bool inference;
    private InferenceSession session;
    [PathAttribute] public string modelPath;
    private List<NamedOnnxValue> modelInputs;
    private static readonly float deltaOutputRange = 0.001f;

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
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,
            AIMove.Forward,

            AIMove.Jump_Forward,
            AIMove.Kick_Jumping_Heavy
        };
    }

    public void EnvironmentReset() {

    }

    public override void OnEpisodeBegin() {

    }

    private void FixedUpdate() {
        /*
        if (rlAI != null && rlAI.IsWaitingForDecision()) {
            var validatedMoves = RLUtility.ValidateAIMoves(2);
            logger.Log("RLAgent: ");
            logger.LogArray(validatedMoves);
            var storedMove = rlAI.cScript.storedMove;
            if (storedMove != null) Debug.Log("stored: " + storedMove.moveName);
            RequestDecision();
        } else {
            isNewActionSet = false;
        }
        */
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

            // check if someone won
            /* alive >> alive 
             * alive >> dead <- win
             * dead >> alive
             * dead >> dead */
            if (wasSomeoneDead) {
                win = 0;
            } else {
                if(rlAI.cScript.isDead) {
                    win = -1;
                } else if(rlAI.opCScript.isDead) {
                    win = 1;
                } else {
                    win = 0;
                }
            }

            SetReward(CalculateReward());
            if (win != 0) {
                EndEpisode();
            }

            wasSomeoneDead = rlAI.cScript.isDead || rlAI.opCScript.isDead;
        }
    }

    public float CalculateReward() {
        float reward = 0;

        float curHp = (float)rlAI.cScript.currentLifePoints;
        float curOpHp = (float)rlAI.opCScript.currentLifePoints;
        float reward_hp = ((curHp - prevHP) - (curOpHp - prevOpHP)) / StateSpace.PlayerState.maxHP * 5f;
        if(reward_hp < 1) {
            reward += reward_hp;
        }
        reward += win;

        prevHP = curHp;
        prevOpHP = curOpHp;

        return reward;
    }
}