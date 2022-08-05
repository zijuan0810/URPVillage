using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FloatingJoystick : Joystick
{
    [SerializeField]
    private bool m_IsFixed;

    protected override void Start()
    {
        base.Start();
        m_Background.gameObject.SetActive(m_IsFixed);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (!m_IsFixed)
        {
            m_Background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
            m_Background.gameObject.SetActive(true);
        }

        base.OnPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (!m_IsFixed)
            m_Background.gameObject.SetActive(false);
        
        base.OnPointerUp(eventData);
    }
}