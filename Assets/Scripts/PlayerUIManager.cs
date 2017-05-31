using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//class that assigns status display boxes to players
public class PlayerUIManager : MonoBehaviour {

    //used to assign display boxes to one team at a time so all players of the same team are grouped together
    public TeamManager teamManager;

    //parent game object ot all players
    GameObject playersParent;

    //go through each player and give them their status display box and shop ui
    //player's are added as children to the playersParent gameobject in the order that they registered themselves
    //so the first player is player 1, second player is player 2, etc..
    //assign ui  components so that player1 status display box / shop goes to player1, and so on
    public void assignUIToPlayers() {

        //get all the status display boxes and shop componetns
        //then assign them to their corresponding player
        StatusDisplayBoxController[] statusDisplayBoxes = gameObject.GetComponentsInChildren<StatusDisplayBoxController>(true);
        ShopController[] shops = gameObject.GetComponentsInChildren<ShopController>(true);

        for (int i = 0; i < playersParent.transform.childCount; ++i) {

            PlayerController player = playersParent.transform.GetChild(i).gameObject.GetComponent<PlayerController>();

            //enable shop ui and status display box for this playerr
            statusDisplayBoxes[i].gameObject.SetActive(true);
            shops[i].gameObject.SetActive(true);

            player.assignStatusDisplayBox(statusDisplayBoxes[i]);
            shops[i].assignPlayer(player);
        }

        //disable all unused display boxes
        for(int i = playersParent.transform.childCount; i < transform.childCount; ++i) {

            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

	// Use this for initialization
	void Start () {

        playersParent = GameObject.FindWithTag("TeamManager");
        teamManager = playersParent.GetComponent<TeamManager>() as TeamManager;

        if (teamManager == null)
            Debug.LogWarning("PlayerUIManager script missing teamManager reference");

        if (playersParent== null)
            Debug.LogWarning("PlayerUIManager script missing playersParent reference");

        //DO NOT ASSIGN A DISPLAY BOX IN THE START FUNCTION
        //NOT ALL GAME OBJECTS ARE GUARANTEED TO HAVE RUN THEIR START ROUTINE
        //this means that player's stats may not be initialized, (such as teams or health), and the status display box will be missing information
        //do this in the game controllers startUp state
        //assignStatusDisplayBoxesToPlayers();
    }
}
