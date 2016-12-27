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

        //angle difference between the vector that goes from the base of the player's arm to the hand, and the vector that goes from the
        //base of the player's arm to the tip of the gun's barrel
        //this is used to modify the player's aim slightly so that the gun lines up with the vector to the player's target, instead of the player's arm
        //being lined up
        //IN RADIANS
        //see PlayerController::determineSpriteArmOrientation for informatoin about usage
        public float gunElevationAbovePlayerHands;
    }

    //the weapon currently equipped by the player
    EquippedWeapon equippedWeapon = new EquippedWeapon();

    public PlayerBodyParts playerBodyParts;

    void Start() {

        if (playerBodyParts == null)
            Debug.LogWarning("EquippedWeaponManager missing reference to playerBodyParts");

        //if there is a gun in the player's hand already then get a reference to it
        getReferenceToEquippedGun();
    }

    void getReferenceToEquippedGun() {

        Transform gunTransform = playerBodyParts.rightHand.transform.Find("Gun");

        //no equipped gun, no need to find references
        if (gunTransform == null)
            return;

        equippedWeapon.gun = gunTransform.gameObject;
        equippedWeapon.properties = equippedWeapon.gun.GetComponent<GunProperties>() as GunProperties;
        equippedWeapon.parts = equippedWeapon.gun.GetComponent<GunParts>() as GunParts;

        equippedWeapon.gunElevationAbovePlayerHands = calculateEquippedGunElevation();
    }

    //calculate the angle of elevation of the tip of the gun barrel above the player's hands, IN RADIANS
    float calculateEquippedGunElevation() {

        if (equippedWeapon.gun == null)
            return 0;

        Vector2 armToBarrelTip = equippedWeapon.parts.partThatIsAimed.transform.position - playerBodyParts.arms.transform.position;
        Vector2 armToHandTip = playerBodyParts.rightHand.transform.position - playerBodyParts.arms.transform.position;

        float cosAngle = Vector2.Dot(armToHandTip.normalized, armToBarrelTip.normalized);
        float angleOffset = Mathf.Acos(cosAngle);

        //gotta divide by two for whatever reason
        return angleOffset;
    }

    //resets the local position/rotation/scale of the gun cartridge
    void resetCartridgeTransform() {

        equippedWeapon.parts.cartridge.transform.localPosition = new Vector3(0, 0, 0);
        equippedWeapon.parts.cartridge.transform.localScale = new Vector3(1, 1, 1);
        equippedWeapon.parts.cartridge.transform.localRotation = Quaternion.identity;
    }

    //moves cartridge from gun to left hand for the reloading animation
    public void moveCartridgeToLeftHand() {

        if (equippedWeapon.gun == null)
            return;

        equippedWeapon.parts.cartridge.transform.parent = playerBodyParts.leftHand.transform;
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

    public float getGunElevationAbovePlayerHands() {

        return equippedWeapon.gunElevationAbovePlayerHands;
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

    //gun can fire if there is enough ammo, and the fire delay has passed
    public bool canFire() {

        if (equippedWeapon.gun == null)
            return false;

        return Time.time > equippedWeapon.properties.lastFiredTime + equippedWeapon.properties.fireDelay && equippedWeapon.properties.remainingBullets > 0;
    }

    //angle that the player rotated his arms by in order to aim at the target, IN RADIANS
    public void fire(float angleToTarget) {

        if (equippedWeapon.gun == null)
            return;

        equippedWeapon.properties.remainingBullets -= 1;
        equippedWeapon.properties.lastFiredTime = Time.time;

        GameObject bullet = Instantiate(equippedWeapon.properties.bullet);
        (bullet.GetComponent<BulletBehaviour>() as BulletBehaviour).fire(equippedWeapon.parts.partThatIsAimed.transform.position, angleToTarget, equippedWeapon.properties.bulletSpread);
    }
    
}
