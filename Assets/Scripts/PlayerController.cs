using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    //isGrounded does a boxcast beneath the player to check for platforms
    //this distance is the maximum distance to cast the box
    private float isGroundedBoxCastDistance = 0.1f;

    //rigid body to handle movement
    [SerializeField]private Rigidbody2D body;
    [SerializeField]new private BoxCollider2D collider;

    public float speed = 5;
    public float jumpSpeed = 10; //initial vertical speed the player gets when he presses the jump button

	void Start () {

        //make sure all components are stored
        body = GetComponent<Rigidbody2D>() as Rigidbody2D;
        collider = GetComponent<BoxCollider2D>() as BoxCollider2D;
	}
	
	// Update is called once per frame
	void Update () {

        //handle input
        Vector2 velocity = new Vector2(Input.GetAxis("Horizontal") * speed, body.velocity.y);

        if(Input.GetButton("Jump") && isGrounded()) {

            velocity.y = jumpSpeed;
        }

        body.velocity = velocity;
    }
    //checks if the player is standing on some kind of platform
    //if player is grounded then he can jump by pressing the jump key
    bool isGrounded() {

        //player is grounded if there is something below him (distance to object beneath him must be really small)
        //cast a box downwards and if it hits anything then you know player is standing on top of something
        Vector2 boxOrigin = collider.bounds.center;
        Vector2 boxSize = collider.bounds.size;
        float boxAngle = 0;

        //disable player collider beforehand so boxcast doesn't return player
        collider.enabled = false;

        RaycastHit2D objectBelowPlayer = Physics2D.BoxCast(boxOrigin, boxSize, 0, Vector2.down, isGroundedBoxCastDistance);

        collider.enabled = true;

        return objectBelowPlayer.collider != null;
    }
}
