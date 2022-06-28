using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using Rnd = UnityEngine.Random;

public class Dial : Section
{
    public KMSelectable cw, ccw;
    public Transform dial;
    public override SectionType type { get { return SectionType.Dial; } }

    private Direction _pointing;

    protected override void OnStart()
    {
        cw.OnInteract += () => { BtnPress(RotDirection.Clockwise); return false; };
        ccw.OnInteract += () => { BtnPress(RotDirection.Counterclockwise); return false; };
        _pointing = (Direction)Rnd.Range(0, 4);
        dial.transform.localEulerAngles = new Vector3(-90, 90 * (int)_pointing, 0);
    }

    public delegate void DialDelegate(RotDirection rotation, Direction newDir);
    public DialDelegate interactionHook;
    private void BtnPress(RotDirection dir)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, dial);
        if (!parentScript.isInteractable)
            return;
        Audio.PlaySoundAtTransform("Dial", dial);
        Rotate(dir);
        Log("Manually rotated the dial {0} to the {1} position.", dir.ToString().ToLower(), _pointing.ToString().ToLower());
        if (acceptCommands)
        {
            maze.AddDirections(new[] { _pointing });
            if (interactionHook != null)
                interactionHook.Invoke(dir, _pointing);
        }
    }

    void Rotate(RotDirection d)
    {
        if (d == RotDirection.Clockwise)
            _pointing = (Direction)(((int)_pointing + 1) % 4);
        else _pointing = (Direction)(((int)_pointing + 3) % 4);
        StartCoroutine(RotateAnim(d));
    }
    public IEnumerator RotateAnim(RotDirection direction)
    {
        yield return new WaitUntil(() => !isAnimating);
        isAnimating = true;
        float initAngle = dial.localEulerAngles.y;
        float endAngle;
        if (direction == RotDirection.Counterclockwise)
            endAngle = initAngle - 90;
        else
            endAngle = initAngle + 90;
        float delta = 0;
        while (delta < 1)
        {
            delta += Time.deltaTime / 0.4f;
            dial.localEulerAngles = new Vector3(-90, Mathf.Lerp(initAngle, endAngle, delta), 0);
            yield return null;
        }
        isAnimating = false;
    }
    private IEnumerator RotateMultiple(int times, RotDirection rot)
    {
        for (int i = 0; i < times; i++)
        {
            Rotate(rot);
            yield return null;
        }
    }
    public override IEnumerator ResetAnim(int cycles)
    {
        RotDirection rot = (RotDirection)Rnd.Range(0, 2);
        yield return RotateMultiple(cycles, rot);
    }
    public override bool isValid()
    {
        if (wheelDirection == RotDirection.Clockwise)
            return _pointing == Direction.Down;
        else return _pointing == Direction.Up;
    }
    protected override void SwitchInteract(RotDirection newDirection)
    {
        Rotate(newDirection);
    }
    protected override void NumberInteraction(int newNumber)
    {
        StartCoroutine(RotateMultiple((newNumber - 1) % 4 + 1, RotDirection.Clockwise));
    }
    protected override void GridInteraction(int pressedPosition)
    {
        if (pressedPosition % 2 == 1)
            Rotate(RotDirection.Clockwise);
        else if (pressedPosition != 4)
            Rotate(RotDirection.Counterclockwise);
    }
    public override string tpRegex { get { return @"^(?:(?:TURN|ROTATE)\s+)?(C?CW)(?:\s+([0-9]{1,2}))?$"; } }
    public override IEnumerator ProcessTwitchCommand(string command)
    {
        Match m = Regex.Match(command, tpRegex);
        yield return null;
        KMSelectable btn = m.Groups[1].Value.Length == 2 ? cw : ccw;
        int presses = m.Groups[2].Length == 0 ? 1 : int.Parse(m.Groups[2].Value);
        for (int i = 0; i < presses; i++)
        {
            btn.OnInteract();
            yield return new WaitForSeconds(0.2f);
        }
    }
}