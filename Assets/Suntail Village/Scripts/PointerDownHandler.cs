using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PointerDownHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Action<PointerEventData> PointerDownHander { get; set; }
    public Action<PointerEventData> PointerUpHander { get; set; }


    public void OnPointerDown(PointerEventData eventData)
    {
        PointerDownHander?.Invoke(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        PointerUpHander?.Invoke(eventData);
    }
}