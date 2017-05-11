using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITextSubMenuController : UIAbstractSelectionTextController {

    public List<UIAbstractSelectionTextController> subMenuEntries;

    public override void onSelect(ShopController shopController) {

        //create a new submenu and display it
        shopController.displaySubMenu(subMenuEntries);
    }
}
