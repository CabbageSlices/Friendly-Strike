using UnityEngine;
using System.Collections;

//class that keeps track of all the properties of a given gun, such as ammo, fire rate, etc
public class GunProperties : MonoBehaviour {

    public int bulletsInMagazine;
    public int remainingBullets;

    void Start() {

        remainingBullets = bulletsInMagazine;
    }
}
