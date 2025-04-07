using System.Collections;
using UnityEditor;
using UnityEngine;


public class Cuestick : MonoBehaviour
{
    [SerializeField] private Cueball cueball;
    [SerializeField] private Transform ghostBall;
    [SerializeField] private LineRenderer aimLine;
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private float trajectoryLineLength = .5f, maxPullDistance;
    [SerializeField] private float maxPower, maxForceForSwerve, minElevationForSwerve;
    [SerializeField] private float animationDuration;

    private Transform parent;
    private MeshRenderer rend;

    private float alphaPerFrame;

    private void Awake()
    {
        parent = transform.parent;
        rend = GetComponent<MeshRenderer>();

        alphaPerFrame = 1 / animationDuration;
    }

    public void Restart()
    {
        Hide();
        parent.rotation = Quaternion.identity;
    }

    public void Rotate(float angle)
    {
        Vector3 rotation = angle * Vector3.up;
        parent.Rotate(rotation, Space.World);

        DrawAimLine();
    }

    public void AimAtBall(Ball b)
    {
     Rotate(Vector3.SignedAngle(parent.right, b.transform.position - parent.position, Vector3.up));
    }

    public void Pull(float power)
    {
        transform.Translate(maxPullDistance * power * Vector3.left, Space.Self);
    }

    private Vector2 cueballSpin;

    public void UpdateSpin(object newSpin)
    {
        cueballSpin = (Vector2) newSpin * Ball.Radius / 2;
    }

    public void Strike(float power)
    {
        transform.Translate(maxPullDistance * power * Vector3.right, Space.Self);

        Vector3 rawForce = maxPower * power * parent.right;
        Vector3 pos = parent.TransformPoint(-Ball.Radius, cueballSpin.y, -cueballSpin.x);
        pos = cueball.GetSurfacePoint(pos);

        Vector3 referenceForce;

        if(cuestickElevation >= minElevationForSwerve && cueballSpin.x != 0)
        {
            referenceForce = rawForce.normalized * maxForceForSwerve;
        }
        else
        {
            referenceForce = rawForce;
        }

        Vector3 spinAdjustedForce = referenceForce + parent.position - pos;
        spinAdjustedForce = spinAdjustedForce.normalized * rawForce.magnitude;

        Vector3 elevationAdjustedForce = Quaternion.AngleAxis(-cuestickElevation, parent.forward) * spinAdjustedForce;

        cueball.Strike(elevationAdjustedForce, pos);

        Hide();
    }

    public void Hide()
    {
        aimLine.positionCount = trajectoryLine.positionCount = 0;
        ghostBall.position =Vector3.zero;
        StopAllCoroutines();
        StartCoroutine(HideCoroutine());
    }
    private IEnumerator HideCoroutine()
    {
        Color c = rend.material.color;
        while (c.a > 0)
        {            
            yield return null;
            c.a -= Time.deltaTime * alphaPerFrame;
            rend.material.color = c;
        }
        c.a = 0;
        rend.material.color = c;
    }

    public void Show()
    {
        parent.position = cueball.transform.position;
        StopAllCoroutines();
        StartCoroutine(ShowCoroutine());
        DrawAimLine();
    }

    private IEnumerator ShowCoroutine()
    {
        Color c = rend.material.color;
        while (c.a < 1)
        {           
            yield return null;
            c.a += Time.deltaTime * alphaPerFrame;
            rend.material.color = c;
        }
        c.a = 1;
        rend.material.color = c;
    }
    private void DrawAimLine()
    {
        Physics.SphereCast(parent.position, Ball.Radius, parent.right, out RaycastHit hit, 3, ~(1<<6) );
        Vector3 ghostBallPos = hit.point + hit.normal * Ball.Radius;
        Vector3 aimLineEndPos = ghostBallPos + (parent.position - ghostBallPos).normalized * Ball.Radius;
        Vector3 TrajectoryLineStart, trajectoryLineEnd;

        if (hit.collider.gameObject.layer == 9)//pockets
        {
            TrajectoryLineStart = trajectoryLineEnd = Vector3.zero;
        }
        else //balls and rails
        {
            Vector3 lineDirection;

            if (hit.collider.gameObject.layer == 10)//rails
            {
            Vector3 reflectionVector = Vector3.Reflect(hit.point - parent.position, hit.normal).normalized;
            TrajectoryLineStart = ghostBallPos + reflectionVector * Ball.Radius;
                lineDirection = reflectionVector;
            }
            else//balls
            {
                TrajectoryLineStart = hit.point;
                lineDirection = - hit.normal;
            }
            if(Physics.Raycast(TrajectoryLineStart,lineDirection,out hit,trajectoryLineLength, 1 << 9 | 1 << 10))
            {
                trajectoryLineEnd = hit.point;
            }
            else
            {
                trajectoryLineEnd = TrajectoryLineStart + lineDirection * trajectoryLineLength;
            }
        }

        aimLine.positionCount = 2;
        aimLine.SetPosition(0, parent.position);
        aimLine.SetPosition(1, aimLineEndPos);

        trajectoryLine.positionCount = 2;
        trajectoryLine.SetPosition(0, TrajectoryLineStart);
        trajectoryLine.SetPosition(1, trajectoryLineEnd);
        ghostBall.position = ghostBallPos;
    }


    private float cuestickElevation;

    public void UpdateElevation(object newElevation)
    {
        cuestickElevation = (float) newElevation;
        transform.localRotation = new Quaternion()
        {
            eulerAngles = Vector3.forward * -cuestickElevation
        };
    }
}
