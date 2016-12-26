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

    void Start() {

        remainingBullets = bulletsInMagazine;
        lastFiredTime = -fireDelay;
    }
}
