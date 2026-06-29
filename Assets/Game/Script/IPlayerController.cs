using System;
using UnityEngine;
public interface IPlayerController
{
    CellState Sign { get; } // The player's piece sign (X or O)
    
    // Request the player to make a move, then return the coordinates via the callback
    void RequestMove(GameBoard board, Action<Vector2Int> onMoveCalculated);
}

// 1. LOCAL PLAYER (Mouse clicks on this device)

// 2. NETWORK PLAYER (Wait for move data sent from the network)
public class NetworkPlayerController : IPlayerController
{
    public CellState Sign { get; }
    private Action<Vector2Int> _pendingMoveCallback;

    public NetworkPlayerController(CellState sign)
    {
        Sign = sign;
    }

    public void RequestMove(GameBoard board, Action<Vector2Int> onMoveCalculated)
    {
        // Do nothing here because the opponent does not click on this device. Just wait...
        _pendingMoveCallback = onMoveCalculated;
    }

    // This method will be called from your Network Manager script when a packet arrives
    public void ReceiveMoveFromNetwork(Vector2Int coords)
    {
        _pendingMoveCallback?.Invoke(coords);
    }
}