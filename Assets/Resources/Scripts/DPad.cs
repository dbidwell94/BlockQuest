using UnityEngine;
using UnityEngine.EventSystems;

public class DPad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {

    public static DPad _Instance;
    public Vector3 DPadOutput;
    public GameObject up, down, left, right;
    private bool downPressed = false, upPressed = false, leftPressed = false, rightPressed = false;
    private bool pressing = false;
    private Rect upRect, downRect, leftRect, rightRect;

    void Awake()
    {
        _Instance = this;
    }

	// Use this for initialization
	void Start () {
        _Instance = this;
        DPadOutput = new Vector3();
	}

	// Update is called once per frame
	void Update () {
        if (downPressed) DPadOutput = new Vector3(0, 0, -1);
        if (upPressed) DPadOutput = new Vector3(0, 0, 1);
        if (rightPressed) DPadOutput = new Vector3(1, 0, 0);
        if (leftPressed) DPadOutput = new Vector3(-1, 0, 0);
        else if(!downPressed && !upPressed && !rightPressed && !leftPressed) DPadOutput = new Vector3();
	}

    public void OnPointerDown(PointerEventData ped)
    {
        pressing = true;
        if (RectTransformUtility.RectangleContainsScreenPoint(right.GetComponent<RectTransform>(), ped.position))
        {
            rightPressed = true;
        }
        if (RectTransformUtility.RectangleContainsScreenPoint(left.GetComponent<RectTransform>(), ped.position))
        {
            leftPressed = true;
        }
        if (RectTransformUtility.RectangleContainsScreenPoint(up.GetComponent<RectTransform>(), ped.position))
        {
            upPressed = true;
        }
        if (RectTransformUtility.RectangleContainsScreenPoint(down.GetComponent<RectTransform>(), ped.position))
        {
            downPressed = true;
        }
    }

    public void OnPointerUp(PointerEventData ped)
    {
        pressing = false;
        upPressed = false;
        leftPressed = false;
        rightPressed = false;
        downPressed = false;
    }

    public void OnDrag(PointerEventData ped)
    {
        upPressed = false;
        leftPressed = false;
        rightPressed = false;
        downPressed = false;
        if (RectTransformUtility.RectangleContainsScreenPoint(right.GetComponent<RectTransform>(), ped.position))
        {
            rightPressed = true;
        }
        if (RectTransformUtility.RectangleContainsScreenPoint(left.GetComponent<RectTransform>(), ped.position))
        {
            leftPressed = true;
        }
        if (RectTransformUtility.RectangleContainsScreenPoint(up.GetComponent<RectTransform>(), ped.position))
        {
            upPressed = true;
        }
        if (RectTransformUtility.RectangleContainsScreenPoint(down.GetComponent<RectTransform>(), ped.position))
        {
            downPressed = true;
        }
    }
}
