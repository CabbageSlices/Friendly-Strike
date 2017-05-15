using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Menu = System.Collections.Generic.List<UnityEngine.GameObject>; //Menu is just a list of ui entry controllers

public class ShopController : MonoBehaviour {

    //referecne to the gameobject that will keep all of the content being scrolled
    //all content must be a child of this gameobject
    GameObject content;
    RectTransform contentRectTransform;

    //rect transform of the entire shop object, which is also the size of the mask where objects can be displayed
    RectTransform shopRectTransform;

    //border that indicates which option is currently selected
    GameObject selectionIndicator;
    RectTransform selectionIndicatorRectTransform;

    //list of all the menus that exist, this way when you press back on a menu you can access the previous menu
    List<Menu> menus = new List<Menu>();

    //id of  currently opened menu
    int idCurrentlyOpenedMenu = 0;

    //id of the entry currently being selected in the shop display in each of the sub menus
    List<int> idCurrentSelectionInMenu = new List<int>();

    //prefab of backbutton, used to add a backb utton to submenus
    public GameObject backButtonPrefab;

    // Use this for initialization
    void Start() {

        setupReferences();

        //delay hte call because unity's layout group scripts and content size fitters need a frame to update and we can't access the contentRects size to resize everything for atleast a frame
        Invoke("recreateShopDisplay", 0.001f);
    }

    void setupReferences() {

       // scrollbar = GetComponentInChildren<Scrollbar>() as Scrollbar;
        content = transform.Find("Viewport/Content").gameObject;
        contentRectTransform = content.GetComponent<RectTransform>() as RectTransform;
        shopRectTransform = GetComponent<RectTransform>() as RectTransform;
        selectionIndicator = transform.Find("Viewport/Content/SelectionIndicator").gameObject;
        selectionIndicatorRectTransform = selectionIndicator.GetComponent<RectTransform>() as RectTransform;

        //get all entries in default menu
        Menu menu = new Menu();
        foreach (Transform menuEntry in content.transform) {

            //don't add the seelction indicator to the default list of options
            if(menuEntry.gameObject.name != "SelectionIndicator")
                menu.Add(menuEntry.gameObject);
        }

        menus.Add(menu);
        idCurrentSelectionInMenu.Add(0);

    }

    RectTransform getRectTransform(GameObject obj) {

        return obj.GetComponent<RectTransform>() as RectTransform;
    }

    //loop through all the entries in the current menu and position them in the content game objecet
    void positionCurrentMenuEntries() {

        //y position of the bottom of the entry that was positioned before the current entry
        //position is relative to topleft of the content rect
        //used to position each entry one after another
        float bottomOfPreviousEntry = 0;

        Menu currentMenu = menus[idCurrentlyOpenedMenu];

        for (int i = 0; i < currentMenu.Count; ++i) {

            RectTransform entryRectTransform = getRectTransform(currentMenu[i]);

            float heightPreviousEntry = i == 0 ? 0 : calculateDisplayedHeight(currentMenu[i - 1]);
            bottomOfPreviousEntry += heightPreviousEntry;

            entryRectTransform.localPosition = new Vector3(1, -bottomOfPreviousEntry);
        }
    }

    //calculates the actual displayed height of the menu entry
    float calculateDisplayedHeight(GameObject entry) {

        //calculate actual height of entry, entrys are scaled so we ehave to scale the rect height to get the size in the world
        return entry.transform.localScale.y * getRectTransform(entry).rect.height;
    }

    //resizes the contrent rect and repositions entries for the currently opened menu
    //does not touch any menu objects except the currently opened menu ones
    void recreateShopDisplay() {

        resizeContentRectToFitContent();
        positionCurrentMenuEntries();
        
        highlightCurrentlySelectedEntry();
    }

    //goes through all entries in the given menu and enables them
    void enableAllEntries(Menu menu) {

        foreach(GameObject obj in menu) {

            obj.SetActive(true);
        }
    }

    void disableAllEntries(Menu menu) {

        foreach (GameObject obj in menu) {

            obj.SetActive(false);
        }
    }

    //removes all the menu entry game objects from the given menu, and then removes the menu from the list of all menus
    void removeMenu(int idMenuToRemove) {

        foreach(GameObject obj in menus[idMenuToRemove])
            GameObject.DestroyImmediate(obj);

        menus.RemoveAt(idMenuToRemove);
        
        //remove the seelction indicator for this menu
        idCurrentSelectionInMenu.RemoveAt(idMenuToRemove);
    }

    //goes through every menu but the currently open menu and disables the gameobjects in that menu
    //then it enables the currently opneed menu
    void enableOnlyOpenedMenu() {

        for(int i = 0; i < menus.Count; ++i) {

            if(i == idCurrentlyOpenedMenu)
                enableAllEntries(menus[i]);
            else
                disableAllEntries(menus[i]);
        }
    }

