using System;
using System.Collections.Generic;
using UnityEngine;


public class BoardView : MonoBehaviour
{
    [SerializeField] private Slot slotPrefab;
    [SerializeField] private Transform boardContainer;
    
    private readonly Dictionary<Vector2Int, Slot> _slotsMap = new Dictionary<Vector2Int, Slot>();
    public event Action<Vector2Int> OnSlotSelected;

    public void GenerateBoard(int boardSize, float slotSize)
    {
        // Generate grid board layout
        float offset = (boardSize % 2 == 0)
            ? (boardSize / 2 - 0.5f) * slotSize
            : (boardSize / 2) * slotSize;
        
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                Slot slot = Instantiate(slotPrefab, boardContainer);
                float xPos = (i * slotSize) - offset;
                float yPos = (j * slotSize) - offset;

                slot.transform.localPosition = new Vector2(xPos, yPos);
                Vector2Int coords = new Vector2Int(i, j);
                slot.Init(coords);
                slot.OnSlotClicked += (c) => OnSlotSelected?.Invoke(c);

                _slotsMap[coords] = slot;
            }
        }
    }

    public void UpdateSlotVisual(Vector2Int coords, CellState state)
    {
        if (_slotsMap.TryGetValue(coords, out Slot slot))
        {
            slot.UpdateVisual(state);
        }
    }
    // Inside File: BoardView.cs

    public void ResetAllSlots()
    {
        // Loop through all generated slots and wipe their visual states clean
        foreach (Slot slot in _slotsMap.Values)
        {
            slot.UpdateVisual(CellState.Empty);
        }
    }
}