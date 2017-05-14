using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//class that defines all the parameters required to create a UISelectionText
//the parameters set in this class will determine if a UITextSubMenuController or a UISelectionTextController is created
//these paramteres are also used to setup what actions occur when the created text is selected
[System.Serializable]
public class UISelectionTextCreationOptions {

	public string selectionTextType; //"submenuText or actionText
    public string displayText; //string displayed in the menu, i.e "Pistols"
    
    //only for submenuText, array of texts to use efor the submenu geenrated by selecting this text
    //the array should be represented as a string which can be parsed into a json object
    /*i.e 
     * {
     *      submenuTexts: [
     *          "{selectionTextType: "something", displayText: "something"    
     *                  }"
     *      ]  
     * }
     * */
    public string[] submenuTexts;

}

public struct Thing {

    public int a;
    public int b;
}