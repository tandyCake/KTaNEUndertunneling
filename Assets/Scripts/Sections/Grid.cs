using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;
using Rnd = UnityEngine.Random;

public class Grid : Section
{

    public static int[][] adjacents = new int[9][]
    {
        new[] { 1, 3}, new[] { 0, 2, 4 }, new[] { 1, 5 },
        new[] { 0, 4, 6 }, new[]{ 1, 3, 5, 7 }, new[] { 2, 4, 8 },
        new[] { 3,7 }, new[]{ 4, 6, 8 }, new[] { 5, 7 }
    };
    public static Direction[][] directionsFromCenter = new Direction[9][]
    {
        new[] { Direction.Up, Direction.Left }, new[] {Direction.Up }, new[] { Direction.Up, Direction.Right },
        new[] { Direction.Left }, new Direction[0], new[] { Direction.Right },
        new[] { Direction.Down, Direction.Left }, new[] { Direction.Down }, new[] { Direction.Down, Direction.Right }
    };
    private static readonly string[] positionNames = { "top-left", "top-middle", "top-right", "middle-left", "center", "middle-right", "bottom-left", "bottom-middle", "bottom-right" };

    public MeshRenderer[] tiles;
    public Material lit, unlit;

    public delegate void GridDelegate(int position);
    public GridDelegate interactionHook;
    public delegate void GridMoveDelegate(Direction d);
    public GridMoveDelegate movementHook;

    public override SectionType type { get { return SectionType.Grid; } }
    private bool[] _states = new bool[9];
    private int _dialInteractionIx;

    private bool _inStage2;
    private int _northLightOffset;
    private Dictionary<int, Direction> lightLookup = new Dictionary<int, Direction>();
    private Coroutine _flickerCoroutine;
    public bool flicker = false;


    public override bool isValid()
    {
        if (wheelDirection == RotDirection.Clockwise)
            return _states.All(x => x);
        else return _states.All(x => !x);
    }

    public override IEnumerator ResetAnim(int cycles)
    {
        _inStage2 = false;
        lightLookup.Clear();

        isAnimating = true;
        if (_flickerCoroutine != null)
            StopCoroutine(_flickerCoroutine);
        flicker = false;
        for (int i = 0; i < cycles; i++)
        {
            for (int tile = 0; tile < 9; tile++)
                if (Ut.RandBool())
                    ToggleLight(tile);
            Audio.PlaySoundAtTransform("beep" + Rnd.Range(0, 3), parentScript.transform);
            yield return new WaitForSeconds(0.2f);
            Audio.PlaySoundAtTransform("beep" + Rnd.Range(0, 3), parentScript.transform);
            yield return new WaitForSeconds(0.2f);
        }
        isAnimating = false;
    }
    private void PressPosition(int ix)
    {
        Audio.PlaySoundAtTransform("beep" + Rnd.Range(0, 3), tiles[ix].transform);
        if (_inStage2)
        {
            if (lightLookup.ContainsKey(ix))
                movementHook.Invoke(lightLookup[ix]);
            return;
        }
        if (!acceptCommands || !parentScript.isInteractable)
            return;
        ToggleLight(ix);
        foreach (int adj in adjacents[ix])
            ToggleLight(adj);
        Log("Manually pressed the {0} light and toggled the cells adjacent to it.", positionNames[ix]);
        LogNewState();
        maze.AddDirections(directionsFromCenter[ix]);
        interactionHook.Invoke(ix);

    }
    private void ToggleLight(int ix)
    {
        _states[ix] = !_states[ix];
        tiles[ix].material = _states[ix] ? lit : unlit;
    }
    protected override void OnStart()
    {
        for (int i = 0; i < 9; i++)
        {
            int ix = i;
            tiles[ix].GetComponent<KMSelectable>().OnInteract += () => { PressPosition(ix); return false; };
            if (Ut.RandBool())
                ToggleLight(ix);
        }
        _dialInteractionIx = Rnd.Range(0, 8);
    }
    protected override void SwitchInteract(RotDirection newDirection)
    {
        string bombTimer = Bomb.GetFormattedTime();
        int[] timerDigits = bombTimer.Where(x => char.IsDigit(x)).Select(ch => ch - '0').ToArray();
        int[] presentDigits = Enumerable.Range(1, 9).Where(x => timerDigits.Contains(x)).ToArray();
        foreach (int digit in presentDigits)
            ToggleLight(digit - 1);
        Log("Switch interaction at {0} toggled the following grid positions: {1}.", bombTimer, presentDigits.Select(x => positionNames[x - 1]).Join(", "));
        LogNewState();
    }
    protected override void DialInteraction(RotDirection rotation, Direction newPosition)
    {
        int[] ring = { 0, 1, 2, 5, 8, 7, 6, 3 };
        _dialInteractionIx += (rotation == RotDirection.Clockwise ? +1 : +7);
        _dialInteractionIx %= 8;
        ToggleLight(ring[_dialInteractionIx]);
        Log("Dial interaction of a {0} rotation rotated the chosen cell {0} to {1} and toggled its state.", rotation.ToString().ToLower(), positionNames[ring[_dialInteractionIx]]);
        LogNewState();
    }
    protected override void NumberInteraction(int newNumber)
    {
        if (newNumber == 0)
            return;
        ToggleLight(newNumber - 1);
        foreach (int adj in adjacents[newNumber - 1])
            ToggleLight(adj);
        Log("Number interaction of setting the value to {0} toggled the {1} light and all the lights adjacent to it.", newNumber, positionNames[newNumber - 1]);
        LogNewState();
    }

