using UnityEngine;
using System.Collections;

//This class will manage a player's equipped weapon
//It will handle the user's input for the weapon, player's firing, reloading, and changing weapons
public class EquippedWeaponManager : MonoBehaviour {

    //hold together the data of the equipped weapon
    struct EquippedWeapon {

        public GameObject gun;//gun root object (parent of all of the sprites)

        public GunProperties properties;//properties of the gun, like ammo
        public GunParts parts;//gun parts, cached so you don't need to access it using getComponent on the gunproperties script
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
        equippedWeapon.properties = equippedWeapon.gun.GetComponent<GunProperties>() as GunProperties;
        equippedWeapon.parts = equippedWeapon.gun.GetComponent<GunParts>() as GunParts;
    }

    //moves cartridge from gun to left hand for the reloading animation
    public void moveCartridgeToLeftHand() {

        if (equippedWeapon.gun == null)
            return;

        equippedWeapon.parts.cartridge.transform.parent = playerLeftHand.transform;
        resetCartridgeTransform();
    }

    //moves cartridge from left hand back to the gun
    public void moveCartridgeToGun() {

        if (equippedWeapon.gun == null)
            return;

        equippedWeapon.parts.cartridge.transform.parent = equippedWeapon.parts.gunBody.transform;

        resetCartridgeTransform();
    }

    public GameObject getPartOfGunToAim() {

        return equippedWeapon.parts.partThatIsAimed;
    }

    public GameObject getPlayerRightHand() {

        return playerRightHand;
    }

    //resets the local position/rotation/scale of the gun cartridge
    void resetCartridgeTransform() {

        equippedWeapon.parts.cartridge.transform.localPosition = new Vector3(0, 0, 0);
        equippedWeapon.parts.cartridge.transform.localScale = new Vector3(1, 1, 1);
        equippedWeapon.parts.cartridge.transform.localRotation = Quaternion.identity;
    }

    public bool canReload() {

        if (equippedWeapon.gun == null)
            return false;


        //don't let player reload if he has max bullets
        return equippedWeapon.properties.remainingBullets < equippedWeapon.properties.bulletsInMagazine;
    }

    public void reload() {

        if (equippedWeapon.gun == null)
            return;

        equippedWeapon.properties.remainingBullets = equippedWeapon.properties.bulletsInMagazine;
    }

    public void fire() {

        if (equippedWeapon.gun == null)
            return;

        equippedWeapon.properties.remainingBullets -= 1;
    }
    
}