    //resize the content rect so that it just barely fits all of the elements
    //the width is never changed,it will be hte same as the viewport width, only the height will be changed
    void resizeContentRectToFitContent() {

        float totalSize = 0;

        foreach (GameObject entry in menus[idCurrentlyOpenedMenu]) {

            totalSize += calculateDisplayedHeight(entry);
        }
        
        contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, totalSize);
    }

    // Update is called once per frame
    void Update() {

        int idCurrentSelection = idCurrentSelectionInMenu[idCurrentlyOpenedMenu];
        //contentRectTransform.localPosition = contentRectTransform.localPosition + new Vector3(0, 2, 0);
        if (Input.GetKeyDown(KeyCode.DownArrow)) {

            goDownOneSelection();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow)) {

            goUpOneSelection();
        }

        if(Input.GetKeyDown(KeyCode.Return)) {

            (menus[idCurrentlyOpenedMenu][idCurrentSelection].GetComponent<IMenuEntry>() as IMenuEntry).onSelect(this);
        }

        if (Input.GetKeyDown(KeyCode.Backspace)) {

            goBackToPrevioussMenu();
        }

        highlightCurrentlySelectedEntry();

        //player selected an option, handle the current selection
        //if(Input.GetKeyDown(KeyCode.Return))
        //entryList[idCurrentSelection].onSelect(this);

    }

    void highlightCurrentlySelectedEntry() {

        int idCurrentSelection = idCurrentSelectionInMenu[idCurrentlyOpenedMenu];

        var selectedEntryRectTransform = menus[idCurrentlyOpenedMenu][idCurrentSelection].GetComponent<RectTransform>() as RectTransform;

        selectionIndicatorRectTransform.localPosition = selectedEntryRectTransform.localPosition;
        selectionIndicatorRectTransform.sizeDelta = new Vector2(contentRectTransform.rect.width - 2, calculateDisplayedHeight(menus[idCurrentlyOpenedMenu][idCurrentSelection]));
    }

    //response to player pressing down
    public void goDownOneSelection() {

        int idCurrentSelection = idCurrentSelectionInMenu[idCurrentlyOpenedMenu];

        //select the next item
        //make sure it doesn't exceed the number of items
        idCurrentSelection = Mathf.Clamp(idCurrentSelection + 1, 0, menus[idCurrentlyOpenedMenu].Count - 1);
        idCurrentSelectionInMenu[idCurrentlyOpenedMenu] = idCurrentSelection;

        //if the selected item is outside of  the view then move the view down by the height of one object
        var selectedEntryRectTransform = menus[idCurrentlyOpenedMenu][idCurrentSelection].gameObject.GetComponent<RectTransform>() as RectTransform;

        float displayedObjectHeight = calculateDisplayedHeight(menus[idCurrentlyOpenedMenu][idCurrentSelection]);

        //none of the displayed entry will ever move, only the content rect is moved
        //to determine if the currently selected entry can be viewed, we need to dtermine if the current offset of the content rect causes the selected entry to be placed outside the view port
        //by subtracting the content rects position from the selected entry's position we determine how far from the top of the viewPort the selected entry is located
        //if it is located outside of hte view port bounds (difference in possition is bigger than the heighto f hte view port) then we need to move the content rect upwards so we can see the selected entry
        //we  add half the entry's height that way we don't scroll down too  early
        if (selectedEntryRectTransform.localPosition.y * -1 + displayedObjectHeight / 2 - contentRectTransform.localPosition.y > shopRectTransform.rect.height)
            contentRectTransform.localPosition = contentRectTransform.localPosition + new Vector3(0, displayedObjectHeight, 0);

        highlightCurrentlySelectedEntry();
    }

    //response to player pressing up
    public void goUpOneSelection() {

        int idCurrentSelection = idCurrentSelectionInMenu[idCurrentlyOpenedMenu];

        //select the next item
        //make sure it doesn't exceed the number of items
        idCurrentSelection = Mathf.Clamp(idCurrentSelection - 1, 0, menus[idCurrentlyOpenedMenu].Count - 1);
        idCurrentSelectionInMenu[idCurrentlyOpenedMenu] = idCurrentSelection;

        //if the selected item is outside of  the view then move the view down by the height of one object
        var selectedEntryRectTransform = getRectTransform(menus[idCurrentlyOpenedMenu][idCurrentSelection]);

        float displayedObjectHeight = calculateDisplayedHeight(menus[idCurrentlyOpenedMenu][idCurrentSelection]);

        //similar idea as for goDownOneSelection
        if (selectedEntryRectTransform.localPosition.y * -1 - contentRectTransform.localPosition.y < -1)
            contentRectTransform.localPosition = contentRectTransform.localPosition - new Vector3(0, displayedObjectHeight, 0);

        highlightCurrentlySelectedEntry();
    }

    //take the given list of menu entries and replaces the current menu options with the new ones
    //asssumes all game objects in the given list are references to prefabs so they must be instansiated
    public void openSubMenu(List<GameObject> menuEntries) {

        List<GameObject> newSubMenu = new List<GameObject>();

        foreach (GameObject obj in menuEntries) {

            GameObject newEntry = GameObject.Instantiate(obj);
            newEntry.transform.SetParent(content.transform, false);
            newSubMenu.Add(newEntry);
        }

        //create a back button
        GameObject backButton = GameObject.Instantiate(backButtonPrefab);
        backButton.transform.SetParent(content.transform, false);
        newSubMenu.Add(backButton);

        menus.Add(newSubMenu);

        //create a seelction indicator for this new menu
        idCurrentSelectionInMenu.Add(0);

        idCurrentlyOpenedMenu = menus.Count - 1;
        recreateShopDisplay();
        enableOnlyOpenedMenu();
    }

    public void goBackToPrevioussMenu() {

        //can't go back to previous menu since this is the only menu
        if(idCurrentlyOpenedMenu == 0)
            return;

        idCurrentlyOpenedMenu -= 1;
        removeMenu(idCurrentlyOpenedMenu + 1);
        recreateShopDisplay();
        enableOnlyOpenedMenu();
    }
}
