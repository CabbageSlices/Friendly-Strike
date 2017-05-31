using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPurchaseWeaponController : MonoBehaviour, IMenuEntry {

    //name of gun to purchace
    public string gunName;

    //store everything in an external shop file that maps a weapon name to it's price and prefab
	public void onSelect(ShopController shopController) {
        
       GunShopDatabase.GunData gun = GunShopDatabase.guns[gunName];
       shopController.handlePurchaseRequest(gun);
    }
}