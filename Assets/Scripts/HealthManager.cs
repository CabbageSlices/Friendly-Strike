using UnityEngine;
using System.Collections;

//keep track of the player's health, and handle increases/decreases
public class HealthManager : MonoBehaviour {

    public int initialHealth = 100;
    public HealthBarManager healthBarManager;//script that handles the healthbar UI changes

    private int currentHealth;

	// Use this for initialization
	void Start () {

        if (healthBarManager == null)
            Debug.LogWarning("HealthManager is missing healthBarManager reference");

        currentHealth = initialHealth;
	}
	
	public void decreaseHealth(int value) {

        currentHealth -= value;

        if (currentHealth < 0)
            currentHealth = 0;

        updateHealthBar();        
    }

    public void restoreHealth() {

        currentHealth = initialHealth;

        updateHealthBar();
    }

    //updates the healthbar graphics to reflect the current health value
    void updateHealthBar() {

        if (healthBarManager == null)
            return;

        //change healthbar value
        float fractionHealthRemaining = (float)currentHealth / (float)initialHealth;
        healthBarManager.setHealthValue(fractionHealthRemaining);
    }
}
