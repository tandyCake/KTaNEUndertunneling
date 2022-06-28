using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class Switch : Section
{
    public KMSelectable switchObj;
    private RotDirection _direction;

    public override SectionType type
    { get { return SectionType.Switch; } }

    private string DirectionText { get { return _direction.ToString().ToLowerInvariant(); } }

    public override IEnumerator ResetAnim(int cycles)
    {
        if (Ut.RandBool())
            yield return Flip();
        maze.switchPosition = _direction;
        Log("Reset starting switch position to {0}.", DirectionText);
    }
    public override bool isValid()
    {
        return _direction == wheelDirection;
    }

    private void SwitchPress()
    {
        if (isAnimating || !parentScript.isInteractable)
            return;
        StartCoroutine(Flip(true));
        Log("Manually flipped the switch to the {0} position.", DirectionText);
        maze.switchPosition = _direction;
        if (interactionHook != null && acceptCommands)
            interactionHook.Invoke(_direction);
    }

    public delegate void SwitchDelegate(RotDirection dir);
    public SwitchDelegate interactionHook;
    private IEnumerator Flip(bool pressed = false)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, switchObj.transform);
        yield return new WaitUntil(() => !isAnimating);
        _direction = 1 - _direction;
        maze.switchPosition = _direction;
        StartCoroutine(FlipAnim());
    }

    private IEnumerator FlipAnim()
    {

        isAnimating = true;
        const float duration = 0.5f;
        float delta = 0;
        while (delta < 1)
        {
            delta += Time.deltaTime / duration;
            switchObj.transform.localEulerAngles = new Vector3(Easing.InOutQuad(delta,
                                                        _direction == RotDirection.Clockwise ? -75 : 75,
                                                        _direction == RotDirection.Clockwise ? 75 : -75,
                                                        1), 90, 0);
            yield return null;
        }
        isAnimating = false;
    }
    protected override void OnStart()
    {
        if (Ut.RandBool())
        {
            _direction = RotDirection.Clockwise;
            switchObj.transform.localEulerAngles = new Vector3(+75, 90, 0);
        }
        else
        {
            _direction = RotDirection.Counterclockwise;
            switchObj.transform.localEulerAngles = new Vector3(-75, 90, 0);
        }
        Log("Starting switch position: {0}.", _direction);
        switchObj.OnInteract += () => { SwitchPress(); return false; };
    }

    protected override void DialInteraction(RotDirection rotation, Direction newPosition)
    {
        if ((newPosition == Direction.Right && _direction == RotDirection.Counterclockwise) || (newPosition == Direction.Left && _direction == RotDirection.Clockwise))
        {
            StartCoroutine(Flip());
            Log("Dial interaction of pointing the dial in the {0} direction flipped the switch to the {0} position.", DirectionText);
        }
        else if ((int)(Bomb.GetTime()) % 2 == 0)
        {
            StartCoroutine(Flip());
            Log("Dial interaction flipped the switch to the {0} position because the last digit of the bomb timer was even.", DirectionText);
        }
    }
    protected override void NumberInteraction(int newNumber)
    {
        int[] primes = { 2, 3, 5, 7 };
        if (primes.Contains(newNumber))
        {
            StartCoroutine(Flip());
            Log("Number interaction of setting the value to a prime ({0}) flipped the switch to the {1} position.", newNumber, DirectionText);
        }
    }
    protected override void GridInteraction(int pressedPosition)
    {
        if (_direction == wheelDirection)
        {
            StartCoroutine(Flip());
            Log("Grid interaction flipped the switch away from the direction the wheel is spinning ({0}).", DirectionText);
        }
    }
    public override string tpRegex { get { return @"^FLIP(?:\s+SWITCH)?$"; } }
    public override IEnumerator ProcessTwitchCommand(string command)
    {
        yield return null;
        switchObj.OnInteract();
        yield return new WaitForSeconds(0.2f);
    }
}