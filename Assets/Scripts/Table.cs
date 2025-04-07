using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Table : MonoBehaviour
{
    [SerializeField] private GameEvent allBallsSleeping, ballOffTheTable, ballPocketed, ballHitByCueball, ballHitRail;
    [SerializeField] private Ball eightBall;
    [SerializeField] private Cueball cueball;
    [SerializeField] private Ball[] stripes, solids;
    private List<Ball> activeBalls = new List<Ball>();
    private Vector3 cueballPreStrikePos, eightballPreStrikePos;
    private Vector3 cueballDefaulPos, eightballDefaultPos;

    private void Awake()
    {
        Ball.table = this;
        cueball.IsBreaker = true;

        cueballDefaulPos = cueball.transform.position;
        eightballDefaultPos = eightBall.transform.position;
    }

    private void Start()
    {
        RackBalls();
    }

    public void UpdatePreStrikePos()
    {
        cueballPreStrikePos = cueball.transform.position;
        eightballPreStrikePos = eightBall.transform.position;
    }

    private void RackBalls()
    {
        Vector3 eightBallPos = eightBall.transform.position;
        float deltaZ = .03501f;
        float deltaX = .03501f * 2 * Mathf.Cos(30 * Mathf.Deg2Rad);
        int ballType = Random.Range(0, 2);
        int ind = 0;

        List<Ball> solidBalls = new List<Ball>(solids);
        List<Ball> stripeBalls = new List<Ball>(stripes);
        List<Ball> currentBalls;

        for (int i = 0; i < 5; i++)
        {
            for (int j = i; j < 5; j++)
            {
                if (i == 1 && j == 2) continue;

                currentBalls = ind++ % 2 == ballType ? solidBalls : stripeBalls;
                int randomIndex = Random.Range(0, currentBalls.Count);
                Ball b = currentBalls[randomIndex];
                currentBalls.RemoveAt(randomIndex);

                Vector3 pos = eightBallPos;
                pos.x = eightBallPos.x + deltaX * (j - 2);
                pos.z = eightBallPos.z + deltaZ * (2 * i - j);

                b.transform.position = pos;
                b.transform.rotation = Quaternion.identity;
                b.ShowBall();
            }
        }
    }

    public void ReportActive(Ball b)
    {
        if (!activeBalls.Contains(b))
        {
            activeBalls.Add(b);
        }
    }

    public void ReportInactive(Ball b)
    {
        activeBalls.Remove(b);
        if (activeBalls.Count == 0)
        {
            allBallsSleeping.Raise();
        }
    }

    public void ReportOffTheTable(Ball b)
    {
        ballOffTheTable.Raise(b);
        ReportInactive(b);
    }

    public void ReportPocketed(Ball b)
    {
        ballPocketed.Raise(b);
        ReportInactive(b);
    }

    public void ReportHitByCueball(Ball b)
    {
        ballHitByCueball.Raise(b);
    }

    public void ReportRailHit(Ball b)
    {
        ballHitRail.Raise(b);
    }

    public void RespotCueball(object respotBehindHeadstring)
    {
        RespotCueball(cueballPreStrikePos, (bool)respotBehindHeadstring);
    }

    private void RespotCueball(Vector3 pos, bool isBreaker)
    {
        cueball.IsBreaker = isBreaker;
        cueball.EnablePhysics(false);
        cueball.transform.position = pos;
        cueball.EnablePhysics(true);
        cueball.ShowBall();
    }
    public void RespotEightball()
    {
        RespotEightball(eightballPreStrikePos);
    }

    private void RespotEightball(Vector3 pos)
    {
        eightBall.EnablePhysics(false);
        eightBall.transform.position = pos;
        eightBall.EnablePhysics(true);
        eightBall.ShowBall();
    }

    public void ReRack()
    {
        cueball.EnablePhysics(false);
        eightBall.EnablePhysics(false);
        EnablePhysics(false);

        eightBall.transform.position = eightballDefaultPos;
        cueball.transform.rotation=eightBall.transform.rotation = Quaternion.identity;

        RackBalls();
        RespotCueball(cueballDefaulPos, true);
        eightBall.EnablePhysics(true);
        eightBall.ShowBall();
        EnablePhysics(true);
    }

    private void EnablePhysics(bool enable)
    {
        foreach (var b in stripes)
        {
            b.EnablePhysics(enable);
        }
        foreach (var b in solids)
        {
            b.EnablePhysics(enable);
        }
    }
}
