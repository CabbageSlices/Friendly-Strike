using UnityEngine;
using System.Collections;

//This class will manage a player's equipped weapon
//It will handle the user's input for the weapon, player's firing, reloading, and changing weapons
public class EquippedWeaponManager : MonoBehaviour {

    //hold together the data of the equipped weapon
    struct EquippedWeapon {

        public GameObject gun;//gun root object (parent of all of the sprites)
        public GameObject gunBody;//gun body object (parent of cartridge)
        public GameObject cartridge;//cartridge sprite game object, used for reloading animation
    }

    //the weapon currently equipped by the player
    EquippedWeapon equippedWeapon = new EquippedWeapon();

    //player's left hand reference, used to put cartridge into hand for reloading animation
    public GameObject playerLeftHand;

    //right hand reference, used to equip the gun
    public GameObject playerRightHand;

    void Start() {

        if (playerLeftHand == null)
            Debug.LogWarning("EquippedWeaponManager is missing a reference to playerLeftHand");

        if (playerRightHand == null)
            Debug.LogWarning("EquippedWeaponManager is missing a reference to playerRightHand");

        //if there is a gun in the player's hand already then get a reference to it
        getReferenceToEquippedGun();
    }

    void getReferenceToEquippedGun() {

        Transform gunTransform = playerRightHand.transform.Find("Gun");

        //no equipped gun, no need to find references
        if (gunTransform == null)
            return;

        equippedWeapon.gun = gunTransform.gameObject;
        equippedWeapon.gunBody = gunTransform.Find("GunBody").gameObject;
        equippedWeapon.cartridge = gunTransform.Find("GunBody/GunCartridge").gameObject;
    }

    //moves cartridge from gun to left hand for the reloading animation
    public void moveCartridgeToLeftHand() {

        if (equippedWeapon.cartridge == null)
            return;

        equippedWeapon.cartridge.transform.parent = playerLeftHand.transform;

        resetCartridgeTransform();
    }

    //moves cartridge from left hand back to the gun
    public void moveCartridgeToGun() {

        if (equippedWeapon.cartridge == null)
            return;

        equippedWeapon.cartridge.transform.parent = equippedWeapon.gunBody.transform;

        resetCartridgeTransform();
    }

    //resets the local position/rotation/scale of the gun cartridge
    void resetCartridgeTransform() {

        equippedWeapon.cartridge.transform.localPosition = new Vector3(0, 0, 0);
        equippedWeapon.cartridge.transform.localScale = new Vector3(1, 1, 1);
        equippedWeapon.cartridge.transform.localRotation = Quaternion.identity;

    }

    public bool canReload() {

        return true;
    }
    
}
