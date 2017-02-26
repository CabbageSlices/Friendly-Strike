using UnityEngine;
using System.Collections;

//handles the graphics of the health bar
public class HealthBarManager : MonoBehaviour {

    //rect transform of the part of the healthbar that represents the amount of health that is remaining (green part)
    public RectTransform healthValueIndicator;

    //how fast the health should go down, as in how fast the bar moves to a given value
    public float barDepletionSpeed = 5;

    //amount of health the bar should display after depletion effect
    private float targetFractionRemaining = 1;

    //how close the health can get to the target value before jumping to value
    private float valueJumpThreshold = 0.05f;

	// Use this for initialization
	void Start () {

        if (healthValueIndicator == null)
            Debug.Log("HealthBarManager missing healthValueIndicator reference");
	}

    private void Update() {
        
        //move health toward target value until you're close enough
        tweenHealthToTarget();
        
        //health reached 0, should hide the health bar
        if(healthValueIndicator.localScale.x == 0) {

            hideHealthBar();
        }
    }

    private void tweenHealthToTarget() {

        if(Mathf.Abs(healthValueIndicator.localScale.x - targetFractionRemaining) < valueJumpThreshold) {

            healthValueIndicator.localScale = new Vector3(targetFractionRemaining, 1, 1);
            return;
        }

        healthValueIndicator.localScale = new Vector3(Mathf.Lerp(healthValueIndicator.localScale.x, targetFractionRemaining, Time.deltaTime * barDepletionSpeed), 1, 1);
    }

    //sets the size of the green part of the health bar to the given fraction of total health
    //health can't be below 0 or above 1
    public void setHealthValue(float fractionHealthRemaining) {

        fractionHealthRemaining = Mathf.Clamp(fractionHealthRemaining, 0, 1);

        targetFractionRemaining = fractionHealthRemaining;
    }

    public void hideHealthBar() {

        gameObject.SetActive(false);
    }

    public void showHealthBar() {

        gameObject.SetActive(true);
    }
}
