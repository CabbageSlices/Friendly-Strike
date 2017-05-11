using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//class that proivdes functionailty for all the text options displayed in a scrollable options list.
//for example, the inter-round shop will feature a list of options. Each option needs this text controller to handle selection events
public abstract class UIAbstractSelectionTextController : MonoBehaviour {

    //the text script attatched to the game object that this controllerr is attached to
    public Text text;

    //should do something when this text is selected
	public abstract void onSelect(ShopController shopController);

}
