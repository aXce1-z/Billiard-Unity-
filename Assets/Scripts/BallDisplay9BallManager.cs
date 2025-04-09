using System.Collections.Generic;
using UnityEngine;

public class BallDisplay9BallManager : MonoBehaviour
{
    [SerializeField] private BallDisplay targetBallDisplay; // Ŀ������ʾUI
    [SerializeField] private Sprite[] ballIcons;           // 1-9����ͼ�꣨����0-8��
    [SerializeField] private GameLogic9Ball gameLogic;      // ֱ��������Ϸ�߼�

    [SerializeField] private Color DisabledColor;

    private void Awake()
    {
        BallDisplay.disabledColor = DisabledColor;

    }


    public void DisplayBalls()
    {

        targetBallDisplay.Show(ballIcons[gameLogic.currentTargetNumber-1]);
        
    }

    // ���������ʱ���ã���GameLogic�¼�������

    public void Restart()
    {
        targetBallDisplay.Show(ballIcons[0]); // ����ʱˢ����ʾ
    }
}
