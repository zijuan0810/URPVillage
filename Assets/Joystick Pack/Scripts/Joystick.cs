using UnityEngine;
using UnityEngine.EventSystems;

public enum AxisOptions
{
    Both,
    Horizontal,
    Vertical
}

public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public float Horizontal => (m_SnapX) ? SnapFloat(m_InputPoint.x, AxisOptions.Horizontal) : m_InputPoint.x;

    public float Vertical => (m_SnapY) ? SnapFloat(m_InputPoint.y, AxisOptions.Vertical) : m_InputPoint.y;

    public Vector2 Direction => new Vector2(Horizontal, Vertical);

    public float HandleRange
    {
        get => m_HandleRange;
        set => m_HandleRange = Mathf.Abs(value);
    }

    public float DeadZone
    {
        get => m_DeadZone;
        set => m_DeadZone = Mathf.Abs(value);
    }

    public AxisOptions AxisOptions
    {
        get => m_AxisOptions;
        set => m_AxisOptions = value;
    }

    public bool SnapX
    {
        get => m_SnapX;
        set => m_SnapX = value;
    }

    public bool SnapY
    {
        get => m_SnapY;
        set => m_SnapY = value;
    }

    [SerializeField]
    private float m_HandleRange = 1;

    [SerializeField]
    private float m_DeadZone;

    [SerializeField]
    private AxisOptions m_AxisOptions = AxisOptions.Both;

    [SerializeField]
    private bool m_SnapX;

    [SerializeField]
    private bool m_SnapY;

    [SerializeField]
    protected RectTransform m_Background;

    [SerializeField]
    private RectTransform m_Handle;


    private Canvas m_Canvas;
    private Camera m_Camera;
    private RectTransform m_BaseRect;
    private Vector2 m_InputPoint = Vector2.zero;

    protected virtual void Start()
    {
        HandleRange = m_HandleRange;
        DeadZone = m_DeadZone;
        
        m_BaseRect = GetComponent<RectTransform>();
        m_Canvas = GetComponentInParent<Canvas>();
        if (m_Canvas == null)
            Debug.LogError("The Joystick is not placed inside a canvas");

        Vector2 center = new Vector2(0.5f, 0.5f);
        m_Background.pivot = center;
        m_Handle.anchorMin = center;
        m_Handle.anchorMax = center;
        m_Handle.pivot = center;
        m_Handle.anchoredPosition = Vector2.zero;
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        m_Camera = null;
        if (m_Canvas.renderMode == RenderMode.ScreenSpaceCamera)
            m_Camera = m_Canvas.worldCamera;

        Vector2 position = RectTransformUtility.WorldToScreenPoint(m_Camera, m_Background.position);
        Vector2 radius = m_Background.sizeDelta / 2f;
        m_InputPoint = (eventData.position - position) / (radius * m_Canvas.scaleFactor);
        if (m_AxisOptions == AxisOptions.Horizontal)
            m_InputPoint.y = 0f;
        else if (m_AxisOptions == AxisOptions.Vertical)
            m_InputPoint.x = 0f;
        HandleInput(m_InputPoint.magnitude, m_InputPoint.normalized, radius, m_Camera);
        m_Handle.anchoredPosition = m_InputPoint * radius * m_HandleRange;
    }

    protected virtual void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
    {
        if (magnitude > m_DeadZone)
        {
            if (magnitude > 1)
                m_InputPoint = normalised;
        }
        else
            m_InputPoint = Vector2.zero;
    }

    private float SnapFloat(float value, AxisOptions snapAxis)
    {
        if (value == 0)
            return value;

        if (m_AxisOptions == AxisOptions.Both)
        {
            float angle = Vector2.Angle(m_InputPoint, Vector2.up);
            if (snapAxis == AxisOptions.Horizontal)
            {
                if (angle < 22.5f || angle > 157.5f)
                    return 0;
                else
                    return (value > 0) ? 1 : -1;
            }
            else if (snapAxis == AxisOptions.Vertical)
            {
                if (angle > 67.5f && angle < 112.5f)
                    return 0;
                else
                    return (value > 0) ? 1 : -1;
            }

            return value;
        }
        else
        {
            if (value > 0)
                return 1;
            if (value < 0)
                return -1;
        }

        return 0;
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        m_InputPoint = Vector2.zero;
        m_Handle.anchoredPosition = Vector2.zero;
    }

    protected Vector2 ScreenPointToAnchoredPosition(Vector2 screenPosition)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_BaseRect, screenPosition, m_Camera, out var localPoint))
        {
            Vector2 pivotOffset = m_BaseRect.pivot * m_BaseRect.sizeDelta;
            return localPoint - (m_Background.anchorMax * m_BaseRect.sizeDelta) + pivotOffset;
        }

        return Vector2.zero;
    }
}