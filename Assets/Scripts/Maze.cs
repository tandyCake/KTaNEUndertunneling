using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Maze {

    public const int STARTING_POSITION = 24; //D4
    public static readonly string[] maze = { "UL", "UD", "UD", "UD", "U", "URD", "ULR", "LR", "UL", "UD", "U", "D", "UR", "LR", "L", "D", "UR", "LR", "UL", "D", "DR", "LR", "UL", "D", "R", "L", "URD", "ULR", "LR", "LR", "UL", "RD", "LD", "UD", "R", "LRD", "LRD", "LR", "ULD", "U", "UR", "LR", "ULD", "UD", "D", "UD", "DR", "DL", "DR" };
    private int _curPos = STARTING_POSITION;
    private int _moduleId;
    public RotDirection switchPosition;
    public bool IsSolved { get { return _curPos == STARTING_POSITION; } }

    public Maze(int modId)
    {
        _moduleId = modId;
    }

	public void AddDirections(IEnumerable<Direction> dirs)
    {
		foreach (Direction dir in dirs)
        {
            if (switchPosition == RotDirection.Counterclockwise)
            {
                Direction newDir = (Direction)(((int)dir + 2) % 4);
                Log("Moved {0} in the maze. (inverted because switch = ccw)", newDir);
                _curPos = Move(newDir);
            }
            else
            {
                Log("Moved {0} in the maze.", dir);
                _curPos = Move(dir);
            }
        }
    }
    private int Move(Direction dir)
    {
        int x = _curPos % 7;
        int y = _curPos / 7;
        switch (dir)
        {
            case Direction.Up: return Flatten(x, (y + 6) % 7);
            case Direction.Right: return Flatten((x + 1) % 7, y);
            case Direction.Down: return Flatten(x, (y + 1) % 7);
            case Direction.Left: return Flatten((x + 6) % 7, y);
            default: throw new System.ArgumentOutOfRangeException();
        }
    }
    public bool TryMove(Direction dir)
    {
        if (maze[_curPos].Contains(dir.ToString()[0]))
        {
            Log("Unsuccessfully tried to move {0} from {1}.", dir, ToCoordinate(_curPos));
            return false;
        }
        int next = Move(dir);
        Log("Successfully moved {0} from {1} to {2}.", dir, ToCoordinate(_curPos), ToCoordinate(next));
        _curPos = next;
        return true;
    }
    private int Flatten(int x, int y)
    {
        return 7 * y + x;
    }
    private string ToCoordinate(int val)
    {
        return "ABCDEFG"[val % 7].ToString() + (val / 7 + 1);
    }
    public void Reset()
    {
        _curPos = STARTING_POSITION;
    }
    private void Log(string msg, params object[] args)
    {
        Debug.LogFormat("[Undertunneling #{0}] {1}", _moduleId, string.Format(msg, args));
    }
}
