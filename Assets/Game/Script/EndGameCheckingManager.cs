// File: EndGameCheckingManager.cs
using UnityEngine;

public class EndGameCheckingManager
{
    // Define 4 axes using Vector2Int (Each array contains a pair of opposite directions)
    private readonly Vector2Int[][] _checkAxes = new Vector2Int[][]
    {
        new Vector2Int[] { Vector2Int.left, Vector2Int.right },          // Horizontal axis
        new Vector2Int[] { Vector2Int.up, Vector2Int.down },             // Vertical axis
        new Vector2Int[] { new Vector2Int(-1, 1), new Vector2Int(1, -1) }, // Diagonal axis (\)
        new Vector2Int[] { new Vector2Int(1, 1), new Vector2Int(-1, -1) }  // Anti-diagonal axis (/)
    };

    /// <summary>
    /// Checks if the last move creates a winning condition.
    /// </summary>
    public bool CheckWin(GameBoard board, Vector2Int lastMove, CellState playerSign)
    {
        foreach (Vector2Int[] axis in _checkAxes)
        {
            int consecutiveCount = 1; // Include the piece just placed

            foreach (Vector2Int direction in axis)
            {
                consecutiveCount += CountInDirection(board, lastMove, direction, playerSign);
            }

            if (consecutiveCount >= 5) 
            {
                return true; 
            }
        }
        return false;
    }

    /// <summary>
    /// Counts consecutive pieces of the same player sign in a specific direction.
    /// </summary>
    private int CountInDirection(GameBoard board, Vector2Int start, Vector2Int dir, CellState playerSign)
    {
        int count = 0;
        Vector2Int current = start + dir;

        // Loop forward while inside bounds and matching the player's sign
        while (board.IsWithinBounds(current.x, current.y) && board.GetCell(current.x, current.y) == playerSign)
        {
            count++;
            current += dir;
        }
        return count;
    }
}