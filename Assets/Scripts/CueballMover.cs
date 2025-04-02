using System.Collections;
using UnityEngine;

public class CueballMover : MonoBehaviour
{
    [SerializeField] Cueball cueball;
    [SerializeField] private float animationDuration;   
    private float alphaPerFrame;
    private SpriteRenderer rend;
    private Collider col;

    private void Awake()
    {
        rend = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider>();
        alphaPerFrame = 1 / animationDuration;
    }
    public void Show()
    {
        StartCoroutine(ShowCoroutine());
    }
    
    public void Hide()
    {
        StartCoroutine(HideCoroutine());
    }

    private IEnumerator ShowCoroutine()
    {
        transform.parent.position = cueball.transform.position;
        col.enabled = true;
        Color c = rend.color;
        while (c.a < 1)
        {
            yield return null;
            c.a += Time.deltaTime * alphaPerFrame;
            rend.color = c;
        }
        c.a = 1;
        rend.color = c;
    }

    private IEnumerator HideCoroutine()
    {
        col.enabled = false;
        Color c = rend.color;
        while (c.a > 0) 
        { 
            yield return null;
            c.a -= Time.deltaTime * alphaPerFrame;
            rend.color = c;
        }
        c.a = 0;
        rend.color = c;
    }
}
