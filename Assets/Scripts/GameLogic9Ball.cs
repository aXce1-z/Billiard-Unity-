using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GameState9Ball
{
    Undefined,

    GameStarted,

    Breakshot,
    BreakshotBallInHand,
    
    PlayingPhase,
    PlayingPhaseTableInPosition,
    PlayingPhaseBallInHand,
    GameOver
}

public class GameLogic9Ball : MonoBehaviour
{
    [SerializeField] private GameEvent cbReadyForStrike, cbReadyForRepositioning, respotCueball, rerack, passTurn, dialogMessageEvent, restartGame;
    [SerializeField] private GameState9Ball currentState, pendingState;
    [SerializeField] private bool playerOneTurn;
    [SerializeField] public int currentTargetNumber = 1;
    private string player, opponent;

    private void Start()
    {
        StartGame();
    }

    private void StartGame()
    {
        currentState = GameState9Ball.GameStarted;
        currentTargetNumber = 1;
        PassTurn();
        player = "Player 1";
    }

    public void RestartGame()
    {
        playerOneTurn = false;
        ClearShotInfo();
        rerack.Raise();
        StartGame();
    }

    public void CueballHit()
    {
        Debug.Log("Cueball hit!");
        switch (currentState)
        {
            case GameState9Ball.BreakshotBallInHand:
                currentState = GameState9Ball.Breakshot;
                break;
            case GameState9Ball.PlayingPhaseBallInHand:
            case GameState9Ball.PlayingPhaseTableInPosition:
                currentState = GameState9Ball.PlayingPhase;
                break;
        }
    }

    public void TableSet()
    {
        Debug.Log("Table set!");
        if (currentState == GameState9Ball.GameOver) return;

        switch (currentState)
        {
            case GameState9Ball.PlayingPhaseTableInPosition:
                cbReadyForStrike.Raise();
                return;

            case GameState9Ball.BreakshotBallInHand:
            case GameState9Ball.PlayingPhaseBallInHand:
                cbReadyForRepositioning.Raise();
                cbReadyForStrike.Raise();
                return;
        }

        if (currentState == GameState9Ball.GameStarted)
        {
            currentState = GameState9Ball.BreakshotBallInHand;
            cbReadyForRepositioning.Raise();
            cbReadyForStrike.Raise();
            ClearShotInfo();
            return;
        }

        if (pendingState == GameState9Ball.GameStarted)
        {
            rerack.Raise();
            PassTurn();
            currentState = GameState9Ball.GameStarted;
            ClearShotInfo();
            return;
        }

        if (pendingState == GameState9Ball.BreakshotBallInHand)
        {
            respotCueball.Raise(true);//behind the headstring
            PassTurn();
            currentState = GameState9Ball.BreakshotBallInHand;
            ClearShotInfo();
            return;
        }

        if (pendingState == GameState9Ball.PlayingPhaseBallInHand)
        {
            respotCueball.Raise(false);//anywhere on the table
            PassTurn();
            currentState = GameState9Ball.PlayingPhaseBallInHand;
            ClearShotInfo();
            return;
        }

        if (currentState == GameState9Ball.Breakshot)
        {
            if (ballsPocketed.Count == 0)
            {
                if (railHits.Count < 4)
                {
                    dialogMessageEvent.Raise("Illegal Break!");
                    rerack.Raise();
                    PassTurn();
                    currentState = GameState9Ball.GameStarted;
                    ClearShotInfo();
                    return;
                }

                PassTurn();
                cbReadyForStrike.Raise();
                currentState = GameState9Ball.PlayingPhaseTableInPosition;
                ClearShotInfo();
                return;
            }

            if (nineBallPocketed)
            {
                GameVictory();
                return;

            }
            else
            {
                UpdateTargetNumber();
                cbReadyForStrike.Raise();
            }
            currentState = GameState9Ball.PlayingPhaseTableInPosition;
            ClearShotInfo();
            return;
        }

        if (currentState == GameState9Ball.PlayingPhase)
        {
            if (nineBallPocketed)
            {
                if (firstHitBall.GetNumber()==currentTargetNumber && !ballsPocketed.Exists((b) => b.GetBallType()==BallType.Cue))
                {
                    player = playerOneTurn ? "Player 1" : "Player 2";
                    dialogMessageEvent.Raise($"{player} wins!");
                    currentState = GameState9Ball.GameOver;
                    return;
                }
                else
                {
                    opponent = playerOneTurn ? "Player 2" : "Player 1";
                    dialogMessageEvent.Raise($"{opponent} wins!");
                    currentState = GameState9Ball.GameOver;
                    return;
                }
            }

            if (firstHitBall == null)
            {
                dialogMessageEvent.Raise("Foul! No object ball hit by cueball.");
                PassTurn();
                respotCueball.Raise(false);
                currentState = GameState9Ball.PlayingPhaseBallInHand;
                ClearShotInfo();
                return;
            }

            if (ballsPocketed.Count == 0)
            {
                if (railHits.Count == 0)
                {
                    dialogMessageEvent.Raise("Foul! No rail hit.");
                    PassTurn();
                    respotCueball.Raise(false);
                    currentState = GameState9Ball.PlayingPhaseBallInHand;
                    ClearShotInfo();
                    return;
                }
                PassTurn();
                cbReadyForStrike.Raise();
                currentState = GameState9Ball.PlayingPhaseTableInPosition;
                ClearShotInfo();
                return;
            }
            if (!ballsPocketed.Exists((b) => b.GetNumber() == currentTargetNumber))
            {
                PassTurn();
            }
            UpdateTargetNumber();
            cbReadyForStrike.Raise();
            currentState = GameState9Ball.PlayingPhaseTableInPosition;
            ClearShotInfo();
            return;

            
        }
    }


    
   


