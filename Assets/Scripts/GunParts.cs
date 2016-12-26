using UnityEngine;
using System.Collections;

//class that keeps references to it's parts
public class GunParts : MonoBehaviour {

    public GameObject gunBody;//gun body object (parent of cartridge)
    public GameObject cartridge;//cartridge sprite game object, used for reloading animation
    public GameObject movingPart;//movign part of gun that will go back and forth
    public GameObject partThatIsAimed;//part of the gun that is aimed toward the target

    // Use this for initialization
    void Start() {

        if (gunBody == null || cartridge == null || movingPart == null || partThatIsAimed == null)
            Debug.LogWarning(gameObject.name + " is missing references to its parts");
    }
}
