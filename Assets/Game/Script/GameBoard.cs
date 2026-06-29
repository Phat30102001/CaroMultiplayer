using System;
using UnityEngine;

public class GameBoard
{
    private readonly CellState[,] _matrix;
    public int Width => _matrix.GetLength(0);
    public int Height => _matrix.GetLength(1);

    // Event triggered when a cell state changes
    public event Action<Vector2Int, CellState> OnCellChanged;

    public GameBoard(int size)
    {
        _matrix = new CellState[size, size];
    }

    public CellState GetCell(int x, int y)
    {
        if (!IsWithinBounds(x, y)) return CellState.Empty;
        return _matrix[x, y];
    }

    public bool SetCell(int x, int y, CellState state)
    {
        if (!IsWithinBounds(x, y) || _matrix[x, y] != CellState.Empty) 
            return false;

        _matrix[x, y] = state;
        // Trigger event so the UI can update automatically
        OnCellChanged?.Invoke(new Vector2Int(x, y), state); 
        return true;
    }

    public bool IsWithinBounds(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }
}