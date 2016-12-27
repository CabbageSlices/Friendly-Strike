using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    //keys used by the animator to access the parameters used to change animation states
    class AnimationHashCodes {

        public int armsAimingStateKey = Animator.StringToHash("Aiming");//aiming state in any layer
        public int armsReloadingStateKey = Animator.StringToHash("Reload");

        public int pistolFireStateKey = Animator.StringToHash("ArmsPistol.Fire");//firing state in pistol arms layer
        public int pistolReloadingStateKey = Animator.StringToHash("ArmsPistol.Reload");//reloading state in pistol arms layer
        public int pistolAimingStateKey = Animator.StringToHash("ArmsPistol.Aiming");//aiming state in pistol arms layer

        public int fireKey = Animator.StringToHash("Fire");
        public int reloadTriggerKey = Animator.StringToHash("Reload");
        public int isWalkingKey = Animator.StringToHash("IsWalking");
        public int jumpVelocityKey = Animator.StringToHash("JumpVelocity"); //velocity is used to determine if jumpUp animatin should be used or jumpDown animation
        public int jumpSpeedKey = Animator.StringToHash("JumpSpeed"); //speed is used to transition from jump up to jump down animations (when speed reaches 0 it's transitioning)
    }

    //isGrounded does a boxcast beneath the player to check for platforms
    //this distance is the maximum distance to cast the box
    private float isGroundedBoxCastDistance = 0.1f;
    private AnimationHashCodes animationHashCodes = new AnimationHashCodes();

    //angle between the player and the target he is aiming at
    //used to fire a bullet IN RADIANS
    private float angleToAimTarget;
    
    //object's own components, way to cache the object returned by GetComponent
    [SerializeField]private Rigidbody2D body;
    [SerializeField]new private BoxCollider2D collider;
    [SerializeField]private Animator animator;

    [SerializeField]private EquippedWeaponManager weaponManager;//used to handle weapon control
    [SerializeField]private PlayerBodyParts bodyParts;//body parts of the player

    public float speed = 5;
    public float jumpSpeed = 10; //initial vertical speed the player gets when he presses the jump button

	void Start () {
        
        if (body == null)
            Debug.LogWarning("playerController script has no Rigidbody2D reference (body is null)");

        if (collider == null)
            Debug.LogWarning("playerController script has no BoxCollider2D reference (collider is null)");
        
        if (animator == null)
            Debug.LogWarning("playerController script has no Animator reference (animator is null)");

        if (weaponManager == null)
            Debug.LogWarning("playerController script has no EquippedWeaponManager reference (weaponManager is null)");

        if (bodyParts == null)
            Debug.LogWarning("playercontroller missing bodyParts refernece");
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
        }

        if(Input.GetButtonDown("Reload0") && weaponManager.canReload()) {

            animator.SetTrigger(animationHashCodes.reloadTriggerKey);
        }
        
        if(Input.GetButton("Fire0") && canFire()) {

            fire();

        } else {

            animator.SetBool(animationHashCodes.fireKey, false);
        }

        body.velocity = velocity;
    }

    bool canReload() {

        return weaponManager.canReload() && animator.GetCurrentAnimatorStateInfo(1).shortNameHash == animationHashCodes.armsAimingStateKey;
    }

    bool canFire() {

        return animator.GetCurrentAnimatorStateInfo(1).fullPathHash != animationHashCodes.pistolReloadingStateKey && weaponManager.canFire();
    }

    //plays the fire animation, creates a bullet
    void fire() {

        animator.SetBool(animationHashCodes.fireKey, true);
        weaponManager.fire(angleToAimTarget);
    }

    //flip the player's sprite to the left or right depending on which way he is moving
    void determineSpriteDirection(float valueHorizontalInputAxis) {

        //when player is moving left, scale body by -1 to make it face left, otherwise make the scale +1 to face right
        Vector3 previousScale = bodyParts.bodyRoot.transform.localScale;

        //multiply scale by -1 to set the correct sign, that way if player was scaled then his size remains the same
        if ((valueHorizontalInputAxis < 0 && previousScale.x > 0) || (valueHorizontalInputAxis > 0 && previousScale.x < 0) ) {

            previousScale.x *= -1;
        }

        bodyParts.bodyRoot.transform.localScale = previousScale;
    }

    //rotates the arms so that they're pointing towards the target
    //flips the arms horizontally if target is to the left or right of the player
    //this will also take the body's current direction into account so that if player is already facing left and target is to the left, the arms won't be mirrored
    void determineSpriteArmOrientation() {

        //reset rotation so all calculations can be made with the player pointing gun stragith to the right
        //arms.transform.rotation = Quaternion.Euler(0, 0, 0);

        //need to rotate arm so that player's line of sight is in line with the vector from the player's arm to the target position
        //player's line of sight vector is defined as the vector from the player's arm to the tip of the gun barrel.
        //first find the angle from the player's arm to the target
        //since the gun's barrel is located slightly above the player's hands, there is a small difference in angle between
        //the line of sight vector and the vector from the player's arm to the target position.
        //so add the small angle to the angle between the arm to the target to get the angle for hte line of sight            

        //first get the angle from arm to target
        //position of the target that the player is aiming at, relative to the playe'rs arm
        Vector2 targetPositionRelativeToPlayer = Camera.main.ScreenToWorldPoint(Input.mousePosition) - bodyParts.arms.transform.position;// arms.transform.position;

        //determine if the arms should be mirrored or not, arms are only mirrored if player is aiming in direction opposite to his arms current orientation
        //they're facing in different directions if their signs are different
        if (bodyParts.arms.transform.lossyScale.x * targetPositionRelativeToPlayer.x < 0) {

            Vector3 scale = bodyParts.arms.transform.localScale;
            scale.x *= -1;

            bodyParts.arms.transform.localScale = scale;
        }

        //calcualte the angle above the horizontal of the arm
        //force angles between [-pi, pi] since mirroring the arm makes the rotation face left or right to get full 2pi radians coverage
        float angle = Mathf.Atan2(targetPositionRelativeToPlayer.y, Mathf.Abs(targetPositionRelativeToPlayer.x));
        angleToAimTarget = Mathf.Atan2(targetPositionRelativeToPlayer.y, targetPositionRelativeToPlayer.x);//this angle is different from the angle used to rotate the arms
        //Since angleToAimTarget is between [-pi, pi] and angle is bewteen [-pi/2, pi/2] AND angle is modified a little bit so the gun is aligned with the line of sight

        //now find the angle between the line of sight and the vector to the hand
        //angle between the vector from the arm to the hand and the line of sight vector
        float angleOffset = 0;

        //if player isn't holding gun then no need to modify angle
        if (weaponManager.getPartOfGunToAim() != null) {

            Vector2 armToBarrelTip = weaponManager.getPartOfGunToAim().transform.position - bodyParts.arms.transform.position;
            Vector2 armToHandTip = bodyParts.rightHand.transform.position - bodyParts.arms.transform.position;

            float cosAngle = Vector2.Dot(armToHandTip.normalized, armToBarrelTip.normalized);
            angleOffset = Mathf.Acos(cosAngle) ;

            Debug.DrawRay(bodyParts.arms.transform.position, armToBarrelTip, Color.green);
            
        }
        
        //divide by two because it makes it align properly, don't really know why
        angle -= angleOffset/2;

        //if user is reloading then disable aiming so it doesn't mess up the reloading animation
        if (animator.GetCurrentAnimatorStateInfo(1).fullPathHash == animationHashCodes.pistolReloadingStateKey)
            angle = 0;

        //if the arm is scaled by -1 then you need to multiply the angle by negative 1 because unity will automatically invert the angle when scale is negative
        if (bodyParts.arms.transform.lossyScale.x < 0)
            angle *= -1;

        bodyParts.arms.transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
        Debug.DrawRay(bodyParts.arms.transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition) - bodyParts.arms.transform.position, Color.red);

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
