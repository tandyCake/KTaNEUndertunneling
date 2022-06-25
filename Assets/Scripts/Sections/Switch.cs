using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Switch : Section
{
    public KMSelectable switchObj;
    private RotDirection _direction;

    public override SectionType type
    { get { return SectionType.Switch; } }

    public override IEnumerator ResetAnim()
    {
        _direction = (RotDirection)UnityEngine.Random.Range(0, 2);
        yield return FlipAnim();
    }

    private void SwitchPress()
    {
        if (isAnimating)
            return;
        StartCoroutine(Flip());
    }

    private IEnumerator Flip()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, switchObj.transform);
        yield return new WaitUntil(() => !isAnimating);
        _direction = 1 - _direction;
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
        switchObj.OnInteract += () => { SwitchPress(); return false; };
    }

    protected override void DialInteract(RotDirection rotation, DialDirection newPosition)
    {
        if ((newPosition == DialDirection.Right && _direction == RotDirection.Counterclockwise) || (newPosition == DialDirection.Left && _direction == RotDirection.Clockwise))
            Flip();
        else if ((int)(Bomb.GetTime()) % 2 == 0)
            Flip();
    }
    protected override void NumberInteract(int newNumber)
    {
        int[] primes = { 2, 3, 5, 7 };
        if (primes.Contains(newNumber))
            Flip();
    }
    protected override void GridInteract(int pressedPosition)
    {
        if (_direction == wheelDirection)
            Flip();
    }
}
