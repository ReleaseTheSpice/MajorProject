using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class Interactable : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] private Canvas canvas;
    private RectTransform rectTransform;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Fires when the player starts dragging the object
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Begin Drag");
    }
    
    // Fires when the player is dragging the object
    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Drag");
        // eventData.delta is the amount the mouse has moved since the last OnDrag event
        // divide by the canvas scale factor so the mouse movement is the same as screen movement
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
    
    // Fires when the player stops dragging the object
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("End Drag");
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("OnPointerDown");
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop");
    }
}


// base card script - hand in hand with the cardobj
//
// trap/secret cards?
//
//     board stack? with general effects?
//     card that makes other cards face down?
//
//     base card obj - contains list of "effect" ob