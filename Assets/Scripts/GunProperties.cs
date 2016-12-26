using UnityEngine;
using System.Collections;

//class that keeps track of all the properties of a given gun, such as ammo, fire rate, etc
public class GunProperties : MonoBehaviour {

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

    //how much a bullet deviates from the angle that the gun is aimed at
    //bullets will be fired at an angle in the range [gunAngle - bulletSpread, gunAngle + bulletSpread]
    //this angle should be in DEGREES
    [Range(0.0f, 45)]
    public float bulletSpread;

    void Start() {

        remainingBullets = bulletsInMagazine;
        lastFiredTime = -fireDelay;
    }
}
