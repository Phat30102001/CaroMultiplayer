using System;
using UnityEngine;

public class LocalPlayerController : IPlayerController
{
    public CellState Sign { get; }
    private readonly BoardView _boardView;
    private Action<Vector2Int> _pendingMoveCallback;

    public LocalPlayerController(CellState sign, BoardView boardView)
    {
        Sign = sign;
        _boardView = boardView;
    }

    public void RequestMove(GameBoard board, Action<Vector2Int> onMoveCalculated)
    {
        _pendingMoveCallback = onMoveCalculated;
        // Listen to click events from the board view UI
        _boardView.OnSlotSelected += HandleUiClick;
    }

    private void HandleUiClick(Vector2Int coords)
    {
        // Unsubscribe immediately to prevent multiple clicks within a single turn
        _boardView.OnSlotSelected -= HandleUiClick;
        
        // Return the move coordinates to the orchestrator
        _pendingMoveCallback?.Invoke(coords);
    }
}