    void LogNewState()
    {
        string allStrings = _states.Select(b => b ? "O" : "X").Join("");
        Log("New grid state: {0}|{1}|{2}", allStrings.Substring(0, 3), allStrings.Substring(3, 3), allStrings.Substring(6, 3));
    }


    public void SetBlank()
    {
        flicker = false;
        if (_flickerCoroutine != null)
            StopCoroutine(_flickerCoroutine);
        for (int i = 0; i < 9; i++)
            tiles[i].material = unlit;
    }

    public void ActivateStage2Mode()
    {
        int[] dirLights = { 1, 5, 7, 3 };
        _inStage2 = true;
        _northLightOffset = Rnd.Range(0, 4);
        for (int i = 0; i < 4; i++)
        {
            lightLookup.Add(dirLights[(i + _northLightOffset) % 4], (Direction) i);
            tiles[dirLights[(i + _northLightOffset) % 4]].material = lit;
        }
        flicker = true;
        _flickerCoroutine = StartCoroutine(Flicker(tiles[dirLights[_northLightOffset]]));
    }
    public void FixIfGridSolvePathInvalid(bool reqEven)
    {
        int[][] paths = new int[9][] { new int[] { 0, 2, 5, 6, 7 }, new int[] { 4, 6, 7, 8 }, new int[] { 0, 2, 3, 7, 8 }, new int[] { 2, 4, 5, 8 }, new int[] { 1, 3, 4, 5, 7 }, new int[] { 0, 3, 4, 6 }, new int[] { 0, 1, 5, 6, 8 }, new int[] { 0, 1, 2, 4 }, new int[] { 1, 2, 3, 6, 8 } };
        var goalState = wheelDirection == RotDirection.Clockwise;
        bool[] toggles = new bool[9];
        for (int i = 0; i < 9; i++)
            if (_states[i] != goalState)
                for (int j = 0; j < paths[i].Length; j++)
                    toggles[paths[i][j]] = !toggles[paths[i][j]];
        bool even = true;
        for (int i = 0; i < toggles.Length; i++)
            if (i != 4 && toggles[i])
                even = !even;
        if (even != reqEven)
        {
            // Find a random tile to toggle to make it solvable.
            var candidates = new[] { 0, 1, 2, 3, 5, 6, 7, 8 };
            ToggleLight(candidates[Rnd.Range(0, candidates.Length)]);
        }
    }
    IEnumerator Flicker(MeshRenderer mesh)
    {
        while (flicker)
        {
            mesh.material = lit;
            yield return new WaitForSeconds(Rnd.Range(1.5f, 3f));
            mesh.material = unlit;
            yield return new WaitForSeconds(Rnd.Range(0.25f, 0.67f));
        }
    }
    public override string tpRegex { get { return @"^(?:PRESS\s+)?((?:(?:[1-9]|[TMB][LMR]|[A-C][1-3])(?:\s+|$))+)$"; } }
    public override IEnumerator ProcessTwitchCommand(string command)
    {
        yield return null;
        Match m = Regex.Match(command, tpRegex);
        string[] posNames = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "TL", "TM", "TR", "ML", "MM", "MR", "BL", "BM", "BR", "A1", "B1", "C1", "A2", "B2", "C2", "A3", "B3", "C3" };
        foreach (string arg in m.Groups[1].Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
        {
            tiles[Array.IndexOf(posNames, arg) % 9].GetComponent<KMSelectable>().OnInteract();
            yield return new WaitForSeconds(0.2f);
        }
    }
    public string stage2TpRegex { get { return @"^(?:(?:MOVE|PRESS)\s+)?([URDL]+)$"; } }
    public IEnumerator ProcessStage2TwitchCommand(string command)
    {
        Match m = Regex.Match(command, stage2TpRegex);
        foreach (char movement in m.Groups[1].Value)
        {
            Direction dir = (Direction) "URDL".IndexOf(movement);
            int pressIx = -1;
            foreach (var pair in lightLookup)
                if (pair.Value == dir)
                    pressIx = pair.Key;
            tiles[pressIx].GetComponent<KMSelectable>().OnInteract();
            yield return new WaitForSeconds(0.2f);
        }
    }
}