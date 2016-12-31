using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class control : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
        Rigidbody2D body = GetComponent<Rigidbody2D>() as Rigidbody2D;

        body.velocity = new Vector2(Input.GetAxis("Horizontal0") * 10, body.velocity.y);
	}
}
