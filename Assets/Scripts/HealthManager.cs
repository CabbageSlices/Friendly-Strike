using UnityEngine;
using System.Collections;

//keep track of the player's health, and handle increases/decreases
public class HealthManager : MonoBehaviour {

    public int initialHealth = 100;

    private int currentHealth;

	// Use this for initialization
	void Start () {

        currentHealth = initialHealth;
	}
	
	public void decreaseHealth(int value) {

        currentHealth -= value;

        if (currentHealth < 0)
            currentHealth = 0;
    }

    public void restoreHealth() {

        currentHealth = initialHealth;
    }
}
