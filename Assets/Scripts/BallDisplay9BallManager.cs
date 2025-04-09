using System.Collections.Generic;
using UnityEngine;

public class BallDisplay9BallManager : MonoBehaviour
{
    [SerializeField] private BallDisplay targetBallDisplay; // 目标球显示UI
    [SerializeField] private Sprite[] ballIcons;           // 1-9号球图标（索引0-8）
    [SerializeField] private GameLogic9Ball gameLogic;      // 直接引用游戏逻辑

    [SerializeField] private Color DisabledColor;

    private void Awake()
    {
        BallDisplay.disabledColor = DisabledColor;

    }


    public void DisplayBalls()
    {

        targetBallDisplay.Show(ballIcons[gameLogic.currentTargetNumber-1]);
        
    }

    // 当有球入袋时调用（从GameLogic事件触发）

    public void Restart()
    {
        targetBallDisplay.Show(ballIcons[0]); // 重置时刷新显示
    }
}
