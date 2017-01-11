using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Class that will manage a status display box and provide functions to get/set the different information
//that is displayed on the box
public class StatusDisplayBoxController : MonoBehaviour {

    [SerializeField]private Image displayBoxSprite;//sprite that represents the display box itself
    [SerializeField]private Text playerName;//text that displays the player's name
    [SerializeField]private Text health; //player's health display
    [SerializeField]private Text money; //player's money display
    [SerializeField]private Text ammo;
    [SerializeField]private Text score;//score of player's team

	// Use this for initialization
	void Start () {
		
        if(displayBoxSprite == null)
            Debug.LogWarning("Status Display Controller missing displayBoxSprite reference");

        if (playerName == null)
            Debug.LogWarning("Status Display Controller missing name reference");

        if (health == null)
            Debug.LogWarning("Status Display Controller missing health reference");

        if (money == null)
            Debug.LogWarning("Status Display Controller missing money reference");

        if (ammo == null)
            Debug.LogWarning("Status Display Controller missing ammo reference");

        if (score == null)
            Debug.LogWarning("Status Display Controller missing score reference");
    }

    public void setPlayerName(string pName) {

        playerName.text = pName;
    }

    public void setHealth(int pHealth) {

        health.text = pHealth.ToString();
    }

    public void setMoney(int pMoney) {

        money.text = pMoney.ToString("N");
    }

    public void setAmmo(int pAmmo) {

        ammo.text = pAmmo.ToString();
    }

    public void setScore(int pScore) {

        score.text = pScore.ToString();
    }

    //color the box according to the player's team's color
    public void setTeamColor(TeamProperties.Teams team) {

        displayBoxSprite.color = TeamProperties.teamNamesToColors[team];
    }
}
