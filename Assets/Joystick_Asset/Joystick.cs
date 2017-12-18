using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;

public class Joystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler {

    public Vector2 JoystickOutput { get; protected set; }
    public int ClickCount { get; protected set; }
    private Image joyImage;
    private Image bkgImage;
    private Color joyColor;
    private bool axisLock;
    private float deadzone;
    private Vector3 tempPoint;
    private Canvas joystickCanvas;
    private Camera mainCam;
    public static Joystick _Instance;

    public void Start()
    {
        JoystickOutput = new Vector2();
        tempPoint = new Vector3();
        ClickCount = 0;
        joyImage = GameObject.Find("Joystick_Left_Main").GetComponent<Image>();
        bkgImage = GetComponent<Image>();
        joyColor = joyImage.color;
        axisLock = false;
        deadzone = .05f;
        joystickCanvas = GetComponentInParent<Canvas>();
        mainCam = Camera.main;
        _Instance = this;
    }

    public virtual void OnDrag(PointerEventData ped)
    {
        Vector3 point;
        ClickCount = 0;
        if(RectTransformUtility.ScreenPointToWorldPointInRectangle(bkgImage.rectTransform,
            ped.position, ped.pressEventCamera, out point))
        {
            float scaleFactorY = Screen.width / Screen.height;
            float scaleFactorX = Screen.height / Screen.width;
            point = (mainCam.ScreenToViewportPoint(ped.position) - mainCam.ScreenToViewportPoint(bkgImage.rectTransform.position));
            point.x = (point.x / (mainCam.ScreenToViewportPoint(bkgImage.rectTransform.position + new Vector3(bkgImage.rectTransform.rect.xMax * scaleFactorX, 0, 0)).x));
            point.y = (point.y / (mainCam.ScreenToViewportPoint(bkgImage.rectTransform.position + new Vector3(bkgImage.rectTransform.rect.yMax * scaleFactorY, 0, 0)).y));

            if (point.x < 1 * deadzone && point.x > -1 * deadzone) point.x = 0;
            if (point.y < 1 * deadzone && point.y > -1 * deadzone) point.y = 0;
            if (axisLock && tempPoint.x != 0) point.y = 0;
            if (axisLock && tempPoint.y != 0) point.x = 0;
            tempPoint = point;
            JoystickOutput = (point.magnitude > 1) ? JoystickOutput = point.normalized : JoystickOutput = point;
            joyImage.rectTransform.anchoredPosition = new Vector2(
                (JoystickOutput.x * bkgImage.rectTransform.sizeDelta.x / 2),
                (JoystickOutput.y * bkgImage.rectTransform.sizeDelta.y / 2));
        }
    }

    public virtual void OnPointerDown(PointerEventData ped)
    {
        //ClickCount++;
        if (ClickCount > 1) axisLock = !axisLock;
            joyImage.color = (axisLock) ? Color.red : joyColor;
    }

    public virtual void OnPointerUp(PointerEventData ped)
    {
        JoystickOutput = new Vector2();
        joyImage.rectTransform.anchoredPosition = new Vector2();
    }
}
