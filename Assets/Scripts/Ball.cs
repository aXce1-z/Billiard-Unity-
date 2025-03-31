using System.Collections;
using UnityEngine;

public enum BallType
{
    Cue,
    Eight,
    Solid,
    Stripe
}
public class Ball : MonoBehaviour
{
    public static Table table;
    public static float Radius { get; protected set; }

    [SerializeField] private BallType type; public BallType GetBallType() { return type; }
    [SerializeField] private int number; public int GetNumber() { return number; }

    protected Rigidbody rb;
    private MeshRenderer rend;
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponentInChildren<MeshRenderer>();
    }

    private bool isSleeping;
    private void FixedUpdate()
    {
        if (rb.isKinematic) return;
        if (!rb.IsSleeping())
        {
            isSleeping = false;
            table.ReportActive(this);
        }
        else
        {
            if (!isSleeping)
            {
                isSleeping = true;
                table.ReportInactive(this);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject obj = collision.gameObject;

        if(collision.gameObject.layer == 8)//cueball
        {
            table.ReportHitByCueball(this);

            Cueball cb = collision.gameObject.GetComponent<Cueball>();
            if (cb.IsBreaker)
            {
                StartCoroutine(HandleBreakShot(cb));
                cb.IsBreaker = false;
            }
        }

        if(obj.layer == 9)//pockets
        {
            EnablePhysics(false);          
            StartCoroutine(HideBall());
            table.ReportPocketed(this);

        }

        if (obj.layer == 10)//rails
        {
            table.ReportRailHit(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject obj = other.gameObject;
        if (obj.layer == 11)//table bounds
        {
            table.ReportOffTheTable(this);
        }
    }
    private IEnumerator HandleBreakShot(Ball b)
    {
        yield return null;
        float totalSpeed = (rb.velocity.magnitude + b.rb.velocity.magnitude) * (1 + Random.Range(0, .1f));
        rb.velocity = rb.velocity.normalized * totalSpeed * .85f;
        b.rb.velocity = b.rb.velocity.normalized * totalSpeed * .15f;
    }

    private IEnumerator HideBall()
    {
        Color c = rend.material.color; ;
        while (c.a > 0)
        {
            yield return null ;
            c.a -= Time.deltaTime;
            rend.material.color = c;
        }
        c.a = 0;
        rend.material.color = c;
    }

    public void ShowBall()
    {
        StopAllCoroutines();

        Color c = rend.material.color;
        c.a = 1;
        rend.material.color = c;
    }
    public void EnablePhysics(bool enable)
    {
        if (enable)
        {
            Respot(transform.position);
        }

        rb.isKinematic = !enable;
        rb.detectCollisions = enable;
    }

    private void Respot(Vector3 pos)
    {
        int layerMask = ~(1 << 6);//all layers except the layer with index 6 (table bed)
        float offsetPerRound = Radius / 3;

        if (!Physics.CheckSphere(pos, Radius, layerMask)) return;

        //find respot position
        int round = 1;

        while (true)
        {
            int steps = 4 * round;
            Vector3 offset = Vector3.forward * offsetPerRound * round++;

            for (int i = 0; i < steps; i++)
            {
                Vector3 checkPos = Quaternion.AngleAxis(360 / steps * i, Vector3.up) * offset + pos;
                Debug.DrawLine(pos, checkPos, Color.blue, 5);
                if (!Physics.CheckSphere(checkPos, Radius, layerMask))
                {
                    transform.position = checkPos;
                    return;
                }
            }

        }
    }
}
