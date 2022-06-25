using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maze {

	private List<DialDirection> _directionsMoved;
	public RotDirection switchPosition;

	public void AddDirections(IEnumerable<DialDirection> dirs)
    {
		foreach (DialDirection dir in dirs)
        {
            if (switchPosition == RotDirection.Counterclockwise)
                _directionsMoved.Add((DialDirection)((int)dir + 2));
            else _directionsMoved.Add(dir);
        }
    }
}
