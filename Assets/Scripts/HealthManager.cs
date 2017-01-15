using UnityEngine;
using System.Collections;

//keep track of the player's health, and handle increases/decreases
public class HealthManager : MonoBehaviour {

    public delegate void HealthEvent();
    public delegate void HealthChangeEvent(int newHealth);

    public event HealthEvent onZeroHealth;//called when health reaches 0
    public event HealthEvent onHealthRestore;//called when health is restored to full health
    public event HealthChangeEvent onHealthChange;//called when player's health changes value somehow

    public int initialHealth = 100;
    public HealthBarManager healthBarManager;//script that handles the healthbar UI changes

    private int currentHealth;

	// Use this for initialization
	void Start () {

        if (healthBarManager == null)
            Debug.LogWarning("HealthManager is missing healthBarManager reference");

        currentHealth = initialHealth;
	}

    //updates the healthbar graphics to reflect the current health value
    void updateHealthBar() {

        if (healthBarManager == null)
            return;

        //change healthbar value
        float fractionHealthRemaining = (float)currentHealth / (float)initialHealth;
        healthBarManager.setHealthValue(fractionHealthRemaining);
    }

    public void decreaseHealth(int value) {

        currentHealth -= value;

        if (currentHealth <= 0) {

            currentHealth = 0;

            if (onHealthChange != null)
                onHealthChange(currentHealth); //run this once before onZeroHealth because the onZeroHealth might kill the player and disable his game object, which would prevent the onhealthChange from activiating and so status display box won't update

            if(onZeroHealth != null)
                onZeroHealth();
        }

        if(onHealthChange != null)
            onHealthChange(currentHealth);

        updateHealthBar();        
    }

    public void restoreHealth() {

        currentHealth = initialHealth;
           
        if(onHealthChange != null)
            onHealthChange(currentHealth);

        if(onHealthRestore != null)
            onHealthRestore();
        
        updateHealthBar();
    }

    public int getCurrentHealth() {

        return currentHealth;
    }
}
