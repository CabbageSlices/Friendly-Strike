using UnityEngine;
using System.Collections;

//class that keeps track of all the properties of a given gun
//and keeps references to it's parts
public class GunProperties : MonoBehaviour {

    

	// Use this for initialization
	void Start () {

        if (parts.gunBody == null || parts.cartridge == null || parts.movingPart == null || parts.partThatIsAimed == null)
            Debug.LogWarning(gameObject.name + " is missing references to its parts");
	}
}
