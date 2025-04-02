using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUp : MonoBehaviour
{
    [SerializeField] private float animationDuration, autoHide;

    private CanvasGroup cg;
    private WaitForSeconds autoHideTimer;

    private static List<PopUp> popups = new List<PopUp>();

    private float alphaPerFrame;

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();

        alphaPerFrame = 1 / animationDuration;
        autoHideTimer = autoHide > 0 ? new WaitForSeconds(autoHide) : null;

        popups.Add(this);
        Hide();
    }

    public void Show()
    {
        foreach (var p in popups)
        {
            p.Hide();
        }
        StopAllCoroutines();
        StartCoroutine(ShowCoroutine());

    }


    private IEnumerator ShowCoroutine()
    {
        while(cg.alpha < 1)
        {
            cg.alpha += Time.deltaTime * alphaPerFrame;
            yield return null;
        }       
        cg.interactable = cg.blocksRaycasts = true;
        if (autoHideTimer != null)
        {
            yield return autoHideTimer;
            StartCoroutine(HideCoroutine());
        }
    }
    public void Hide()
    {
        StopAllCoroutines();
        StartCoroutine(HideCoroutine());
    }

    private IEnumerator HideCoroutine() 
    {
        while (cg.alpha > 0)
        {
            cg.alpha -= Time.deltaTime * alphaPerFrame;
            yield return null;
        }
        cg.interactable = cg.blocksRaycasts = false;
    }
}
