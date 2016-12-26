using UnityEngine;
using System.Collections;

//class that keeps track of all the properties of a given gun
//and keeps references to it's parts
public class GunProperties : MonoBehaviour {

    [System.Serializable]
    public struct Parts {

        public GameObject gunBody;//gun body object (parent of cartridge)
        public GameObject cartridge;//cartridge sprite game object, used for reloading animation
        public GameObject movingPart;//movign part of gun that will go back and forth
        public GameObject partThatIsAimed;//part of the gun that is aimed toward the target
    };

    public Parts parts = new Parts();

	// Use this for initialization
	void Start () {

        if (parts.gunBody == null || parts.cartridge == null || parts.movingPart == null || parts.partThatIsAimed == null)
            Debug.LogWarning(gameObject.name + " is missing references to its parts");
	}
}
