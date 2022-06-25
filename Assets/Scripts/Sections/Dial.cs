using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dial : Section
{
    public KMSelectable cw, ccw;
    public override SectionType type { get { return SectionType.Dial; } }

    protected override void OnStart()
    {
        cw.OnInteract += () => { ClockPress(); return false; };
        ccw.OnInteract += () => { CounterPress(); return false; };
    }
    private void CounterPress()
    {

    }
    private void ClockPress()
    {

    }

    public override IEnumerator ResetAnim()
    {
        throw new System.NotImplementedException();
    }
}
