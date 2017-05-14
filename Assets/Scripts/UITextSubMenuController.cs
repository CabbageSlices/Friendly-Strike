using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//a text option that when selected goes to a sub menu
public class UITextSubMenuController : UIAbstractSelectionTextController {

    //list of all submenus of created by selecting this option
    //these objects should be prefabs and must be instantiated
    public List<GameObject> subMenuEntries = new List<GameObject>();
    
    public override void onSelect(ShopController shopController) {

        shopController.openSubMenu(subMenuEntries);
    }
}
