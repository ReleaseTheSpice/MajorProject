using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Hand : MonoBehaviour
{
    public List<GameObject> cards;
    public bool[] availableSlots;
    
    #region Card Functions
    
    public void AddCard(GameObject card)
    {
        Debug.Log("Adding card " + card.name + " to hand");
        for (int i = 0; i < availableSlots.Length; i++)
        {
            if (availableSlots[i]) // If this slot is available:
            {
                card.GetComponent<Interactable>().draggable = true; // Boolean to allow dragging
                card.transform.SetParent(transform);                // Set the card's parent to this hand
                card.GetComponent<Image>().raycastTarget = true;    // Allow raycasting so the card can receive OnDrag/drop callbacks
                cards.Add(card);
                updateCardPositions();
                availableSlots[i] = false;
                break;
            }
        }
    }
    
    public void RemoveCard(GameObject card)
    {
        Debug.Log("Removing card " + card.name + " from hand");
        int index = cards.IndexOf(card);
        cards.Remove(card);
        availableSlots[index] = true;
        updateCardPositions();
    }

    private void updateCardPositions()
    {
        // Set all slots to available
        availableSlots = Enumerable.Repeat(true, 10).ToArray();
        for (int i = 0; i < cards.Count; i++)
        {
            //cards[i].transform.position = cardSlots[i].transform.position;
            Vector3 newPos = new Vector3(i * 120 - 540, 0, 0);
            cards[i].transform.position = newPos; // Move cards to z=0 so they are visible
            cards[i].GetComponent<Interactable>().homePosition = newPos;
            cards[i].GetComponent<Interactable>().ReturnHome();
            availableSlots[i] = false;
        }
    }
    
    #endregion
    
    #region Callback Handlers
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #endregion
}
