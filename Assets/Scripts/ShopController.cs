using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
    //different from the text object's rect height because we made the text extra large and scaled it down to makei t look better, and so the rect height is incorrect
    int displayedObjectHeight = 39;
    public string z;

	// Use this for initialization
	void Start () {

        setupReferences();

        recreateShopDisplay();
        resizeContentRectToFitContent();
        positionTexts();
	}

    //loop through all the display texts and position them in the content game objecet
    void positionTexts() {

        //y position of the bottom of the text that was positioned before the current text
        //position is relative to topleft of the content rect
        //used to position each text one after another
        float bottomOfPreviousText = 0;

        for(int i = 0; i < textList.Count; ++i) {

            Text text = textList[i];

            float heightPreviousText = i == 0 ? 0 : calculateDisplayedHeight(textList[i - 1]);
            bottomOfPreviousText += heightPreviousText;

            text.rectTransform.localPosition = new Vector3(1, -bottomOfPreviousText);
        }
    }

    //calculates the eactual displayed height of the given text
    float calculateDisplayedHeight(Text text) {

        //calculate actual height of text, texts are scaled so we ehave to scale the rect height to get the size in the world
        return text.transform.localScale.y * text.rectTransform.rect.height;
    }

    //resizes the contrent rect and repositions texts
    void recreateShopDisplay() {

        resizeContentRectToFitContent();
        positionTexts();
    }

    //resize the content rect so that it just barely fits all of the elements
    //the width is never changed,it will be hte same as the viewport width, only the height will be changed
    void resizeContentRectToFitContent() {

        float totalSize = 0;

        foreach(Text text in textList) {

            totalSize += calculateDisplayedHeight(text);
        }

        contentRectTransform.sizeDelta = new Vector2(1, totalSize);
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

        //player selected an option, handle the current selection
        //if(Input.GetKeyDown(KeyCode.Return))
            //textList[idCurrentSelection].onSelect(this);
            
	}

    void highlightCurrentlySelectedText() {

        var selectedTextRectTransform = textList[idCurrentSelection].gameObject.GetComponent<RectTransform>() as RectTransform;

        selectionIndicatorRectTransform.localPosition = selectedTextRectTransform.localPosition;
        selectionIndicatorRectTransform.sizeDelta = new Vector2 (contentRectTransform.rect.width - 2, calculateDisplayedHeight(textList[idCurrentSelection]));
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
        if (selectedTextRectTransform.localPosition.y * -1 + displayedObjectHeight / 2 - contentRectTransform.localPosition.y > shopRectTransform.rect.height)
            contentRectTransform.localPosition = contentRectTransform.localPosition + new Vector3(0, displayedObjectHeight, 0);

        highlightCurrentlySelectedText();
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
        
        highlightCurrentlySelectedText();
    }

    //take the given list of menu entries and replaces the current menu options with the new ones
    //asssumes all game objects in the given list are references to prefabs so they must be instansiated
    public void openSubMenu(List<GameObject> menuEntries) {

        List<GameObject> newSubMenu = new List<GameObject>();

        foreach(GameObject obj in menuEntries) {

            GameObject newEntry = GameObject.Instantiate(obj, content.transform);
            newSubMenu.Add(newEntry);
        }

        recreateShopDisplay();
    }
}
