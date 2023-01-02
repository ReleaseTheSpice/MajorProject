using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class Interactable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // Fires when the player starts dragging the object
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Begin Drag");
    }
    
    // Fires when the player is dragging the object
    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Drag");
        // Move the object to the mouse position
        this.transform.position = eventData.position;
    }
    
    // Fires when the player stops dragging the object
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("End Drag");
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