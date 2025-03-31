using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TurnIndicator : MonoBehaviour
{
    [SerializeField] private Image hand, p1NamePlate, p2NamePlate;
    [SerializeField] private Sprite[] p1Sprites, p2Sprites;
    [SerializeField] private Sprite p1NamePlateIdle, p1NamePlateActive, p2NamePlateIdle, p2NamePlateActive;

    public void PassTurn(object playerOneTurn)
    {
        StopAllCoroutines();

        if ((bool)playerOneTurn)
        {
            StartCoroutine(P1());
        }
        else
        {
            StartCoroutine(P2());
        }
    }

    private IEnumerator P1()
    {
        p2NamePlate.sprite = p2NamePlateIdle;
        foreach (Sprite s in p1Sprites)
        {
            hand.sprite = s;
            yield return new WaitForSeconds(.01f);
        }
        p1NamePlate.sprite = p1NamePlateActive;
    }

    private IEnumerator P2()
    {
        p1NamePlate.sprite = p1NamePlateIdle;
        foreach (Sprite s in p2Sprites)
        {
            hand.sprite = s;
            yield return new WaitForSeconds(.01f);
        }
        p2NamePlate.sprite = p2NamePlateActive;
    }
}
