using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Table : MonoBehaviour
{
    [SerializeField] private GameEvent allBallsSleeping;
    [SerializeField] private Ball eightBall;
    [SerializeField] private Cueball cueball;
    [SerializeField] private Ball[] stripes, solids;
    private List<Ball> activeBalls = new List<Ball>();

    private void Awake()
    {
        Ball.table = this;
        RackBalls();
        cueball.IsBreaker = true;
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

        for(int i = 0; i < 5; i++)
        {
            for(int j = i; j < 5; j++)
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
        if (activeBalls.Count== 0)
        {
            allBallsSleeping.Raise();
        }
    }
}
