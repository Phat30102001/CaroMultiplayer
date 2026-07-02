// File: NetworkMenuUI.cs
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkMenuUI : MonoBehaviour
{
    public static NetworkMenuUI Instance { get; private set; }

    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private GameObject uiPanel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
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

    public void ShowUi()
    {
        if (uiPanel != null) uiPanel.SetActive(true);
    }

    private void HideUi()
    {
        if (uiPanel != null) uiPanel.SetActive(false);
    }
}