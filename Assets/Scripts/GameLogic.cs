using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;

public enum GameState
{
    undefined,

    GameStarted,

    Breakshot,
    BreakshotBallInHand,

    OpenTable,
    OpenTableTableInPosition,
    OpenTableBallInHand,

    MainPhase,
    MainPhaseTableInPosition,
    MainPhaseBallInHand,

    GameOver
}
public class GameLogic : MonoBehaviour
{
    [SerializeField] private GameEvent cbReadyForStrike, cbReadyForRepositioning, respotCueball,respotEightball, rerack, passTurn;
    [SerializeField] private GameState currentState, pendingState;
    [SerializeField] private bool playerOneTurn;
    private int solidsLeft, stripesLeft;
    [SerializeField] private BallType playerOneBallType, playerTwoBallType;
    private string player, opponent;
    private void Start()
    {
        StartGame();
    }
    private void StartGame()
    {
        currentState = GameState.GameStarted;
        solidsLeft = stripesLeft = 7;
        PassTurn();
        player = "Player One";
    }

    public void RestartGame()
    {
        ClearShotInfo();
        rerack.Raise();
        StartGame();
    }

    public void CueballHit()
    {
        Debug.Log("Cueball hit!");
        switch (currentState)
        {
            case GameState.BreakshotBallInHand:
                currentState = GameState.Breakshot;
                break;

            case GameState.OpenTableBallInHand:
            case GameState.OpenTableTableInPosition:
                currentState = GameState.OpenTable;
                break;

            case GameState.MainPhaseBallInHand:
            case GameState.MainPhaseTableInPosition:
                currentState = GameState.MainPhase;
                break;
        }
    }
    public void TableSet()
    {
        Debug.Log("Table set!");
        if (currentState == GameState.GameOver) return;

        switch (currentState)
        {
            case GameState.OpenTableTableInPosition:
                cbReadyForStrike.Raise();
                return;

            case GameState.BreakshotBallInHand:
            case GameState.OpenTableBallInHand:
            case GameState.MainPhaseBallInHand:
                cbReadyForRepositioning.Raise();
                cbReadyForStrike.Raise();  
                return;
        }

        if (currentState == GameState.GameStarted)
        {
            currentState = GameState.BreakshotBallInHand;
            cbReadyForRepositioning.Raise();
            cbReadyForStrike.Raise();
            ClearShotInfo();
            return;
        }

        if (pendingState == GameState.GameStarted)
        {
            rerack.Raise();
            PassTurn();
            currentState = GameState.GameStarted;
            ClearShotInfo();
            return;
        }
        
        if (pendingState == GameState.BreakshotBallInHand)
        {
            respotCueball.Raise(true);//behind the headstring
            PassTurn();
            currentState = GameState.BreakshotBallInHand;
            ClearShotInfo();
            return;
        }
        if (pendingState == GameState.OpenTableBallInHand)
        {
            respotCueball.Raise(false);//anywhere on the table
            PassTurn();
            currentState = GameState.OpenTableBallInHand;
            ClearShotInfo();
            return;
        }
        if (pendingState == GameState.MainPhaseBallInHand)
        {
            respotCueball.Raise(false);//anywhere on the table
            PassTurn();
            currentState = GameState.MainPhaseBallInHand;
            ClearShotInfo();
            return;
        }

        if (currentState == GameState.Breakshot)
        {
            if (ballsPocketed.Count == 0)
            {
                if (railHits.Count < 4)
                {
                    Debug.Log("Illegal Break");
                    rerack.Raise();
                    PassTurn();
                    currentState = GameState.GameStarted;
                    ClearShotInfo();
                    return;
                }

                PassTurn();
                if (eightballPocketed)
                {
                    respotEightball.Raise(); 
                }
                else
                {
                    cbReadyForStrike.Raise();
                }
                currentState = GameState.OpenTableTableInPosition;
                ClearShotInfo();
                return;
            }
            if (eightballPocketed)
            {
                respotEightball.Raise();
            }
            else
            {
                cbReadyForStrike.Raise();
            }
            currentState = GameState.OpenTableTableInPosition;
            ClearShotInfo();
            return;
        }

        if (currentState == GameState.OpenTable)
        {
            if (firstHitBall == null){
                Debug.Log("Foul! No object ball hit by cueball.");
                PassTurn();
                respotCueball.Raise(false);
                currentState = GameState.OpenTableBallInHand;
                ClearShotInfo();
                return;
            }
            if (ballsPocketed.Count == 0)
            {
                if (railHits.Count == 0)
                {
                    Debug.Log("Foul! No rail hit.");
                    PassTurn();
                    respotCueball.Raise(false);
                    currentState = GameState.OpenTableBallInHand;
                    ClearShotInfo();
                    return;
                }
                PassTurn();
                cbReadyForStrike.Raise();
                currentState = GameState.OpenTableTableInPosition;
                ClearShotInfo();
                return;
            }
            AssignGroups();
            cbReadyForStrike.Raise();
            currentState =GameState.MainPhaseTableInPosition;
            ClearShotInfo();
            return;
        }
        if (currentState == GameState.MainPhase)
        {
            if (eightballPocketed)
            {
                player = playerOneTurn ? "Player One" : "Player Two";
                Debug.Log($"{player} wins!");
                currentState = GameState.GameOver;
                return;
            }
         
            if (firstHitBall == null)
            {
                Debug.Log("Foul! No object ball hit by cueball.");
                PassTurn();
                respotCueball.Raise(false);
                currentState = GameState.MainPhaseBallInHand;
                ClearShotInfo();
                return;
            }

            if (ballsPocketed.Count == 0)
            {
                if (railHits.Count == 0)
                {
                    Debug.Log("Foul! No rail hit.");
                    PassTurn();
                    respotCueball.Raise(false);
                    currentState = GameState.MainPhaseBallInHand;
                    ClearShotInfo();
                    return;
                }
                PassTurn();
                cbReadyForStrike.Raise();
                currentState = GameState.MainPhaseTableInPosition;
                ClearShotInfo();
                return;
            }
            if (!ballsPocketed.Exists((b) => b.GetBallType() == GetCorrectBallType())){
               PassTurn();
            }

            cbReadyForStrike.Raise();
            currentState = GameState.MainPhaseTableInPosition;
            ClearShotInfo();
            return;
        }
    }


