using System;
using System.Collections.Generic;

public class GameplayFlowController
{
    private int[] participants;
    private int currentTurnIndex;
    public int CurrentTurn => participants[currentTurnIndex];
    public void Init(int participantCount)
    {
        participants= new int[participantCount];
        for (int i = 0; i < participantCount; i++)
        {
            participants[i] = i+1;
        }
        currentTurnIndex = 0;
    }

    public void MoveToNextTurn(Action callback)
    {
        if (currentTurnIndex < participants.Length - 1)
        {
            currentTurnIndex++;
        }
        else
        {
            currentTurnIndex = 0;
        }
        callback?.Invoke();
    }
    
}