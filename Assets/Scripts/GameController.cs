using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class that manages the various game states
//handles match setup/takedown, assigning scores, and switching between gameplay and shopping
public class GameController : MonoBehaviour {

    enum States {

        StartUp,
        Shopping,
        Gameplay,
        PostRoundEvaluation,
        Results,
        ShutDown
    };

    States currentState = States.StartUp;

	// Use this for initialization
	void Start () {
		
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