    private void AssignGroups()
    {
        BallType AssignedGroup = solidsLeft < stripesLeft ? BallType.Solid : BallType.Stripe;

        if (playerOneTurn)
        {
            playerOneBallType = AssignedGroup;
            playerTwoBallType = AssignedGroup == BallType.Solid ? BallType.Stripe : BallType.Solid;
        }
        else
        {
            playerTwoBallType = AssignedGroup;
            playerOneBallType = AssignedGroup == BallType.Solid ? BallType.Stripe : BallType.Solid;
        }
    }
    private void ClearShotInfo()
    {
        updateEight();
        ballsPocketed.Clear();
        railHits.Clear();
        firstHitBall = null;
        eightballPocketed = false;
        pendingState = GameState.undefined;
    }

    private void updateEight()
    {
        if ((playerOneBallType == BallType.Solid && solidsLeft == 0) || (playerOneBallType == BallType.Stripe && stripesLeft == 0))
        {
            playerOneBallType = BallType.Eight;
        }
        if ((playerTwoBallType == BallType.Solid && solidsLeft == 0) || (playerTwoBallType == BallType.Stripe && stripesLeft == 0))
        {
            playerTwoBallType = BallType.Eight;
        }
    }

    private bool CheckEight()
    {
        if(playerOneTurn) return playerOneBallType == BallType.Eight;
        return playerTwoBallType == BallType.Eight; 
    }
    private void PassTurn()
    {
        playerOneTurn = !playerOneTurn;
        passTurn.Raise(playerOneTurn);
    }
    private BallType GetCorrectBallType()
    {
        if(playerOneTurn) return playerOneBallType;
        return playerTwoBallType;
    }

