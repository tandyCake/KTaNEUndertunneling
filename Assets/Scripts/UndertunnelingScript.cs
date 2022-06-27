using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class UndertunnelingScript : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBombModule Module;

    public Transform wheel;
    private RotDirection wheelDirection;

    public KMSelectable centerBtn;
    private float heldTime;
    private bool isHeld;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;
    private bool stage2;
    public Switch switchComponent;
    public Dial dialComponent;
    public Number numberComponent;
    public Grid gridComponent;

    public Section[] sections;
    public Transform[] sectionPositions;

    private Maze maze;
    public bool isInteractable = true;
    float wheelSpeed = 1;

    void Awake () {
        moduleId = moduleIdCounter++;
        maze = new Maze(moduleId);
        SetUpSections();
        centerBtn.OnInteract += () => { Hold();  return false; };
        centerBtn.OnInteractEnded += () => Release();
    }

    void Hold()
    {
        if (isHeld || !isInteractable)
            return;
        centerBtn.AddInteractionPunch();
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, centerBtn.transform);
        heldTime = Time.time;
        isHeld = true;
    }
    void Release()
    {
        if (!isInteractable)
            return;
        centerBtn.AddInteractionPunch(0.25f);
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, centerBtn.transform);
        if (Time.time - heldTime > 1)
            StartCoroutine(Reset());
        isHeld = false;
    }
    IEnumerator Reset()
    {
        if (moduleSolved)
            yield break;
        isInteractable = false;
        maze.Reset();
        yield return new WaitUntil(() => sections.All(x => !x.isAnimating));
        for (int i = 0; i < 4; i++)
        {
            StartCoroutine(sections[i].ResetAnim(Rnd.Range(12, 20)));
            sections[i].acceptCommands = true;
        }
        yield return new WaitUntil(() => sections.All(x => !x.isAnimating));
        isInteractable = true;
    }
    void SetUpSections()
    {
        sections.Shuffle();
        Log("Section layout in clockwise order: " + sections.Select(x => x.type.ToString()).Join(" | "));
        wheelDirection = (RotDirection)Rnd.Range(0, 2);
        Log("Chosen wheel direction: {0}.", wheelDirection);
        for (int i = 0; i < 4; i++)
        {
            sections[i].transform.parent = sectionPositions[i];
            sections[i].transform.localPosition = Vector3.zero;
            sections[i].transform.localEulerAngles = Vector3.zero;
            sections[i].moduleId = moduleId;
            sections[i].wheelDirection = wheelDirection;
            sections[i].maze = maze;
            sections[i].Connect(sections[(i + 2) % 4]);
        }
        
    }

    void Update ()
    {
        if (moduleSolved)
            return;
        float offset = Time.deltaTime * 35 * wheelSpeed;
        if (wheelDirection == RotDirection.Counterclockwise)
            offset *= -1;
        wheel.transform.localEulerAngles += offset * Vector3.up;

        if (!stage2 && sections.All(x => x.isValid()))
            StartCoroutine(EnterMovementStage());
    }

    IEnumerator EnterMovementStage()
    {
        stage2 = true;
        for (int i = 0; i < 4; i++)
            sections[i].acceptCommands = false;
        numberComponent.SetBlank();
        gridComponent.SetBlank();
        Audio.PlaySoundAtTransform("progress", transform);
        while (wheelSpeed > 0)
        {
            yield return null;
            wheelSpeed -= 0.33f * Time.deltaTime;
        }
        wheelSpeed = 0;
        yield return new WaitForSeconds(1.5f);
        gridComponent.ActivateStage2Mode();
        gridComponent.movementHook += (d) => TryMove(d);
    }

    private void TryMove(Direction d)
    {
        if (moduleSolved)
            return;
        if (!maze.TryMove(d))
            Module.HandleStrike();
        else if (maze.IsSolved)
        {
            moduleSolved = true;
            Log("The starting cell has been returned to. Module solved!");
            Module.HandlePass();
            Audio.PlaySoundAtTransform("COMPLETE!", transform);
            gridComponent.isSolved = true;
            gridComponent.SetBlank();
        }
    }

    private void Log(string msg, params object[] args)
    {
        Debug.LogFormat("[Undertunneling #{0}] {1}", moduleId, string.Format(msg, args));
    }

    /*
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use <!{0} foobar> to do something.";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand (string command)
    {
        command = command.Trim().ToUpperInvariant();
        List<string> parameters = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        yield return null;
    }

    IEnumerator TwitchHandleForcedSolve ()
    {
        yield return null;
    }
    */
}