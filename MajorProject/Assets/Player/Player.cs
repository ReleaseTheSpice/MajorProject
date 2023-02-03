using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject myDeck;
    public GameObject myHand;
    public GameObject lifeText;
    
    public int life;
    

    #region Callback Handlers

    // Start is called before the first frame update
    void Start()
    {
        life = 20;
        
        List<int> deckCode = new List<int>(){ 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2 };
        myDeck.GetComponent<Deck>().GenerateDeck(deckCode);
    }

    // Update is called once per frame
    void Update()
    {
        lifeText.GetComponent<TextMeshProUGUI>().text = life.ToString();
    }
    
    #endregion
    
}
