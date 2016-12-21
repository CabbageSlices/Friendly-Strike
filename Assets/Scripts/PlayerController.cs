using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    //keys used by the animator to access the parameters used to change animation states
    class AnimationHashCodes {

        public int isWalkingKey = Animator.StringToHash("IsWalking");
    }

    //isGrounded does a boxcast beneath the player to check for platforms
    //this distance is the maximum distance to cast the box
    private float isGroundedBoxCastDistance = 0.1f;
    private AnimationHashCodes animationHashCodes = new AnimationHashCodes();
    
    //object's own components, way to cache the object returned by GetComponent
    [SerializeField]private Rigidbody2D body;
    [SerializeField]new private BoxCollider2D collider;
    [SerializeField]private Animator animator;

    [SerializeField]private GameObject arms;//the player's arms, used to aim the arms towards the target location
    [SerializeField]private GameObject playerBodySprite;//object that contains the sprite heirachy for the player, used to flip the player's body when facing different direction

    public float speed = 5;
    public float jumpSpeed = 10; //initial vertical speed the player gets when he presses the jump button

	void Start () { 

        if (body == null)
            Debug.LogWarning("playerController script has no Rigidbody2D reference (body is null)");

        if (collider == null)
            Debug.LogWarning("playerController script has no BoxCollider2D reference (collider is null)");
        
        if (animator == null)
            Debug.LogWarning("playerController script has no Animator reference (animator is null)");

        if (arms == null)
            Debug.LogWarning("playerController script is missing a GameObject reference (arms is null)");

        if (playerBodySprite == null)
            Debug.LogWarning("playerController script is missing a GameObject reference (playerBodySprite is null)");

    }
	
	// Update is called once per frame
	void Update () {

        handleInput();

        animator.SetBool(animationHashCodes.isWalkingKey, body.velocity.x != 0);
    }

    void handleInput() {

        float valueHorizontalAxis = Input.GetAxis("Horizontal");

        Vector2 velocity = new Vector2(valueHorizontalAxis * speed, body.velocity.y);

        //when player is moving left, scale body by -1 to make it face left, otherwise make the scale +1 to face right
        if(valueHorizontalAxis < 0) {

            playerBodySprite.transform.localScale = new Vector3(-1, 1, 1);

        } else if(valueHorizontalAxis > 0) {

            playerBodySprite.transform.localScale = new Vector3(1, 1, 1);
        }

        if (Input.GetButton("Jump") && isGrounded()) {

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

        RaycastHit2D objectBelowPlayer = Physics2D.BoxCast(boxOrigin, boxSize, boxAngle, Vector2.down, isGroundedBoxCastDistance);

        collider.enabled = true;

        return objectBelowPlayer.collider != null;
    }
}
