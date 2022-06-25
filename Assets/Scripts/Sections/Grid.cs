using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class Grid : Section {

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

    public MeshRenderer[] tiles;
    public Material lit, unlit;

    public delegate void GridDelegate(int position);
    public GridDelegate interactionHook;

    public override SectionType type { get { return SectionType.Grid; } }
    private bool[] _states = new bool[9];
    private int dialInteractionIx;
    public override bool isValid()
    {
        if (wheelDirection == RotDirection.Clockwise)
            return _states.All(x => x);
        else return _states.All(x => !x);
    }

    public override IEnumerator ResetAnim(int cycles)
    {
        isAnimating = true;
        for (int i = 0; i < cycles; i++)
        {
            for (int tile = 0; tile < 9; tile++)
                if (Ut.RandBool())
                    ToggleLight(tile);
            yield return new WaitForSeconds(0.2f);
        }
        isAnimating = false;
    }
    private void PressPosition(int ix)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, tiles[ix].transform);
        ToggleLight(ix);
        foreach (int adj in adjacents[ix])
            ToggleLight(adj);
        maze.AddDirections(directionsFromCenter[ix]);
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
        dialInteractionIx = Rnd.Range(0, 8);
    }
    protected override void SwitchInteract(RotDirection newDirection)
    {
        string bombTimer = Bomb.GetFormattedTime();
        int[] timerDigits = bombTimer.Where(x => char.IsDigit(x)).Select(ch => ch - '0').ToArray();
        for (int i = 1; i < 10; i++)
            if (timerDigits.Contains(i))
                ToggleLight(i - 1);
    }
    protected override void DialInteraction(RotDirection rotation, Direction newPosition)
    {
        int[] ring = { 0, 1, 2, 5, 8, 7, 6, 3 };
        dialInteractionIx += (rotation == RotDirection.Clockwise ? +1 : +7);
        dialInteractionIx %= 8;
        ToggleLight(ring[dialInteractionIx]);
    }
    protected override void NumberInteraction(int newNumber)
    {
        if (newNumber == 0)
            return;
        ToggleLight(newNumber - 1);
        foreach (int adj in adjacents[newNumber - 1])
            ToggleLight(adj);
    }
}