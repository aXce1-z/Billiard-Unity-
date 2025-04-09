using System.Collections;
using UnityEngine;

public enum BallType
{
    Cue,
    Eight,
    Nine,
    Solid,
    Stripe,
    Other
}
public class Ball : MonoBehaviour
{
    public static Table table;
    public static Table9Ball table9Ball;
    public static float Radius { get; protected set; }

    [SerializeField] private BallType type; public BallType GetBallType() { return type; }
    [SerializeField] private int number; public int GetNumber() { return number; }

    protected Rigidbody rb;
    private MeshRenderer rend;
    private float defaultDrag;
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        defaultDrag = rb.angularDrag;
        rend = GetComponentInChildren<MeshRenderer>();
    }

    private bool isSleeping;

    private void FixedUpdate()
    {
        if (rb.isKinematic) return;
        if (!rb.IsSleeping())
        {
            isSleeping = false;
            if(table!=null)table.ReportActive(this);
            if(table9Ball!=null)table9Ball.ReportActive(this); 

            float speed = rb.velocity.magnitude;
            if (speed < .01f)
            {
                rb.angularDrag += .03f;
            }
        }
        else
        {
            if (!isSleeping)
            {
                rb.angularDrag = defaultDrag;
                isSleeping = true;
                if (table != null) table.ReportInactive(this);
                if (table9Ball != null) table9Ball.ReportInactive(this);


            }
        }
    }

    private bool CheckCollision(Ball b)
    {
        if (Physics.SphereCast(rb.position, Radius, rb.velocity, out RaycastHit hit, 5 * Radius, ~(1 << 6)))
        {
            if (hit.collider.GetComponent<Ball>() == b)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    protected void HandleBallToBallCollision(Ball b)
    {
        if (CheckCollision(b)) return;
            if (this.GetBallType() == BallType.Cue || b.GetBallType() == BallType.Cue)
            {
                Ball ob = this.GetBallType() == BallType.Cue ? b : this;
                Cueball cb = this.GetBallType() == BallType.Cue ? (Cueball)this : (Cueball)b;
                if (table != null) table.ReportHitByCueball(ob);
                if (table9Ball != null) table9Ball.ReportHitByCueball(ob);
                


            if (cb.IsBreaker)
                {
                    StartCoroutine(cb.HandleBreakShot(ob.rb));
                    cb.IsBreaker = false;
                }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject obj = collision.gameObject;
        Ball b =obj.GetComponent<Ball>();

        if (b != null) 
        {
            HandleBallToBallCollision(b);
        }
        if (obj.layer == 8)//cueball
        {
            obj.GetComponent<Cueball>().AddFrozen(this);
        }

        if(obj.layer == 9)//pockets
        {
            EnablePhysics(false);          
            StartCoroutine(HideBall());
            if (table != null) table.ReportPocketed(this);
            if (table9Ball != null)
                table9Ball.ReportPocketed(this);
            

        }

        if (obj.layer == 10)//rails
        {
            if (table != null)
                table.ReportRailHit(this);
            if (table9Ball != null)
                table9Ball.ReportRailHit(this);
           
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        GameObject obj = other.gameObject;
        if (obj.layer == 11)//table bounds
        {

            if (table != null)
                table.ReportOffTheTable(this);
            if (table9Ball != null)
                table9Ball.ReportOffTheTable(this);
            
        }
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
    public virtual void EnablePhysics(bool enable)
    {
        if (enable)
        {
            if (type == BallType.Cue || type == BallType.Eight) 
            { 
                Respot(transform.position);
            }
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
