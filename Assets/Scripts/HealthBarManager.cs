using UnityEngine;
using System.Collections;

//handles the graphics of the health bar
public class HealthBarManager : MonoBehaviour {

    //rect transform of the part of the healthbar that represents the amount of health that is remaining (green part)
    public RectTransform healthValueIndicator;

	// Use this for initialization
	void Start () {

        if (healthValueIndicator == null)
            Debug.Log("HealthBarManager missing healthValueIndicator reference");
	}
	
    //sets the size of the green part of the health bar to the given fraction of total health
    //health can't be below 0 or above 1
	public void setHealthValue(float fractionHealthRemaining) {

        fractionHealthRemaining = Mathf.Clamp(fractionHealthRemaining, 0, 1);

        healthValueIndicator.localScale = new Vector3(fractionHealthRemaining, 1, 1);
    }

    public void hideHealthBar() {

        gameObject.SetActive(false);
    }

    public void showHealthBar() {

        gameObject.SetActive(true);
    }
}
