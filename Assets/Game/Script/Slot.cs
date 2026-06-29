// File: Slot.cs
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI text;
    
    public event Action<Vector2Int> OnSlotClicked;
    private Vector2Int _coords;

    public void Init(Vector2Int coords)
    {
        _coords = coords;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnSlotClicked?.Invoke(_coords));
    }

    public void UpdateVisual(CellState state)
    {
        button.interactable = (state == CellState.Empty);
        
        switch (state)
        {
            case CellState.Empty:
                text.text = "";
                break;
            case CellState.X:
                text.text = "X";
                text.color = Color.blue;
                break;
            case CellState.O:
                text.text = "O";
                text.color = Color.red;
                break;
        }
    }
}