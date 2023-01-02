using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

using UFE3D;

public class ActionSpace: ISpace
{
    public AIMove move = AIMove.Neutral;

    public enum AIMove {
        Neutral,
        Forward,
        Back,
        Jump_Vertical,
        Jump_Forward,
        Jump_Back,
        Crouch,
        Crouch_Back,

        Punch_Standing_Light,
        Punch_Standing_Heavy,
        Punch_Crouching_Light,
        Punch_Crouching_Heavy,
        Punch_Jumping_Light,
        Punch_Jumping_Heavy,
        Kick_Standing_Light,
        Kick_Standing_Heavy,
        Kick_Crouching_Light,
        Kick_Crouching_Heavy,
        Kick_Jumping_Light,
        Kick_Jumping_Heavy,

        Dash,
        Fireball_Light,
        Fireball_Heavy,
        Focus,
        Wall_Launcher
    }

    public static readonly Dictionary<AIMove, ButtonPress[][]> simulatedInput = new Dictionary<AIMove, ButtonPress[][]> {
        #region Basic moves
        {   AIMove.Neutral, 
            new ButtonPress[1][] {
                new ButtonPress[0]
            }
        },
        {   AIMove.Forward,
            new ButtonPress[3][] {
                new ButtonPress[1] {
                    ButtonPress.Forward
                },
                new ButtonPress[1] {
                    ButtonPress.Forward
                },
                new ButtonPress[1] {
                    ButtonPress.Forward
                }
            }
        },
        {   AIMove.Back,
            new ButtonPress[3][] {
                new ButtonPress[1] {
                    ButtonPress.Back
                },
                new ButtonPress[1] {
                    ButtonPress.Back
                },
                new ButtonPress[1] {
                    ButtonPress.Back
                }
            }
        },
        {   AIMove.Jump_Vertical,
            new ButtonPress[1][] {
                new ButtonPress[1] {
                    ButtonPress.Up
                }
            }
        },
        {   AIMove.Crouch,
            new ButtonPress[3][] {
                new ButtonPress[1] {
                    ButtonPress.Down
                },
                new ButtonPress[1] {
                    ButtonPress.Down
                },
                new ButtonPress[1] {
                    ButtonPress.Down
                }
            }
        },
        {   AIMove.Jump_Forward,
            new ButtonPress[1][] {
                new ButtonPress[2] {
                    ButtonPress.Forward,
                    ButtonPress.Up
                }
            }
        },
        {   AIMove.Jump_Back,
            new ButtonPress[1][] {
                new ButtonPress[2] {
                    ButtonPress.Back,
                    ButtonPress.Up
                }
            }
        },
        {   AIMove.Crouch_Back,
            new ButtonPress[3][] {
                new ButtonPress[2] {
                    ButtonPress.Back,
                    ButtonPress.Down
                },
                new ButtonPress[2] {
                    ButtonPress.Back,
                    ButtonPress.Down
                },
                new ButtonPress[2] {
                    ButtonPress.Back,
                    ButtonPress.Down
                }
            }
        },
        #endregion Basic moves
        #region Attack moves
        {   AIMove.Punch_Standing_Light,
            new ButtonPress[1][] {
                new ButtonPress[1] {
                    ButtonPress.Button1
                }
            }
        },
        {   AIMove.Punch_Standing_Heavy,
            new ButtonPress[1][] {
                new ButtonPress[1] {
                    ButtonPress.Button2
                }
            }
        },
        {   AIMove.Punch_Crouching_Light,
            new ButtonPress[1][] {
                new ButtonPress[3] {
                    ButtonPress.Back,
                    ButtonPress.Down,
                    ButtonPress.Button1
                }
            }
        },
        {   AIMove.Punch_Crouching_Heavy,
            new ButtonPress[1][] {
                new ButtonPress[3] {
                    ButtonPress.Back,
                    ButtonPress.Down,
                    ButtonPress.Button2
                }
            }
        },
        {   AIMove.Punch_Jumping_Light,
            new ButtonPress[1][] {
                new ButtonPress[1] {
                    ButtonPress.Button1
                }
            }
        },
        {   AIMove.Punch_Jumping_Heavy,
            new ButtonPress[1][] {
                new ButtonPress[1] {
                    ButtonPress.Button2
                }
            }
        },
        {   AIMove.Kick_Standing_Light,
            new ButtonPress[1][] {
                new ButtonPress[1] {
                    ButtonPress.Button4
                }
            }
        },
        {   AIMove.Kick_Standing_Heavy,
            new ButtonPress[1][] {
                new ButtonPress[1] {
                    ButtonPress.Button5
                }
            }
        },
        {   AIMove.Kick_Crouching_Light,
            new ButtonPress[1][] {
                new ButtonPress[3] {
                    ButtonPress.Back,
                    ButtonPress.Down,
                    ButtonPress.Button4
                }
            }
        },
        {   AIMove.Kick_Crouching_Heavy,
            new ButtonPress[1][] {
                new ButtonPress[3] {
                    ButtonPress.Back,
                    ButtonPress.Down,
                    ButtonPress.Button5
                }
            }
        },
        {   AIMove.Kick_Jumping_Light,
            new ButtonPress[1][] {
                new ButtonPress[1] {
                    ButtonPress.Button4
                }
            }
        },
        {   AIMove.Kick_Jumping_Heavy,
            new ButtonPress[1][] {
                new ButtonPress[1] {
                    ButtonPress.Button5
                }
            }
        },
        #endregion Attack moves
        #region Special moves
        {   AIMove.Dash,
            new ButtonPress[3][] {
                new ButtonPress[1] {
                    ButtonPress.Forward
                },
                new ButtonPress[0],
                new ButtonPress[1] {
                    ButtonPress.Forward
                }
            }
        },
        {   AIMove.Fireball_Light,
            new ButtonPress[3][] {
                new ButtonPress[1] {
                    ButtonPress.Down
                },
                new ButtonPress[2] {
                    ButtonPress.Down,
                    ButtonPress.Forward
                },
                new ButtonPress[1] {
                    ButtonPress.Button1
                }
            }
        },
        {   AIMove.Fireball_Heavy,
            new ButtonPress[3][] {
                new ButtonPress[1] {
                    ButtonPress.Down
                },
                new ButtonPress[2] {
                    ButtonPress.Down,
                    ButtonPress.Forward
                },
                new ButtonPress[1] {
                    ButtonPress.Button2
                }
            }

        },
        {   AIMove.Focus,
            new ButtonPress[1][] {
                new ButtonPress[1] {
                    ButtonPress.Button6
                }
            }
        },
        {   AIMove.Wall_Launcher,
            new ButtonPress[1][] {
                new ButtonPress[2] {
                    ButtonPress.Forward,
                    ButtonPress.Button2
                }
            }
        },
        #endregion Special moves
    };

    public ActionSpace() {
        
    }

    public float[] GetTensor() {
        return GetTensorFromMove(move);
    }

    public static float[] GetTensorFromMove(AIMove aiMove) {
        var tensor = new List<float>();
        foreach (AIMove move in Enum.GetValues(typeof(AIMove))) {
            if (move == aiMove) tensor.Add(1);
            else tensor.Add(0);
        }

        return tensor.ToArray();
    }

    public static AIMove GetMoveFromTensor(float[] tensor) {
        for(int i = 0; i < tensor.Length; i++) {
            if (tensor[i] == 1) return (AIMove)i;
        }

        return AIMove.Neutral;
    }

    public int GetLength() {
        return Enum.GetValues(typeof(AIMove)).Length;
    }
}
