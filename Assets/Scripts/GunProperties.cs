using UnityEngine;
using System.Collections;

//class that keeps track of all the properties of a given gun, such as ammo, fire rate, etc
public class GunProperties : MonoBehaviour {

    //weapon type lets player determine what type of animation to play for this weapon
    public enum Type {

        Pistol = 1,
        SMG
    };

    public Type weaponType;

    public int bulletsInMagazine;
    public int remainingBullets;

    //how long the user has to wait before being able to fire a gun again, in SECONDS
    [Range(0.0f, 5.0f)]
    public float fireDelay;

    //time since the last the the gun was fired, in seconds
    [System.NonSerialized]
    public float lastFiredTime;

    //bullet that should be produced by this gun when it fires
    public GameObject bullet;

    //cartridge attached to this gun
    //used to get the initial position/rotation/scale of the cartridge that way when the player does a reloading animation he can
    //use the stored values of the position/rotation/scale to put the cartridge back into the gun
    public GameObject cartridge;

    //initial position/rotation/scale of the cartrdige RELATIVE TO THE GUN BODY
    [HideInInspector]
    public Vector3 initialCartridgePosition;
    [HideInInspector]
    public Vector3 initialCartridgeScale;
    [HideInInspector]
    public Quaternion initialCartridgeRotation;

    //how much a bullet deviates from the angle that the gun is aimed at
    //bullets will be fired at an angle in the range [gunAngle - bulletSpread, gunAngle + bulletSpread]
    //this angle should be in DEGREES
    [Range(0.0f, 45)]
    public float bulletSpread;

    void Start() {

        initialCartridgePosition = cartridge.transform.localPosition;
        initialCartridgeScale = cartridge.transform.localScale;
        initialCartridgeRotation = cartridge.transform.localRotation;

        remainingBullets = bulletsInMagazine;
        lastFiredTime = -fireDelay;
    }
}