    public void BallOffTheTable(object ball)
    {
        Debug.Log($"{((Ball)ball)}off the table!");
        BallType t = ((Ball)ball).GetBallType();
        if (currentState == GameState.Breakshot)
        {
            if (t == BallType.Cue || t == BallType.Eight)
            {
                pendingState = GameState.GameStarted;
            }
            return;
        }
        if (t == BallType.Eight)
        {
            currentState = GameState.GameOver;
            return;
        }
        if (t == BallType.Cue)
        {
            pendingState = currentState == GameState.OpenTable ? GameState.OpenTableBallInHand : GameState.MainPhaseBallInHand;
            return;
        }

        if (t == BallType.Solid) solidsLeft--;
        else if (t == BallType.Stripe) stripesLeft--;
        pendingState = currentState == GameState.OpenTable ? GameState.OpenTableBallInHand : GameState.MainPhaseBallInHand;

    }

    private List<Ball> ballsPocketed = new List<Ball>();
    private bool eightballPocketed;

    public void BallPocketed(object ball)
    {
        Debug.Log($"{((Ball)ball)} pocketed!");
        BallType t = ((Ball)ball).GetBallType();
        if (t == BallType.Eight)
        {
            if (currentState == GameState.Breakshot)
            {
                eightballPocketed = true;
                return;
            }

            if (currentState == GameState.OpenTable)
            {
                opponent= playerOneTurn ? "Player Two" : "Player One";
                Debug.Log($"Eightball pocketed. {opponent} wins!");
                currentState = GameState.GameOver;
                return;
            }
            if (currentState == GameState.MainPhase)
            {
                if (!CheckEight())
                {
                    opponent = playerOneTurn ? "Player Two" : "Player One";
                    Debug.Log($"Eightball pocketed. {opponent} wins!");
                    currentState = GameState.GameOver;
                    return;
                }
                eightballPocketed = true;
                return;
            }
        }

        if (t == BallType.Cue)
        {
            Debug.Log("Foul! Cueball pocketed.");
            switch (currentState)
            {
                case GameState.Breakshot:
                    pendingState = GameState.BreakshotBallInHand;
                    break;
                case GameState.OpenTable:
                    pendingState = GameState.OpenTableBallInHand;
                    break;
                case GameState.MainPhase:
                    pendingState = GameState.MainPhaseBallInHand;
                    break;
            }

            return;
        }
        if (t == BallType.Solid) solidsLeft--;
        if (t == BallType.Stripe) stripesLeft--;
        ballsPocketed.Add((Ball)ball);
    }

    private List<Ball> railHits = new List<Ball>();
    public void RailHit(object ball)
    {
        Debug.Log($"{((Ball)ball)} hit the rail!");
        Ball b = (Ball)ball;

        if (currentState == GameState.Breakshot && b.GetBallType() == BallType.Cue) return;

        if (!railHits.Contains(b))
        {
            railHits.Add(b);
        }
    }

    private BallType? firstHitBall;

    public void HitByCueball(object ball)
    {
        Debug.Log($"{((Ball)ball)} hit by cueball!");
        if (currentState == GameState.Breakshot) return;
        if (firstHitBall != null) return;
        firstHitBall = ((Ball)ball).GetBallType();

        if (currentState == GameState.OpenTable)
        {
            if (firstHitBall == BallType.Eight)
            {
                Debug.Log("Foul! Wrong ball hit.");
                pendingState = GameState.OpenTableBallInHand;
            }
            return;
        }
        if (firstHitBall != GetCorrectBallType())
        {
            Debug.Log("Foul! Wrong ball hit.");
            pendingState = GameState.MainPhaseBallInHand;
            return;
        }
    }
}
