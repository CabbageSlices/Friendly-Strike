using UnityEngine;
using System.Collections;

//keep track of the player's health, and handle increases/decreases
public class HealthManager : MonoBehaviour {

    public delegate void HealthEvent();

    public event HealthEvent onZeroHealth;//called when health reaches 0
    public event HealthEvent onHealthRestore;//called when health is restored to full health

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

        if (currentHealth <= 0) {

            currentHealth = 0;
            onZeroHealth();
        }

        updateHealthBar();        
    }

    public void restoreHealth() {

        currentHealth = initialHealth;

        onHealthRestore();
        
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
