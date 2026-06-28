using System.Collections.Generic;
using System;
using UnityEngine;

public class BoardManager : MonoBehaviour
{ 
    private EndGameCheckingManager endGameCheckingManager= new EndGameCheckingManager();
    private GameplayFlowController gameplayFlowController= new GameplayFlowController();
    
    [SerializeField] private int boardSize = 15;
    [SerializeField] private float slotSpacing = 0.1f;
    [SerializeField] private float slotSize;

    [SerializeField] private Slot slotPrefab;
    [SerializeField] private Transform board;
    [SerializeField] private List<Slot> slots;
    private int[,] slotsStatus;
    public int[,] SlotsStatus => slotsStatus;
    

    private void Start()
    {
        slotsStatus = new int[boardSize, boardSize];
        endGameCheckingManager.Init(slotsStatus);
        // set gameplay flow to 2 player
        gameplayFlowController.Init(2);
        slotSize = slotPrefab.GetComponent<RectTransform>().sizeDelta.x;
        GenerateBoard();
    }

    private void GenerateBoard()
    {
        // generate grid board
        float offset = (boardSize % 2 == 0)
            ? (boardSize / 2 - 0.5f) * slotSize
            : (boardSize / 2) * slotSize;
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                var slot = Instantiate(slotPrefab, board);
                float xPos = (i * slotSize)  - offset;
                float yPos = (j * slotSize)  - offset;

                slot.transform.localPosition = new Vector2(xPos, yPos);
                slot.Init();
                slot.AssignEvent(OnUpdateSlotStatus);
                slot.SetData((i,j));
                slots.Add(slot);
            }

        }



    }
    private void OnUpdateSlotStatus((int x,int y) slotIndex, Action<int> callback)
    {
       bool isWin= endGameCheckingManager.CheckWin(slotIndex.x, slotIndex.y, gameplayFlowController.CurrentTurn);
       callback(gameplayFlowController.CurrentTurn);
       if (isWin)
       {
           Debug.Log("Player " + gameplayFlowController.CurrentTurn + " Won");
       }
       else
       {
           gameplayFlowController.MoveToNextTurn((() =>
           {
               Debug.Log("Player " + gameplayFlowController.CurrentTurn + " Turn");
           }));
       }
    }



}
