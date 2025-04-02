using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameEvent mouseClickNotOverUI;
    [SerializeField] private Cuestick cuestick;
    [SerializeField] private Cueball cueball;
    [SerializeField] private CueballMover cueballMover;
    [SerializeField] private Transform tableBedTopLeft, tableBedBottomRight, kitchenBottomRight;

    private Vector3 prevMPos;
    private EventSystem es;
    private Camera cam;
    private GraphicRaycaster gr;

    private void Awake()
    {
        es=EventSystem.current;
        cam= Camera.main;
        gr = GetComponent<GraphicRaycaster>();
    }

    public void EnableUI()
    {
        gr.enabled = uiEnabled = true; 
    }

    public void DisableUI()
    {
        gr.enabled = uiEnabled = false;
    }

    private bool rotateCuestick, moveCueball, clickAboveBall, clickRightOfBall, aimAtBall;
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandlePointerDown(Input.mousePosition);
        }


        if (Input.GetMouseButtonUp(0))
        {
            HandlePointUp();
        }

        if (Input.GetMouseButton(0))
        {
            if (moveCueball)
            {
                MoveCueball();
            }
            if (rotateCuestick) 
            { 
                RotateCuestick();
            }

            prevMPos = Input.mousePosition;
        }
    }

    private Ball clickedBall;
    private bool uiEnabled;
    private void HandlePointerDown(Vector3 pointerPos)
    {
        if (!uiEnabled) return;
        if (es.IsPointerOverGameObject()) return;

        mouseClickNotOverUI.Raise();

        prevMPos = pointerPos;

        if (Physics.Raycast(cam.ScreenPointToRay(pointerPos), out RaycastHit hit))
        {
            moveCueball = hit.collider.GetComponent<CueballMover>() != null;
           clickedBall = hit.collider.GetComponent<Ball>();
        }

        if (moveCueball)
        {
            cuestick.Hide();
            cueball.EnablePhysics(false);
            return;
        }

        if (clickedBall != null)
        {
            cuestick.AimAtBall(clickedBall);
            clickedBall = null;
        }

        rotateCuestick = true;

        Vector3 ballScreenPos = cam.WorldToScreenPoint(cueball.transform.position);
        clickAboveBall = pointerPos.y > ballScreenPos.y;
        clickRightOfBall = pointerPos.x > ballScreenPos.x;
    }

    private void HandlePointUp()
    {
        rotateCuestick = false;

        if (moveCueball)
        {
            cueball.EnablePhysics(true);
            cuestick.Show();
            moveCueball = false;
        }
    }

    private void RotateCuestick()
    {
        Vector3 diff = Input.mousePosition - prevMPos;

        if (diff == Vector3.zero) return;

        float angle = 1;

        if (Mathf.Abs(diff.y) > Mathf.Abs(diff.x))//vertical swipe
        {
            angle = diff.y;
            if (clickRightOfBall) angle *= -1;
        }
        else//horizontal swipe
        {
            angle = diff.x;
            if (!clickAboveBall) angle *= -1;
        }

        cuestick.Rotate(angle * Mathf.Abs(angle) * Time.deltaTime * .1f);
    }

    private void MoveCueball()
    {
        Vector3 mPosWorld = Input.mousePosition;
        Vector3 prevMPosWorld = prevMPos;
        mPosWorld.z = prevMPosWorld.z = cam.transform.position.y - cueballMover.transform.position.y;

        Vector3 worldSpaceDiff = cam.ScreenToWorldPoint(mPosWorld) - cam.ScreenToWorldPoint(prevMPosWorld);

        Vector3 newPos = cueball.transform.position + worldSpaceDiff;

        float minX, maxX, minZ, maxZ;

        minX = tableBedTopLeft.position.x;
        maxZ= tableBedTopLeft.position.z;
        minZ = tableBedBottomRight.position.z;
        maxX= cueball.IsBreaker ? kitchenBottomRight.position.x : tableBedBottomRight.position.x;

        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.z = Mathf.Clamp(newPos.z, minZ, maxZ);

        cueball.Move(newPos);
    }
}
