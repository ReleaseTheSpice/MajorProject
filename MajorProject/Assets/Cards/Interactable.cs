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
    public Vector3 homePosition;
    // ^ Maybe this shouldnt be serializable/public?
    
    public bool draggable = false;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        homePosition = rectTransform.anchoredPosition3D;
    }
    
    public void ReturnHome()
    {
        rectTransform.anchoredPosition3D = homePosition;
    }

    #region Input Callbacks
    
    // Fires when the player starts dragging the object
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Debug.Log("Begin Drag");
    }
    
    // Fires when the player is dragging the object
    public void OnDrag(PointerEventData eventData)
    {
        if (draggable)
        {
            // Debug.Log("Drag");
            
            // eventData.delta is the amount the mouse has moved since the last OnDrag event
            // divide by the canvas scale factor so the mouse distance is the same as screen distance
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }
    
    // Fires when the player stops dragging the object
    public void OnEndDrag(PointerEventData eventData)
    {
        // Debug.Log("End Drag");
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        // Debug.Log("OnPointerDown");
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        // Debug.Log("OnDrop");
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(eventData.position), Vector2.zero);
        if (hit.collider != null && hit.collider.gameObject.CompareTag("DropZone"))
        {
            // Debug.Log(gameObject.name + "Hit DropZone");
            // Remove the card from the hand
            gameObject.GetComponentInParent<Hand>().RemoveCard(gameObject);
            
            // Add me to the stack
            Stack s = hit.collider.gameObject.GetComponent<Stack>();
            Transform t = s.GetCanvasTransform();
            transform.SetParent(t);
            updateCanvas(hit.collider.gameObject.GetComponentInChildren<Canvas>());
            homePosition = hit.collider.gameObject.GetComponent<RectTransform>().anchoredPosition;
            ReturnHome();
            
            // Then call some function on the stack to add the card object to it
            s.AddCard(gameObject);
            gameObject.GetComponent<Card>().AddEffect(s);
        }
        else
        {
            // Debug.Log("Hit Nothing");
            ReturnHome();
        }
    }

    #endregion
    
    public void updateCanvas(Canvas c)
    {
        canvas = c;
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