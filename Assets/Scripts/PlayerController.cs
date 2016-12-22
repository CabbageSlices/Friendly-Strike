using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    //keys used by the animator to access the parameters used to change animation states
    class AnimationHashCodes {

        public int isWalkingKey = Animator.StringToHash("IsWalking");
        public int jumpVelocityKey = Animator.StringToHash("JumpVelocity"); //velocity is used to determine if jumpUp animatin should be used or jumpDown animation
        public int jumpSpeedKey = Animator.StringToHash("JumpSpeed"); //speed is used to transition from jump up to jump down animations (when speed reaches 0 it's transitioning)
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

        animator.SetFloat(animationHashCodes.jumpVelocityKey, body.velocity.y);
        animator.SetFloat(animationHashCodes.jumpSpeedKey, Mathf.Abs(body.velocity.y));
        animator.SetBool(animationHashCodes.isWalkingKey, body.velocity.x != 0);
    }

    void handleInput() {

        float valueHorizontalAxis = Input.GetAxis("Horizontal0");

        determineSpriteDirection(valueHorizontalAxis);
        determineSpriteArmOrientation();

        Vector2 velocity = new Vector2(valueHorizontalAxis * speed, body.velocity.y);

        if (Input.GetButton("Jump0") && isGrounded()) {

            velocity.y = jumpSpeed;

            //rest jump animations since he is now grounded
            //if user holds jump button then jump animations never stop because animator never has a chance to set jumpspeed to 0 so he is always in jump state
            animator.SetFloat(animationHashCodes.jumpVelocityKey, 0);
            animator.SetFloat(animationHashCodes.jumpSpeedKey, 0);
        }

        body.velocity = velocity;
    }

    //flip the player's sprite to the left or right depending on which way he is moving
    void determineSpriteDirection(float valueHorizontalInputAxis) {

        //when player is moving left, scale body by -1 to make it face left, otherwise make the scale +1 to face right
        Vector3 previousScale = playerBodySprite.transform.localScale;

        //multiply scale by -1 to set the correct sign, that way if player was scaled then his size remains the same
        if ((valueHorizontalInputAxis < 0 && previousScale.x > 0) || (valueHorizontalInputAxis > 0 && previousScale.x < 0) ) {

            previousScale.x *= -1;
        }

        playerBodySprite.transform.localScale = previousScale;
    }

    //rotates the arms so that they're pointing towards the target
    //flips the arms horizontally if target is to the left or right of the player
    //this will also take the body's current direction into account so that if player is already facing left and target is to the left, the arms won't be mirrored
    void determineSpriteArmOrientation() {

        Vector2 mousePositionRelativeToPlayer = Camera.main.ScreenToWorldPoint(Input.mousePosition) - arms.transform.position;

        //determine if the arms should be mirrored or not, arms are only mirrored if player is aiming in direction opposite to his arms current orientation
        //they're facing in different directions if their signs are different
        if (arms.transform.lossyScale.x * mousePositionRelativeToPlayer.x < 0) {

            Vector3 scale = arms.transform.localScale;
            scale.x *= -1;

            arms.transform.localScale = scale;
        }

        //calcualte the angle above the horizontal of the arm
        //force angles between [-pi, pi] since mirroring the arm makes the rotation face left or right to get full 2pi radians coverage
        float angle = Mathf.Atan2(mousePositionRelativeToPlayer.y, Mathf.Abs(mousePositionRelativeToPlayer.x));

        //if the arm is scaled by -1 then you need to multiply the angle by negative 1 because unity will automatically invert the angle when scale is negative
        if (arms.transform.lossyScale.x < 0)
            angle *= -1;

        arms.transform.rotation = Quaternion.Euler(0, 0, angle * 180 / 3.14159265f);
        
        Physics2D.Raycast(arms.transform.position, arms.transform.rotation * Vector3.right);
        Debug.DrawRay(arms.transform.position, arms.transform.rotation * Vector3.right);
        Debug.DrawRay(arms.transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition) - arms.transform.position, Color.red);
        
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
