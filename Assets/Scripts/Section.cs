using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Section : MonoBehaviour {

    public KMAudio Audio;
    public KMBombInfo Bomb;
    public UndertunnelingScript parentScript;

    public Maze maze;
    [HideInInspector]
    public RotDirection wheelDirection;
    [HideInInspector]
    public bool isAnimating;
    [HideInInspector]
    public bool acceptCommands = true;
    public int moduleId { private get; set; }

	public abstract SectionType type { get; }
    public abstract IEnumerator ResetAnim(int cycles);
    public abstract bool isValid();
     
    protected virtual void SwitchInteract(RotDirection newDirection)
    { throw new NotImplementedException(); }
    protected virtual void DialInteraction(RotDirection rotation, Direction newPosition)
    { throw new NotImplementedException(); }
    protected virtual void NumberInteraction(int newNumber)
    { throw new NotImplementedException(); }
    protected virtual void GridInteraction(int pressedPosition)
    { throw new NotImplementedException(); }

    public void Connect(Section other)
    {
        switch (other.type)
        {
            case SectionType.Switch:
                (other as Switch).interactionHook += SwitchInteract; break;
            case SectionType.Dial:
                (other as Dial).interactionHook += DialInteraction; break;
            case SectionType.Number:
                (other as Number).interactionHook += NumberInteraction; break;
            case SectionType.Grid:
                (other as Grid).interactionHook += GridInteraction;  break;
        }
        Log("Connected {0} to {1}.", this.type, other.type);
    }

    void Start()
    {
        OnStart();
    }
    protected virtual void OnStart()
    { }

    protected void Log(string msg, params object[] args)
    {
        Debug.LogFormat("[Undertunneling #{0}] {1}", moduleId, string.Format(msg, args));
    }
}
