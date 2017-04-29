using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopController : MonoBehaviour {

    //reference to scroll bar in order handle scrolling
    Scrollbar scrollbar;

    //referecne to the gameobject that will keep all of the content being scrolled
    //all content must be a child of this gameobject
    GameObject content;
    RectTransform contentRectTransform;

    //rect transform of the entire shop object, which is also the size of the mask where objects can be displayed
    RectTransform shopRectTransform;

    //border that indicates which option is currently selected
    GameObject selectionIndicator;
    RectTransform selectionIndicatorRectTransform;

    //collection of all text displayed
    List<Text> textList = new List<Text>();

    //id of the text currently being selected in the shop display
    int idCurrentSelection = 0;

    //height of each object displayed in the shop
    int displayedObjectHeight = 39;

    //number of objects that can be displayed through the viewport mask
    int numberOfObjectsDisplayed;

	// Use this for initialization
	void Start () {
		
        setupReferences();

        numberOfObjectsDisplayed = (int)shopRectTransform.rect.height / displayedObjectHeight;

        //set height of content rect to just enough to display all the objects that way we can't scroll over empty slots
        contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, displayedObjectHeight * textList.Count);

        selectionIndicatorRectTransform.sizeDelta = new Vector2(contentRectTransform.rect.width - 2, displayedObjectHeight - 1);
        
        for(int i = 0; i < textList.Count; ++i) {

            Text text = textList[i];
            text.rectTransform.localPosition = new Vector3(2, -displayedObjectHeight * i);
        }
	}

    void setupReferences() {

        scrollbar = GetComponentInChildren<Scrollbar>() as Scrollbar;
        content = transform.Find("Viewport/Content").gameObject;
        contentRectTransform = content.GetComponent<RectTransform>() as RectTransform;
        shopRectTransform = GetComponent<RectTransform>() as RectTransform;
        selectionIndicator = transform.Find("Viewport/Content/SelectionIndicator").gameObject;
        selectionIndicatorRectTransform = selectionIndicator.GetComponent<RectTransform>() as RectTransform;


        var texts = GetComponentsInChildren<Text>();
        foreach(var obj in texts) {

            textList.Add(obj as Text);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
        //contentRectTransform.localPosition = contentRectTransform.localPosition + new Vector3(0, 2, 0);
        if(Input.GetKeyDown(KeyCode.DownArrow) ) {

            goDownOneSelection();
        }

        if(Input.GetKeyDown(KeyCode.UpArrow)) {

            goUpOneSelection();
        }
            
	}

    //response to player pressing down
    public void goDownOneSelection() {
        
        //select the next item
        //make sure it doesn't exceed the number of items
        idCurrentSelection = Mathf.Clamp(idCurrentSelection + 1, 0, textList.Count - 1);

        //if the selected item is outside of  the view then move the view down by the height of one object
        var selectedTextRectTransform = textList[idCurrentSelection].gameObject.GetComponent<RectTransform>() as RectTransform;

        //none of the displayed text will ever move, only the content rect is moved
        //to determine if the currently selected text can be viewed, we need to dtermine if the current offset of the content rect causes the selected text to be placed outside the view port
        //by subtracting the content rects position from the selected text's position we determine how far from the top of the viewPort the selected text is located
        //if it is located outside of hte view port bounds (difference in possition is bigger than the heighto f hte view port) then we need to move the content rect upwards so we can see the selected text
        //we  add half the text's height that way we don't scroll down too  early
        if(selectedTextRectTransform.localPosition.y * -1 + displayedObjectHeight / 2 - contentRectTransform.localPosition.y > shopRectTransform.rect.height)
            contentRectTransform.localPosition = contentRectTransform.localPosition + new Vector3(0, displayedObjectHeight, 0);

        selectionIndicatorRectTransform.localPosition = selectedTextRectTransform.localPosition;

    }

    //response to player pressing up
    public void goUpOneSelection() {

        //select the next item
        //make sure it doesn't exceed the number of items
        idCurrentSelection = Mathf.Clamp(idCurrentSelection - 1, 0, textList.Count - 1);

        //if the selected item is outside of  the view then move the view down by the height of one object
        var selectedTextRectTransform = textList[idCurrentSelection].rectTransform;

        //similar idea as for goDownOneSelection
        if (selectedTextRectTransform.localPosition.y * -1 - contentRectTransform.localPosition.y < -1)
            contentRectTransform.localPosition = contentRectTransform.localPosition - new Vector3(0, displayedObjectHeight, 0);

        selectionIndicatorRectTransform.localPosition = selectedTextRectTransform.localPosition;
    }
}
