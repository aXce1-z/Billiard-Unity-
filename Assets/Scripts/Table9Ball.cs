using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Table9Ball : MonoBehaviour
{
    [SerializeField] private GameEvent allBallsSleeping, ballOffTheTable, ballPocketed, ballHitByCueball, ballHitRail;
    [SerializeField] private Ball nineBall;
    [SerializeField] private Cueball cueball;
    [SerializeField] private Ball[] allBalls;
    private List<Ball> activeBalls = new List<Ball>();
    private Vector3 cueballPreStrikePos, eightballPreStrikePos;
    private Vector3 cueballDefaulPos, eightballDefaultPos;

    private void Awake()
    {
        Ball.table9Ball = this;
        cueball.IsBreaker = true;

        cueballDefaulPos = cueball.transform.position;
        eightballDefaultPos = nineBall.transform.position;
    }

    private void Start()
    {
        RackBalls();
    }

    public void UpdatePreStrikePos()
    {
        cueballPreStrikePos = cueball.transform.position;
        eightballPreStrikePos = nineBall.transform.position;
    }

    private void RackBalls()
    {
        Vector3 nineBallPos = nineBall.transform.position;
        float deltaZ = 0.03501f;
        float deltaX = 0.03501f * 2 * Mathf.Cos(30 * Mathf.Deg2Rad);

        // �����������еľŸ�λ��
        List<Vector3> positions = new List<Vector3>
    {
        // ��һ�� (1����)
        new Vector3(nineBallPos.x, nineBallPos.y, nineBallPos.z - 2 * deltaZ),
        
        // �ڶ���
        new Vector3(nineBallPos.x - deltaX, nineBallPos.y, nineBallPos.z - deltaZ),
        new Vector3(nineBallPos.x + deltaX, nineBallPos.y, nineBallPos.z - deltaZ),
        
        // ������
        new Vector3(nineBallPos.x - 2 * deltaX, nineBallPos.y, nineBallPos.z),
        nineBallPos, // 9����λ�ã����ģ�
        new Vector3(nineBallPos.x + 2 * deltaX, nineBallPos.y, nineBallPos.z),
        
        // ������
        new Vector3(nineBallPos.x - deltaX, nineBallPos.y, nineBallPos.z + deltaZ),
        new Vector3(nineBallPos.x + deltaX, nineBallPos.y, nineBallPos.z + deltaZ),
        
        // ������
        new Vector3(nineBallPos.x, nineBallPos.y, nineBallPos.z + 2 * deltaZ)
    };

        // ��ȡ�����򲢷���
        Ball oneBall = null;
        List<Ball> otherBalls = new List<Ball>();

        foreach (Ball b in allBalls) // ����allBalls��������1-9����
        {
            if (b.GetNumber() == 1) oneBall = b;
            else if (b.GetNumber() != 9) otherBalls.Add(b);
        }

        // �������������2-8�ţ�
        Shuffle(otherBalls);

        // �ڷ�������
        int positionIndex = 0;
        int otherIndex = 0;

        foreach (Vector3 pos in positions)
        {
            Ball currentBall;

            // ��������λ��
            if (positionIndex == 0) // 1����λ��
            {
                currentBall = oneBall;
            }
            else if (positionIndex == 4) // 9����λ��
            {
                currentBall = nineBall.GetComponent<Ball>();
            }
            else // ������λ��
            {
                currentBall = otherBalls[otherIndex++];
            }

            // ����λ�ú�״̬
            currentBall.transform.position = pos;
            currentBall.transform.rotation = Quaternion.identity;
            currentBall.ShowBall();

            positionIndex++;
        }
    }

    // ������������������б�
    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
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
        nineBall.EnablePhysics(false);
        nineBall.transform.position = pos;
        nineBall.EnablePhysics(true);
        nineBall.ShowBall();
    }

    public void ReRack()
    {
        cueball.EnablePhysics(false);
        nineBall.EnablePhysics(false);
        EnablePhysics(false);

        nineBall.transform.position = eightballDefaultPos;
        cueball.transform.rotation=nineBall.transform.rotation = Quaternion.identity;

        RackBalls();
        RespotCueball(cueballDefaulPos, true);
        nineBall.EnablePhysics(true);
        nineBall.ShowBall();
        EnablePhysics(true);
    }

    private void EnablePhysics(bool enable)
    {
        foreach (var b in allBalls)
        {
            b.EnablePhysics(enable);
        }
        
    }
}
