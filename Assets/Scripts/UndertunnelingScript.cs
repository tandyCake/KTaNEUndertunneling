using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class UndertunnelingScript : MonoBehaviour
{
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
    private bool canEnterStage2 = true;
    public Switch switchComponent;
    public Dial dialComponent;
    public Number numberComponent;
    public Grid gridComponent;

    public Section[] sections;
    public Transform[] sectionPositions;

    private Maze maze;
    public bool isInteractable = true;
    float wheelSpeed = 1;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        maze = new Maze(moduleId);
        centerBtn.OnInteract += () => { Hold(); return false; };
        centerBtn.OnInteractEnded += () => Release();
        StartCoroutine(SetUpSections());
    }

    void Hold()
    {
        if (isHeld)
            return;
        centerBtn.AddInteractionPunch();
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, centerBtn.transform);
        if (!isInteractable)
            return;
        heldTime = Time.time;
        isHeld = true;
    }
    void Release()
    {
        centerBtn.AddInteractionPunch(0.25f);
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, centerBtn.transform);
        if (!isInteractable)
            return;
        if (Time.time - heldTime > 1)
            StartCoroutine(Reset());
        isHeld = false;
    }
    IEnumerator Reset()
    {
        if (moduleSolved)
            yield break;
        isInteractable = false;
        canEnterStage2 = false;
        stage2 = false;
        gridComponent.flicker = false;
        maze.Reset();
        StartCoroutine(SpeedUpAgain());
        yield return new WaitUntil(() => sections.All(x => !x.isAnimating));
        for (int i = 0; i < 4; i++)
        {
            StartCoroutine(sections[i].ResetAnim(Rnd.Range(12, 20)));
            sections[i].acceptCommands = true;
        }
        yield return new WaitUntil(() => sections.All(x => !x.isAnimating));
        canEnterStage2 = true;
        isInteractable = true;
        FixGridIfUnsolvable();
    }
    IEnumerator SpeedUpAgain()
    {
        while (wheelSpeed < 1)
        {
            yield return null;
            wheelSpeed += 0.33f * Time.deltaTime;
        }
        wheelSpeed = 1;
    }
    IEnumerator SetUpSections()
    {
        sections.Shuffle();
        Log("Section layout in clockwise order: " + sections.Select(x => x.type.ToString()).Join(" | "));
        wheelDirection = (RotDirection) Rnd.Range(0, 2);
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

        // Wait for one frame to give all sections a chance to randomize themselves
        yield return null;
        FixGridIfUnsolvable();
    }

    void FixGridIfUnsolvable()
    {
        // If the grid and dial are paired, make sure that the module is still solvable
        var gridIx = Array.IndexOf(sections, gridComponent);
        if (sections[(gridIx + 2) % 4].type == SectionType.Dial)
            gridComponent.FixIfGridSolvePathInvalid((int) dialComponent.pointing % 2 == 0);
    }

    void Update()
    {
        if (moduleSolved)
            return;
        float offset = Time.deltaTime * 35 * wheelSpeed;
        if (wheelDirection == RotDirection.Counterclockwise)
            offset *= -1;
        wheel.transform.localEulerAngles += offset * Vector3.up;

        if (!stage2 && canEnterStage2 && sections.All(x => x.isValid()))
            StartCoroutine(EnterMovementStage());
    }

    IEnumerator EnterMovementStage()
    {
        stage2 = true;
        isInteractable = false;
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
        if (gridComponent.movementHook == null)
            gridComponent.movementHook += (d) => TryMove(d);
        isInteractable = true;

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
            gridComponent.flicker = false;
            gridComponent.SetBlank();
        }
    }

    private void Log(string msg, params object[] args)
    {
        Debug.LogFormat("[Undertunneling #{0}] {1}", moduleId, string.Format(msg, args));
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = "Use <!{0} flip> to flip the switch. Use <!{0} turn cw/ccw #> to turn the dial clockwise/counterclockwise # times (the # is optional). Use <!{0} set up/down #> to set the number display to # by pressing the up/down button. Use <!{0} press 1 5 9> to press those lights on the light grid.\nAppend 'at #' to do that action when the last timer digit is a certain value. Two digits may also be supplied for the last 2 timer digits.\nUse <!{0} move URDL> to move in those directions in the maze in phase 2. Use <!{0} reset> to reset the module.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        if (!isInteractable)
        {
            yield return "sendtochaterror The module is not interactable at this time.";
            yield break;
        }
        command = command.Trim().ToUpperInvariant();
        if (command == "RESET")
        {
            yield return null;
            centerBtn.OnInteract();
            yield return new WaitForSeconds(1.25f);
            centerBtn.OnInteractEnded();
            yield break;
        }
        if (stage2)
        {
            if (Regex.IsMatch(command, gridComponent.stage2TpRegex))
            {
                yield return null;
                yield return gridComponent.ProcessStage2TwitchCommand(command);
            }
            yield break;
        }

        Match cutOffTimerPart = Regex.Match(command, @"^(.+)(?:ON|AT)\s+([0-5]?[0-9])$");
        command = cutOffTimerPart.Success ? cutOffTimerPart.Groups[1].Value.Trim() : command.Trim();
        if (!sections.Any(sec => Regex.IsMatch(command, sec.tpRegex)))
            yield break;
        yield return null;
        if (cutOffTimerPart.Success)
        {
            yield return null;
            string timerPart = cutOffTimerPart.Groups[2].Value;
            int target = int.Parse(timerPart);
            int modulus = timerPart.Length == 1 ? 10 : 60;
            while (((int) Bomb.GetTime()) % modulus != target)
                yield return "trycancel";
        }
        foreach (Section section in sections)
            if (Regex.IsMatch(command, section.tpRegex))
                yield return section.ProcessTwitchCommand(command);
    }

}