    private void UpdateTargetNumber()
    {
        int newTarget = currentTargetNumber;
        foreach (Ball b in ballsPocketed.OrderBy(b => b.GetNumber()))
        {
            if (b.GetNumber() == newTarget)
            {
                newTarget++;
            }
        }
        currentTargetNumber = Mathf.Min(newTarget, 9);
    }

    private bool IsLegalNineBallShot()
    {
        return currentTargetNumber == 9 && firstHitBall?.GetNumber() == 9;
    }

    private void GameVictory(bool opponent = false)
    {
        string winner = opponent ? (playerOneTurn ? "Player 2" : "Player 1") : (playerOneTurn ? "Player 1" : "Player 2");
        dialogMessageEvent.Raise($"{winner} wins!");
        currentState = GameState9Ball.GameOver;
    }

    private void ClearShotInfo()
    {
        railHits.Clear();
        firstHitBall = null;
        nineBallPocketed = false;
        waitForRail = false;
        pendingState = GameState9Ball.Undefined;
    }

    private void PassTurn()
    {
        playerOneTurn = !playerOneTurn;
        passTurn.Raise(playerOneTurn);
        opponent = playerOneTurn ? "Player 2" : "Player 1";
    }

    private List<Ball> ballsPocketed = new List<Ball>();
    private bool nineBallPocketed;
    private List<Ball> railHits = new List<Ball>();
    private Ball firstHitBall;
    private bool waitForRail;

    public void BallPocketed(object ball)
    {
        Ball b = (Ball)ball;
        Debug.Log($"{b} pocketed£¡");

        if (b.GetNumber() == 9)
        {
            if (currentState == GameState9Ball.Breakshot)
            {
                nineBallPocketed = true;
                return;
            }
            if (currentState == GameState9Ball.PlayingPhase)
            {
                if (firstHitBall.GetNumber() == currentTargetNumber)
                {
                    player = playerOneTurn ? "Player 1" : "Player 2";
                    dialogMessageEvent.Raise($"{player} wins!");
                    currentState = GameState9Ball.GameOver;
                    return;
                }
                else
                {
                    opponent = playerOneTurn ? "Player 2" : "Player 1";
                    dialogMessageEvent.Raise($"{opponent} wins!");
                    currentState = GameState9Ball.GameOver;
                    return;
                }
            }
        }
        if (b.GetBallType() == BallType.Cue)
        {
            dialogMessageEvent.Raise("Foul! Cueball pocketed.");
            switch (currentState)
            {
                case GameState9Ball.Breakshot:
                    pendingState = GameState9Ball.BreakshotBallInHand;
                    break;
                case GameState9Ball.PlayingPhase:
                    pendingState = GameState9Ball.PlayingPhaseBallInHand;
                    break;
            
            }

            return;
        }
        ballsPocketed.Add(b);
    }

    public void RailHit(object ball)
    {
        Debug.Log($"{((Ball)ball)} hit the rail!");
        Ball b = (Ball)ball;
        if (currentState == GameState9Ball.Breakshot && b.GetBallType() == BallType.Cue) return;

        if (!waitForRail) return;
        if (!railHits.Contains(b))
        {
            railHits.Add(b);
        }
    }

    public void HitByCueball(object ball)
    {
        waitForRail = true;
        Debug.Log($"{((Ball)ball)} hit by cueball!");
        Ball b = (Ball)ball;
        if (currentState == GameState9Ball.Breakshot) return;
        if (firstHitBall != null) return;

        firstHitBall = b;

        if (firstHitBall.GetNumber() != currentTargetNumber)
        {
            dialogMessageEvent.Raise("Foul! Wrong ball hit.");
            pendingState = GameState9Ball.PlayingPhaseBallInHand;
            return;
        }

    }

    public void BallOffTheTable(object ball)
    {
        Ball b = (Ball)ball;
        dialogMessageEvent.Raise("Ball off the table£¡");

        if (b.GetNumber() == 9)
        {
            currentState = GameState9Ball.GameOver;
            return;
        }
        if (b.GetBallType()==BallType.Cue)
        {
            pendingState = currentState == GameState9Ball.Breakshot ?
                GameState9Ball.BreakshotBallInHand : GameState9Ball.PlayingPhaseBallInHand;
            return;
        }
        UpdateTargetNumber();
        pendingState = currentState == GameState9Ball.Breakshot ?
                GameState9Ball.BreakshotBallInHand : GameState9Ball.PlayingPhaseBallInHand;
    }
}
