using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI text;
    private Action<(int,int), Action<int>> _onSlotClick;
    private (int,int) _slotIndex;
    
    public void Init()
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
            _onSlotClick?.Invoke(_slotIndex,SetStatus));
    }

    public void SetData( (int,int) slotIndex)
    {
        _slotIndex=slotIndex;
    }

    public void AssignEvent(Action<(int,int), Action<int>> onSlotClick)
    {
        _onSlotClick = onSlotClick;
    }
    
    private void SetStatus(int status)
    {
        button.interactable = status == 0;
        text.text = status.ToString();
        switch (status)
        {
            case 1:
                text.text = "X";
                text.color = Color.blue;
                break;
            case 2:
                text.text = "O";
                text.color = Color.red;
                break;
        }
        
    }
}
