using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class that manages the various game states
//handles match setup/takedown, assigning scores, and switching between gameplay and shopping
public class GameController : MonoBehaviour {

    enum States {

        NULL,
        StartUp,
        Shopping,
        Gameplay,
        PostRoundEvaluation,
        Results,
        ShutDown
    };

    public TeamManager teamManager;
    public GameObject playersParent;

    //minimum amount of money players will receive every round
    public int basePayoutPerRound;

    //winners bonus for winning a round
    public int roundWinnersBonus;

    //how long to wait to end a round once all teams but one are annhiliated
    //in seconds
    public float roundEndDelay;

    public StatusDisplayManager statusDisplayManager;

    List<PlayerController> players = new List<PlayerController>();

    States currentState = States.NULL;

	// Use this for initialization
	void Start () {
		
        if(teamManager == null)
            Debug.LogWarning("teamManager reference in GameController is null");

        if(playersParent == null)
            Debug.LogWarning("PlayerParent reference in GameController is null");

        if (statusDisplayManager == null)
            Debug.LogWarning("statusDisplayManager reference in GameController is null");

        getReferenceToPlayers();
	}
	
	// Update is called once per frame
	void Update () {
		
        if(currentState == States.NULL) {

            enterState(States.StartUp);
            enterState(States.Gameplay);
        }
	}

    void getReferenceToPlayers() {

        var playerControllerComponents = playersParent.GetComponentsInChildren<PlayerController>();

        //add all playercontrollers to the list of players
        foreach(var component in playerControllerComponents) {

            players.Add(component as PlayerController);
        }
    }

    void enterState(States next) {

        if(currentState == States.StartUp) {

            statusDisplayManager.assignStatusDisplayBoxesToPlayers();
        }

        if(currentState == States.Gameplay) {

            //idk do something here to start the match, maybe post a START image or something
            //startImage.enabled = true;
        }

        currentState = next;
    }

    //exits the current state and returns the next state that should be entered
    States exitState() {

        States nextState = States.StartUp;

        if(currentState == States.Gameplay) {

            //round ended, assign money and scores
            assignScoreToTeams();
            givePlayersMoney();

            //respawn players into random locations
            respawnPlayers();

            //go to shopping if match is still ongoing, go to winners screen if match ended
            nextState = States.Gameplay;
        }

        return nextState;
    }

    //exit current state  after waiting the given delay
    //exit delay must be in seconds
    //callback is a substitue for the function return value
    //the argument passed to callback will be a return value specifying what state to transition to after the current state is finished
    IEnumerator delayedExitState(float exitDelay, System.Action<States> callback) {

        States nextState = States.StartUp;

        if(currentState == States.Gameplay)
            yield return new WaitForSeconds(exitDelay);

        nextState = exitState();

        callback(nextState);
    }

    void assignScoreToTeams() {

        teamManager.addScoreToTeamWithLivingPlayers();
    }

    void givePlayersMoney() {

        //loop through all the players andd calculate how much they earned from the last round
        foreach(PlayerController player in players) {

            int payout = calculatePayoutForPlayer(player);
            player.receiveMoney(payout);
        }
    }

    void respawnPlayers() {

        foreach(PlayerController player in players) {

            player.respawn();
        }
    }

    //calculate how much money to pay the eplayer at the end of the current round
    //round should be finished by now
    int calculatePayoutForPlayer(PlayerController player) {
        
        int payOut = basePayoutPerRound;
        payOut += player.isAlive() ? roundWinnersBonus : 0;

        return payOut;
    }

    //called by players whenever they die
    public void onPlayerDeath() {

        //shouldn't happen outside of gameplay state
        if(currentState != States.Gameplay)
            Debug.LogWarning("onPlayerrDeath() called in state " + currentState.ToString());

        //if there is only 1 team, or less, left with living players then match has ended
        if(teamManager.getNumberOfTeamsWithLivingPlayers() <= 1) {
            
            //wait before exiting the gameplay state
            StartCoroutine(delayedExitState(roundEndDelay, (nextState) => {
                enterState(nextState);}
            ));
        }
    }
}
