using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cueball : Ball
{
    [SerializeField] private Transform mover;
    public bool IsBreaker { get; set; }
    
    private SphereCollider col;
    private List<Ball> frozenBalls = new List<Ball>();

    protected override void Awake()
    {
        base.Awake();
        col = GetComponent<SphereCollider>();
        Radius = col.radius;
    }

    public void AddFrozen(Ball b)
    {
        if (frozenBalls.Contains(b)) return;
        frozenBalls.Add(b);
    }

    private void OnCollisionExit(Collision collision)
    {
        Ball b=collision.gameObject.GetComponent<Ball>();
        if (b)
        {
            frozenBalls.Remove(b);
        }
    }

    public IEnumerator HandleBreakShot(Rigidbody obRigidbody)
    {
        yield return null;
        float totalSpeed = (rb.velocity.magnitude + obRigidbody.velocity.magnitude) * (1 + Random.Range(0, .1f));
        rb.velocity = rb.velocity.normalized * totalSpeed * .15f;
        obRigidbody.velocity = obRigidbody.velocity.normalized * totalSpeed * .85f;
    }

    private Vector3 prevPos;

    private void Update()
    {
        Debug.DrawLine(prevPos, transform.position, Color.black, 5);
        prevPos = transform.position;
    }
    public void Strike(Vector3 force, Vector3 position)
    {
        rb.AddForceAtPosition(force, position, ForceMode.Impulse);

        StartCoroutine(CheckFrozenBalls());
    }

    private IEnumerator CheckFrozenBalls()
    {
        yield return null;
        foreach (var b in frozenBalls)
        {
            HandleBallToBallCollision(b);
        }
    }

    public override void EnablePhysics(bool enable)
    {
        base.EnablePhysics(enable);
        if (enable)
        {
            Move(transform.position);
        }
    }
    public void Move(Vector3 newPos)
    {
        mover.position = transform.position = newPos;
    }

    public Vector3 GetSurfacePoint(Vector3 pos) 
    {
        return col.ClosestPoint(pos);
    }
}
