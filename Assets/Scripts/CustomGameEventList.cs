using Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGameEventList : MonoBehaviour
{
    public static Action<float> PlayerChangedDressTime;
    public static Action<Appearance> PlayerChangedDressTo;
    public static Action<bool> DressChangeCorrectState;
    public static Action<MovementType> ChangeMovementType;
    public static Action<bool> PlayerCrossedTheFinishLine = delegate( bool _playerWinStatus ) { };
    public static Action<List<bool>> NextMainGatesInfo;

    public static Action<Control, int> CharacterCrossedTheFinishLine;
    public static Action< bool, int> CharacterInSafeArea;

    public static Action<List<Appearance>, List<Vector3>, int, int, Locations> OnEnterArea;

    public static Action<int, int> OnEnterStopLine;

    public static Action<GameState> OnChangeGameState;

    public static Action CutSceneSkipped;
    public static Action SkipLevel;

    public static int TUTORIAL_TO_ENABLE = 0;

    public static Action NextTutorial = delegate() 
    { 
        TUTORIAL_TO_ENABLE++;

        if(TUTORIAL_TO_ENABLE == 3) 
            TUTORIAL_TO_ENABLE = 4;

        TutorialCheckpoinReached(TUTORIAL_TO_ENABLE);
    };
    public static Action<int> TutorialCheckpoinReached;
    public static Action<bool> TurnOnHint = delegate( bool _hintStatus) { };

    public void ChangeMoveType(int t)
    {
        ChangeMovementType.Invoke( (MovementType)t );
    }

}
