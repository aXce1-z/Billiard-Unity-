using System.Collections.Generic;
using UnityEngine;

public class BallDisplayManager : MonoBehaviour
{
    [SerializeField] private BallDisplay[] p1Displays, p2Displays;
    [SerializeField] private Sprite[] ballIcons;

    [SerializeField] private Color DisabledColor;

    private List<BallDisplay> allDisplays = new List<BallDisplay>();
    private List<int> pocketedBalls = new List<int>();
    private void Awake()
    {
        BallDisplay.disabledColor = DisabledColor;
    }

    public void Restart()
    {
        if (allDisplays.Count == 0) return;
        Clear(1, 7);
        Clear(9, 15);
        allDisplays.Clear();
    }
    public void DisplayBalls(object playerOneBallType)
    {
        BallDisplay[] solids, stripes;

        if((BallType)playerOneBallType == BallType.Solid)
        {
            solids=p1Displays;
            stripes = p2Displays;

        }
        else
        {
            solids = p2Displays;
            stripes = p1Displays;
        }

        for(int i = 0; i < 7; i++)
        {
            solids[i].Show(ballIcons[i]);
            stripes[i].Show(ballIcons[i + 8]);
        }
        allDisplays.Add(null);
        allDisplays.AddRange(solids);
        allDisplays.Add(null);//EightBall
        allDisplays.AddRange(stripes);
        foreach(var n in pocketedBalls)
        {
            allDisplays[n].Hide();
        }
    }

    public void UpdateDisplay(object ball)
    {
        Ball b = (Ball)ball;
        if (b.GetBallType() == BallType.Cue || b.GetBallType() == BallType.Eight) return;
        pocketedBalls.Add(b.GetNumber());


        if (allDisplays.Count == 0) return;

        allDisplays[b.GetNumber()].Hide();

        if (pocketedBalls.Count >= 7)
        {
            if (CheckPocketed(1,7))
            {
                Clear(1, 7);
                allDisplays[4].Show(ballIcons[7]);
            }
           
            if (CheckPocketed(9,15))
            {
                Clear(9, 15);
                allDisplays[12].Show(ballIcons[7]);
            }
        }

    }
    private bool CheckPocketed(int from, int to)
    {
        for(int i = from; i <= to; i++)
        {
            if (!pocketedBalls.Contains(i)) return false;
        }

        return true;
    }

    private void Clear(int from, int to)
    {
        for(int i=from; i <= to; i++)
        {
            allDisplays[i].Clear();
        }
    }
}
