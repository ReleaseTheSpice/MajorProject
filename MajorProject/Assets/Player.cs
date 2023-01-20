using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject deck;
    
    // Start is called before the first frame update
    void Start()
    {
        //Deck myDeck = new Deck(1);
        // Instantiate a gameobject from a prefab, grab its deck component, and assign it to myDeck
        Deck myDeck = Instantiate(deck, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<Deck>();
        
        List<int> deckCode = new List<int>(){ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        myDeck.GenerateDeck(deckCode);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
