using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//when the text containing this controller is selected and calls the onSelect function, the menu will go back to he previous menu
public class UIGoBackTextController : MonoBehaviour, IMenuEntry {

    public void onSelect(ShopController shopController) {
        
        shopController.goBackToPrevioussMenu();
    }
}
