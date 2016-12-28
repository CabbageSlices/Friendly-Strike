using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//keeps track of what player is in which team, and stores references to members of each team
//also used by bullets to disable collision with entire teams
public class TeamManager : MonoBehaviour {

    public Dictionary<TeamProperties.Teams, List<PlayerController> > playersInTeams = new Dictionary<TeamProperties.Teams, List<PlayerController> >() {

        {TeamProperties.Teams.red, new List<PlayerController>()},
        {TeamProperties.Teams.green, new List<PlayerController>()},
        {TeamProperties.Teams.blue, new List<PlayerController>()},
        {TeamProperties.Teams.yellow, new List<PlayerController>()},
        {TeamProperties.Teams.black, new List<PlayerController>()},
        {TeamProperties.Teams.white, new List<PlayerController>()},
        {TeamProperties.Teams.cyan, new List<PlayerController>()},
        {TeamProperties.Teams.magenta, new List<PlayerController>()}
    };

    //cache access to the colliders of each player in each team
    public Dictionary<TeamProperties.Teams, List<BoxCollider2D> > collidersForEachTeam = new Dictionary<TeamProperties.Teams, List<BoxCollider2D> >() {

        {TeamProperties.Teams.red, new List<BoxCollider2D>()},
        {TeamProperties.Teams.green, new List<BoxCollider2D>()},
        {TeamProperties.Teams.blue, new List<BoxCollider2D>()},
        {TeamProperties.Teams.yellow, new List<BoxCollider2D>()},
        {TeamProperties.Teams.black, new List<BoxCollider2D>()},
        {TeamProperties.Teams.white, new List<BoxCollider2D>()},
        {TeamProperties.Teams.cyan, new List<BoxCollider2D>()},
        {TeamProperties.Teams.magenta, new List<BoxCollider2D>()}
    };

    //game object that all player's are children of, for organization
    public GameObject playersParent;

    //go through each player in the game and register him into his respective team
    public void registerPlayersIntoChosenTeams() {

        foreach (Transform child in playersParent.transform) {

            PlayerController playerController = child.gameObject.GetComponent<PlayerController>() as PlayerController;

            playersInTeams[playerController.team].Add(playerController);
            collidersForEachTeam[playerController.team].Add(child.gameObject.GetComponent<BoxCollider2D>() as BoxCollider2D);
        }
    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.Z))
            registerPlayersIntoChosenTeams();
	}
}
