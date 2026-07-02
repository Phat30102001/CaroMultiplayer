// File: NetworkGameManager.cs
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameOrchestrator orchestrator;

    // Track the rematch votes using Client ID as the key
    private readonly Dictionary<ulong, bool> _rematchVotes = new Dictionary<ulong, bool>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _rematchVotes.Clear();
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            CheckAndStartMatch();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[NetworkGameManager] Client connected: {clientId}");
        CheckAndStartMatch();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"[NetworkGameManager] Client disconnected: {clientId}");
        // If anyone disconnects during the game or endgame lobby, force terminate the session
        TerminateMatchClientRpc("Opponent has left the game.");
    }

    private void CheckAndStartMatch()
    {
        if (NetworkManager.Singleton.ConnectedClients.Count == 2)
        {
            Debug.Log("[NetworkGameManager] Both players ready. Rolling roles...");
            DeterminePlayerRoles();
        }
    }

    private void DeterminePlayerRoles()
    {
        // Randomize roles completely fresh every time a match initializes
        CellState hostRole = (Random.value > 0.5f) ? CellState.X : CellState.O;
        CellState clientRole = (hostRole == CellState.X) ? CellState.O : CellState.X;

        AssignRolesClientRpc(hostRole, clientRole);
    }

    [ClientRpc]
    private void AssignRolesClientRpc(CellState hostRole, CellState clientRole)
    {
        CellState myAssignedSign = IsServer ? hostRole : clientRole;
        Debug.Log($"[NetworkGameManager] Match Initialized. Local Sign: {myAssignedSign}");
        
        RematchPanelUI.Instance.HidePanel();
        orchestrator.InitNetworkMatch(myAssignedSign);
    }

    /// <summary>
    /// Sent by clients to vote for a rematch (true = accept, false = decline)
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void SubmitRematchVoteServerRpc(bool wantsRematch, ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        _rematchVotes[senderId] = wantsRematch;

        // If a player declines the rematch, immediately abort and kick everyone out
        if (!wantsRematch)
        {
            TerminateMatchClientRpc("Rematch declined by opponent.");
            return;
        }

        // Check if both players have voted, and both agreed
        if (_rematchVotes.Count == 2)
         {
             bool everyoneAgreed = true;
             foreach (var vote in _rematchVotes.Values)
             {
                 if (!vote) everyoneAgreed = false;
             }

             if (everyoneAgreed)
             {
                 Debug.Log("[NetworkGameManager] Rematch consensus reached! Restarting...");
                 _rematchVotes.Clear();
                 DeterminePlayerRoles(); // Spin up a new round with fresh random signs!
             }
         }
    }

    [ClientRpc]
    private void TerminateMatchClientRpc(string reason)
    {
        Debug.Log($"[NetworkGameManager] Terminating session. Reason: {reason}");
        
        // Clean UI states
        RematchPanelUI.Instance.HidePanel();
        
        // Hard shutdown of the Netcode system connection
        NetworkManager.Singleton.Shutdown();
        
        // Restore the main connection menu UI
        NetworkMenuUI.Instance.ShowUi();
    }

    [ServerRpc(RequireOwnership = false)]
    public void BroadcastMoveServerRpc(Vector2Int coords, CellState sign)
    {
        ReceiveMoveClientRpc(coords, sign);
    }

    [ClientRpc]
    private void ReceiveMoveClientRpc(Vector2Int coords, CellState sign)
    {
        orchestrator.OnReceiveNetworkMove(coords, sign);
    }
}