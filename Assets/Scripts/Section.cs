using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Section : MonoBehaviour {

    public KMAudio Audio;
    public KMBombInfo Bomb;
    public RotDirection wheelDirection;
    public Maze maze;
    public bool isAnimating;

	public abstract SectionType type { get; }
    public abstract IEnumerator ResetAnim();

    public Action InteractionHook;

   


    protected virtual void SwitchInteract(RotDirection newDirection)
    { throw new NotImplementedException(); }
    protected virtual void DialInteract(RotDirection rotation, DialDirection newPosition)
    { throw new NotImplementedException(); }
    protected virtual void NumberInteract(int newNumber)
    { throw new NotImplementedException(); }
    protected virtual void GridInteract(int pressedPosition)
    { throw new NotImplementedException(); }


    void Start()
    {
        OnStart();
    }
    protected virtual void OnStart()
    { }
}
