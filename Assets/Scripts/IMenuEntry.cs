using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//menu entry interface, all objectss taht should be part of a menu need to implement this interface
public interface IMenuEntry {

	void onSelect(ShopController shopController);
}
