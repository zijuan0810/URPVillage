using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FloatingJoystick : Joystick
{
    protected override void Start()
    {
        base.Start();
        m_Background.gameObject.SetActive(false);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        m_Background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
        m_Background.gameObject.SetActive(true);
        base.OnPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        m_Background.gameObject.SetActive(false);
        base.OnPointerUp(eventData);
    }
}