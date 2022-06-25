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

    public override IEnumerator ResetAnim(int cycles)
    {
        if (Ut.RandBool())
            _direction = 1 - _direction;
        yield return FlipAnim();
    }
    public override bool isValid()
    {
        return _direction == wheelDirection;
    }

    private void SwitchPress()
    {
        if (isAnimating)
            return;
        StartCoroutine(Flip(true));
        if (interactionHook != null)
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
        switchObj.OnInteract += () => { SwitchPress(); return false; };
    }

    protected override void DialInteraction(RotDirection rotation, Direction newPosition)
    {
        if ((newPosition == Direction.Right && _direction == RotDirection.Counterclockwise) || (newPosition == Direction.Left && _direction == RotDirection.Clockwise))
            Flip();
        else if ((int)(Bomb.GetTime()) % 2 == 0)
            Flip();
    }
    protected override void NumberInteraction(int newNumber)
    {
        int[] primes = { 2, 3, 5, 7 };
        if (primes.Contains(newNumber))
            Flip();
    }
    protected override void GridInteraction(int pressedPosition)
    {
        if (_direction == wheelDirection)
            Flip();
    }

}