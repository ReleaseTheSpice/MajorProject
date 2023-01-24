using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public List<GameObject> cards = new List<GameObject>();
    //public Transform[] cardSlots;
    public bool[] availableSlots;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void AddCard(GameObject card)
    {
        Debug.Log("Adding card to hand");
        for (int i = 0; i < availableSlots.Length; i++)
        {
            if (availableSlots[i]) // If this slot is available:
            {
                card.GetComponent<Interactable>().draggable = true;
                card.transform.SetParent(transform);
                cards.Add(card);
                updateCardPositions();
                availableSlots[i] = false;
                break;
            }
        }

    }
    
    public void RemoveCard(GameObject card)
    {
        Debug.Log("Removing card from hand");
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
            cards[i].transform.position = newPos;
            cards[i].GetComponent<Interactable>().homePosition = newPos;
            //cards[i].GetComponent<Interactable>().ReturnHome();
            availableSlots[i] = false;
        }
    }
}
