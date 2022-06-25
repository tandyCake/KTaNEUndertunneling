using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maze {

    public const int STARTING_POSITION = 24; //D4
    public static readonly string[] maze = { "UL", "UD", "UD", "UD", "U", "URD", "ULR", "LR", "UL", "UD", "U", "D", "UR", "LR", "L", "D", "UR", "LR", "UL", "D", "DR", "LR", "UL", "D", "R", "L", "URD", "ULR", "LR", "LR", "UL", "RD", "LD", "UD", "R", "LRD", "LRD", "LR", "ULD", "U", "UR", "LR", "ULD", "UD", "D", "UD", "DR", "DL", "DR" };
    private int curPos = STARTING_POSITION;
    public RotDirection switchPosition;

	public void AddDirections(IEnumerable<Direction> dirs)
    {
		foreach (Direction dir in dirs)
        {
            if (switchPosition == RotDirection.Counterclockwise)
                curPos = Move((Direction)((int)dir + 2));
            else curPos = Move(dir);
        }
    }
    private int Move(Direction dir)
    {
        int x = curPos % 7;
        int y = curPos / 7;
        switch (dir)
        {
            case Direction.Up: return Flatten(x, (y + 6) % 7);
            case Direction.Right: return Flatten((x + 1) % 7, y);
            case Direction.Down: return Flatten(x, (y + 1) % 7);
            case Direction.Left: return Flatten((x + 6) % 7, y);
            default: throw new System.ArgumentOutOfRangeException();
        }
    }
    private int Flatten(int x, int y)
    {
        return 7 * y + x;
    }
}
