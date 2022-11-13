using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

using UFE3D;

public class ActionSpace: ISpace
{
    public bool noInput;
    public Dictionary<ButtonPress, int> inputs; // 1 for press, 0 for not press
    public static readonly List<ButtonPress> usedButtons = new List<ButtonPress> {
        ButtonPress.Forward,
        ButtonPress.Back,
        ButtonPress.Up,
        ButtonPress.Down,
        ButtonPress.Button1,
        ButtonPress.Button2,
        ButtonPress.Button3,
        ButtonPress.Button4,
        ButtonPress.Button5,
        ButtonPress.Button6
    };

    public ActionSpace() {
        inputs = new Dictionary<ButtonPress, int>();
        foreach (ButtonPress button in usedButtons) {
            inputs[button] = 0;
        }
    }

    public float[] GetTensor() {
        var inputs_array = Array.ConvertAll<int, float>(inputs.Values.ToArray(), i => noInput ? 0 : i); // cast int to float and make it 0 if noInput is true
        var tensor = new List<float>();
        tensor.Add(noInput ? 1 : 0);
        tensor.AddRange(inputs_array);

        return tensor.ToArray();
    }
}
