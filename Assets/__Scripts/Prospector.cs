using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class Prospector : MonoBehaviour {

	static public Prospector 	S;

	[Header("Set in Inspector")]
	public TextAsset			deckXML;
    public TextAsset layoutXML;
    public float xOffset = 3;
    public float yOffset = -2.5f;
    public Vector3 layoutCenter;

	[Header("Set Dynamically")]
	public Deck					deck;
    public Layout layout;
    public List<CardProspector> drawPile;
    public Transform layoutAnchor;
    public CardProspector target;
    public List<CardProspector> tableau;
    public List<CardProspector> discardPile;

	void Awake(){
		S = this;
	}

	void Start() {
		deck = GetComponent<Deck> ();
		deck.InitDeck (deckXML.text);
        Deck.Shuffle(ref deck.cards);

        // Card c;
        // for(int cNum = 0; cNum <deck.cards.Count; cNum++){
        //  c = deck.cards[cNum];
        //  c.transform.localPosition = new Vector3((cNum%13)*3, cNum/13*4, 0);
        // }

        layout = GetComponent<Layout>();
        layout.ReadLayout(layoutXML.text);
        drawPile = ConvertListCardsToListCardProspectors(deck.cards);
        LayoutGame();
	}


    List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> CD)
    {
        List<CardProspector> CP = new List<CardProspector>();
        CardProspector tCP;
        foreach(Card tCD in CD){
            tCP = tCD as CardProspector;
            CP.Add(tCP);
        }
        return (CP);
    }

    CardProspector Draw()
    {
        CardProspector cd = drawPile[0];
        drawPile.RemoveAt(0);
        return (cd);
    }

    void LayoutGame()
    {
        if(layoutAnchor == null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;
            layoutAnchor.transform.position = layoutCenter;
        }

        CardProspector cp;
        foreach (SlotDef tSD in layout.slotDefs)
        {
            cp = Draw();
            cp.faceUp = tSD.faceUp;
            cp.transform.parent = layoutAnchor;
            cp.transform.localPosition = new Vector3(layout.multipier.x*tSD.x, layout.multipier.y*tSD.y,-tSD.layerID);
            cp.layoutID = tSD.id;
            cp.slotDef = tSD;
            cp.state = eCardState.tableau;
            cp.SetSortingLayerName(tSD.layerName);
            tableau.Add(cp);
        }


        MoveToTarget(Draw());
        UpdateDrawPile();
    }

    void MoveToDiscard(CardProspector cd)
    {
        cd.state = eCardState.discard;
        discardPile.Add(cd);
        cd.transform.parent = layoutAnchor;
        cd.transform.localPosition = new Vector3(layout.multipier.x*layout.discardPile.x, layout.multipier.y*layout.discardPile.y, -layout.discardPile.layerID+0.5f);
        cd.faceUp = true;
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(-100 + discardPile.Count);

    }
    void MoveToTarget(CardProspector cd)
    {
        if (target != null) MoveToDiscard(target);
        target = cd;
        cd.state = eCardState.target;
        cd.transform.parent = layoutAnchor;
        cd.transform.localPosition = new Vector3(layout.multipier.x * layout.discardPile.x, layout.multipier.y * layout.discardPile.y, -layout.discardPile.layerID);
        cd.faceUp = true;
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(0);
    }
    void UpdateDrawPile()
    {
        CardProspector cd;
        for(int i = 0; i<drawPile.Count; i++)
        {
            cd = drawPile[i];
            cd.transform.parent = layoutAnchor;
            Vector2 dpStagger = layout.drawPile.stagger;
            cd.transform.localPosition = new Vector3(layout.multipier.x * (layout.drawPile.x + i * dpStagger.x), layout.multipier.y * (layout.drawPile.y + i * dpStagger.y), -layout.drawPile.layerID + 0.1f * i);
            cd.faceUp = false;
            cd.state = eCardState.drawpile;
            cd.SetSortingLayerName(layout.drawPile.layerName);
            cd.SetSortOrder(-10 * i);
        }
    }

    public void CardClicked(CardProspector cd)
    {
        switch (cd.state)
        {
            case eCardState.target:
                break;
            case eCardState.drawpile:
                MoveToDiscard(target);
                MoveToTarget(Draw());
                UpdateDrawPile();
                break;
            case eCardState.tableau:
                bool validMatch = true;
                if (!cd.faceUp)
                {
                    validMatch = false;
                }
                if(!AdjacentRank(cd, target))
                {
                    validMatch = false;
                }
                if (!validMatch) return;
                tableau.Remove(cd);
                MoveToTarget(cd);
                break;
        }
    }
    
    public bool AdjacentRank(CardProspector c0, CardProspector c1)
    {
        if (!c0.faceUp || !c1.faceUp) return (false);
        if(Mathf.Abs(c0.rank - c1.rank) == 1)
        {
            return (true);
        }
        if (c0.rank == 1 && c1.rank == 13) return (true);
        if (c0.rank == 13 && c1.rank == 1) return (true);
        return (false);
    }




}
