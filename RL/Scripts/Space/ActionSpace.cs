using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

using UFE3D;

public class ActionSpace: ISpace
{
    public AvailableMove move = AvailableMove.Neutral;

    public enum AvailableMove {
        Neutral,
        Forward,
        Back,
        Up,
        Down,
        Forward_Up,
        Forward_Down,
        Back_Up,
        Back_Down,

        Punch_Standing_Light,
        Punch_Standing_Heavy,
        Punch_Crouching_Light,
        Punch_Crouching_Heavy,
        Kick_Standing_Light,
        Kick_Standing_Heavy,
        Kick_Crouching_Light,
        Kick_Crouching_Heavy,

        Dash,
        Fireball_Light,
        Fireball_Heavy,
        Focus,
        Wall_Launcher
    }

    public static readonly Dictionary<AvailableMove, ButtonPress[][]> simulatedInput = new Dictionary<AvailableMove, ButtonPress[][]> {
        #region Basic moves
        {   AvailableMove.Neutral, 
            new ButtonPress[1][] {
                new ButtonPress[0]
            }
        },
        {   AvailableMove.Forward,
            new ButtonPress[1][] {
                new ButtonPress[1] {
                    ButtonPress.Forward
                }
            }
        },
        {   AvailableMove.Back,
            new ButtonPress[1][] {
                new ButtonPress[1] {
                    ButtonPress.Back
                }
            }
        },
        {   AvailableMove.Up,
            new ButtonPress[1][] {
                new ButtonPress[1] {
                    ButtonPress.Up
                }
            }
        },
        {   AvailableMove.Down,
            new ButtonPress[1][] {
                new ButtonPress[1] {
                ButtonPress.Back
            }
            }
        },
        {   AvailableMove.Forward_Up,
            new ButtonPress[1][] {
                new ButtonPress[2] {
                    ButtonPress.Forward,
                    ButtonPress.Up
                }
            }
        },
        {   AvailableMove.Forward_Down,
            new ButtonPress[1][] {
                new ButtonPress[2] {
                    ButtonPress.Forward,
                    ButtonPress.Down
                }
            }
        },
        {   AvailableMove.Back_Up,
            new ButtonPress[1][] {
                new ButtonPress[2] {
                    ButtonPress.Back,
                    ButtonPress.Up
                }
            }
        },
        {   AvailableMove.Back_Down,
            new ButtonPress[1][] {
                new ButtonPress[2] {
                    ButtonPress.Back,
                    ButtonPress.Down
                }
            }
        },
        #endregion Basic moves
        #region Attack moves
        {   AvailableMove.Punch_Standing_Light,
            new ButtonPress[1][] {
                new ButtonPress[1] {
                    ButtonPress.Button1
                }
            }
        },
        {   AvailableMove.Punch_Standing_Heavy,
            new ButtonPress[1][] {
                new ButtonPress[1] {
                    ButtonPress.Button2
                }
            }
        },
        {   AvailableMove.Punch_Crouching_Light,
            new ButtonPress[1][] {
                new ButtonPress[3] {
                    ButtonPress.Back,
                    ButtonPress.Down,
                    ButtonPress.Button1
                }
            }
        },
        {   AvailableMove.Punch_Crouching_Heavy,
            new ButtonPress[1][] {
                new ButtonPress[3] {
                    ButtonPress.Back,
                    ButtonPress.Down,
                    ButtonPress.Button2
                }
            }
        },
        {   AvailableMove.Kick_Standing_Light,
            new ButtonPress[1][] {
                new ButtonPress[1] {
                    ButtonPress.Button4
                }
            }
        },
        {   AvailableMove.Kick_Standing_Heavy,
            new ButtonPress[1][] {
                new ButtonPress[1] {
                    ButtonPress.Button5
                }
            }
        },
        {   AvailableMove.Kick_Crouching_Light,
            new ButtonPress[1][] {
                new ButtonPress[3] {
                    ButtonPress.Back,
                    ButtonPress.Down,
                    ButtonPress.Button4
                }
            }
        },
        {   AvailableMove.Kick_Crouching_Heavy,
            new ButtonPress[1][] {
                new ButtonPress[3] {
                    ButtonPress.Back,
                    ButtonPress.Down,
                    ButtonPress.Button5
                }
            }
        },
        #endregion Attack moves
        #region Special moves
        {   AvailableMove.Dash,
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
        {   AvailableMove.Fireball_Light,
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
        {   AvailableMove.Fireball_Heavy,
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
        {   AvailableMove.Focus,
            new ButtonPress[1][] {
                new ButtonPress[1] {
                    ButtonPress.Button6
                }
            }
        },
        {   AvailableMove.Wall_Launcher,
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
        var tensor = new List<float>();
        foreach(AvailableMove move in Enum.GetValues(typeof(AvailableMove))) {
            if (move == this.move) tensor.Add(1);
            else tensor.Add(0);
        }
        
        return tensor.ToArray();
    }

    public static AvailableMove GetMoveFromTensor(float[] tensor) {
        for(int i = 0; i < tensor.Length; i++) {
            if (tensor[i] == 1) return (AvailableMove)i;
        }

        return AvailableMove.Neutral;
    }
}
