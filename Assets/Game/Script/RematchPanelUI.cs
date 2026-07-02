// File: RematchPanelUI.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RematchPanelUI : MonoBehaviour
{
    public static RematchPanelUI Instance { get; private set; }

    [SerializeField] private GameObject panelContainer;
    [SerializeField] private Button rematchButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TextMeshProUGUI statusText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        HidePanel();
    }

    private void Start()
    {
        rematchButton.onClick.AddListener(OnRematchClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
    }

    public void ShowPanel(string resultMessage)
    {
        panelContainer.SetActive(true);
        rematchButton.interactable = true;
        cancelButton.interactable = true;
        statusText.text = resultMessage;
    }

    public void HidePanel()
    {
        panelContainer.SetActive(false);
    }

    private void OnRematchClicked()
    {
        rematchButton.interactable = false;
        statusText.text = "Waiting for opponent's agreement...";
        
        // Notify the network manager that this local client wants a rematch
        NetworkGameManager.Instance.SubmitRematchVoteServerRpc(true);
    }

    private void OnCancelClicked()
    {
        rematchButton.interactable = false;
        cancelButton.interactable = false;
        statusText.text = "Canceling match...";
        
        // Notify the network manager that this local client declined
        NetworkGameManager.Instance.SubmitRematchVoteServerRpc(false);
    }
}