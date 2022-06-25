using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class Number : Section
{
    private static readonly bool[][] numbers = new bool[10][]
    {
        new bool[7] { true, true, true, false, true, true, true },
        new bool[7] { false, false, true, false, false, true, false },
        new bool[7] { true, false, true, true, true, false, true },
        new bool[7] { true, false, true, true, false, true, true},
        new bool[7] { false, true, true, true, false, true, false },
        new bool[7] { true, true, false, true, false, true, true },
        new bool[7] { true, true, false, true, true, true, true },
        new bool[7] { true, false, true, false, false, true, false },
        new bool[7] { true, true, true, true, true, true, true },
        new bool[7] { true, true, true, true, false, true, true },
    };
    private static readonly int[][] wheelDiagrams = new int[5][]
    {
        new int[5] { 9, 3, 0, 5, 7 },
        new int[5] { 0, 2, 4, 8, 1 },
        new int[5] { 1, 5, 0, 4, 7 },
        new int[5] { 5, 3, 6, 2, 1 },
        new int[5] { 8, 6, 2, 5, 9 },
    };
    public override SectionType type
    { get { return SectionType.Number; } }
    private int _value;
    private int _target;

    public MeshRenderer[] segments;
    public Material unlit, lit;
    public KMSelectable up, down;

    public delegate void NumberDelegate(int n);
    public NumberDelegate interactionHook;

    protected override void OnStart()
    {
        if (wheelDirection == RotDirection.Clockwise)
            _target = Bomb.GetSerialNumberNumbers().First();
        else _target = Bomb.GetSerialNumberNumbers().Last();
        _value = Rnd.Range(0, 10);
        SetDisplay();

        up.OnInteract += () => { UpPress(); return false; };
        down.OnInteract += () => { DownPress(); return false; };
    }

    private void UpPress()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, up.transform);
        up.AddInteractionPunch(0.15f);
        if (isAnimating)
            return;
        IncrementValue(+1);
        if (interactionHook != null)
            interactionHook.Invoke(_value);
        maze.AddDirections(new[] { Direction.Up });
    }
    private void DownPress()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, down.transform);
        down.AddInteractionPunch(0.15f);
        if (isAnimating)
            return;
        IncrementValue(+9);
        if (interactionHook != null)
            interactionHook.Invoke(_value);
        maze.AddDirections(new[] { Direction.Down });
    }
    private void IncrementValue(int offset)
    {
        _value += offset;
        _value %= 10;
        SetDisplay();
    }

    public override bool isValid()
    {
        return _value == _target;
    }

    public override IEnumerator ResetAnim(int cycles)
    {
        isAnimating = true;
        for (int i = 0; i < cycles; i++)
        {
            for (int seg = 0; seg < 8; seg++)
                segments[seg].material = Ut.RandBool() ? unlit : lit;
            yield return new WaitForSeconds(0.2f);
        }
        _value = Rnd.Range(0, 10);
        SetDisplay();
        isAnimating = false;
    }
    private void SetDisplay()
    {
        for (int i = 0; i < 7; i++)
            segments[i].material = numbers[_value][i] ? lit : unlit;
        segments[7].material = lit;
    }

    protected override void SwitchInteract(RotDirection newDirection)
    {
        if (newDirection == wheelDirection)
            IncrementValue(+5);
        else
        {
            _value = 9 - _value;
            SetDisplay();
        }
    }
    protected override void DialInteraction(RotDirection rotation, Direction newPosition)
    {
        int[] wheel = wheelDiagrams[Bomb.GetSerialNumberNumbers().Last() % 5];
        int curCycle = Array.IndexOf(wheel, _value);
        if (curCycle != -1)
        {
            curCycle = curCycle + (rotation == RotDirection.Clockwise ? +1 : +4) % 5;
            _value = wheel[curCycle];
        }
    }
    protected override void GridInteraction(int pressedPosition)
    {
        IncrementValue(pressedPosition + 1);
    }
}
