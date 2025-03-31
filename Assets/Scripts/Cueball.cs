using UnityEngine;

public class Cueball : Ball
{
    public bool IsBreaker { get; set; }
    
    private SphereCollider col;

    protected override void Awake()
    {
        base.Awake();
        col = GetComponent<SphereCollider>();
        Radius = col.radius;
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
    }

    public void Move(Vector3 newPos)
    {
        transform.position = newPos;
    }

    public Vector3 GetSurfacePoint(Vector3 pos) 
    {
        return col.ClosestPoint(pos);
    }
}
