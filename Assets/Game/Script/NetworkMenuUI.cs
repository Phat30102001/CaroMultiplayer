// File: NetworkMenuUI.cs
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkMenuUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private GameObject uiPanel;

    private void Start()
    {
        // Bind UI buttons to Unity Netcode actions
        hostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            HideUi();
        });

        joinButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            HideUi();
        });
    }

    private void HideUi()
    {
        if (uiPanel != null) uiPanel.SetActive(false);
    }
}