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
    private float angleToFireBullets;

    //direction of the target the player is aiming towards relative to the player's arm origin
    //UN-NORMALIZD vector because when the player uses a joystick, this will store the user inputs on each axis
    //give initial value of the right vector since players start off facing to the right
    private Vector2 aimTargetPosition = new Vector2(1, 0);
    
    //object's own components, way to cache the object returned by GetComponent
    [SerializeField]private Rigidbody2D body;
    [SerializeField]new private BoxCollider2D collider;
    [SerializeField]private Animator animator;

    [SerializeField]private EquippedWeaponManager weaponManager;//used to handle weapon control
    [SerializeField]private PlayerBodyParts bodyParts;//body parts of the player

    public float speed = 5;
    public float jumpSpeed = 10; //initial vertical speed the player gets when he presses the jump button
    [Range(10, 100)]public int aimSensitivity = 1; //how fast the aim will jump to player's intended target

    public int controllerNumber;

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

        foreach (string name in Input.GetJoystickNames())
            Debug.Log(name);
    }
	
	// Update is called once per frame
	void Update () {

        handleInput();

        animator.SetFloat(animationHashCodes.jumpVelocityKey, body.velocity.y);
        animator.SetFloat(animationHashCodes.jumpSpeedKey, Mathf.Abs(body.velocity.y));
        animator.SetBool(animationHashCodes.isWalkingKey, body.velocity.x != 0);

        determineSpriteDirection();
        determineSpriteArmOrientation();
    }

    void handleInput() {

        float valueHorizontalAxis = Input.GetAxis("Horizontal" + controllerNumber);

        Vector2 velocity = new Vector2(valueHorizontalAxis * speed, body.velocity.y);

        if (Input.GetButton("Jump" + controllerNumber) && isGrounded()) {

            velocity.y = jumpSpeed;
        }

        if(Input.GetButtonDown("Reload" + controllerNumber) && weaponManager.canReload()) {

            animator.SetTrigger(animationHashCodes.reloadTriggerKey);
        }
        
        if(Input.GetButton("Fire" + controllerNumber) && canFire()) {

            fire();

        } else {

            animator.SetBool(animationHashCodes.fireKey, false);
        }

        calculateVectorToTarget();

        body.velocity = velocity;

        for(int i = 1; i < 9; ++i) {

            if (Input.GetKeyDown((KeyCode)System.Enum.Parse(typeof(KeyCode), "Joystick" + i + "Button0")))
                Debug.Log(i);
        }
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
        weaponManager.fire(angleToFireBullets);
    }

    //flip the player's sprite to the left or right depending on which way he is moving
    void determineSpriteDirection() {

        //when player is moving left, scale body by -1 to make it face left, otherwise make the scale +1 to face right
        Vector3 previousScale = bodyParts.bodyRoot.transform.localScale;

        //multiply scale by -1 to set the correct sign, that way if player was scaled then his size remains the same
        if ((body.velocity.x < 0 && previousScale.x > 0) || (body.velocity.x > 0 && previousScale.x < 0) ) {

            previousScale.x *= -1;
        }

        bodyParts.bodyRoot.transform.localScale = previousScale;
    }

    //calculate direction to the player's target is relative to his arms
    //this is done to determine the orientation of the arms and gun so that it is pointing in the direction the player is aiming
    //returns an UN-NORMALIZED vector
    void calculateVectorToTarget() {

        //for keyboard/mouse user
        if(controllerNumber == 0) {

            aimTargetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) - bodyParts.arms.transform.position;
            return;
        }

        //for controllers
        //don't immediately use the input as the target position because we don't want the player's aim to jump around when he lets go of the control stick
        Vector2 currentAxisValue = new Vector2(Input.GetAxisRaw("AimHorizontal" + controllerNumber), Input.GetAxisRaw("AimVertical" + controllerNumber));

        //ignore values at center of axis that way when player lets go of the stick, the position of the target remains unchanged because
        //it will use the last recorded input values which are all non zero, and the current reading is zero which is ignored
        //this will let the user's aim stay close to what it was before he let go of the stick
        //rawAxis is used because the inputManager's deadzones will output 0 for values at the edge of the deadzone, and i don't want
        //to have values of 0 outside of my deadzone, i want the actual value of the controller.
        float deadzone = 0.15f;
        if(currentAxisValue.sqrMagnitude > deadzone * deadzone)
            aimTargetPosition = new Vector2(Input.GetAxis("AimHorizontal" + controllerNumber), Input.GetAxis("AimVertical" + controllerNumber));
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

        //determine if the arms should be mirrored or not, arms are only mirrored if player is aiming in direction opposite to his arms current orientation
        //they're facing in different directions if their signs are different
        if (bodyParts.arms.transform.lossyScale.x * aimTargetPosition.x < 0) {

            Vector3 scale = bodyParts.arms.transform.localScale;
            scale.x *= -1;

            bodyParts.arms.transform.localScale = scale;
        }

        //calcualte the angle above the horizontal of the arm
        //force angles between [-pi, pi] since mirroring the arm makes the rotation face left or right to get full 2pi radians coverage
        float angle = Mathf.Atan2(aimTargetPosition.y, Mathf.Abs(aimTargetPosition.x));
        angleToFireBullets = Mathf.Atan2(aimTargetPosition.y, aimTargetPosition.x);//this angle is different from the angle used to rotate the arms
        //Since angleToFireBullets is between [-pi, pi] and angle is bewteen [-pi/2, pi/2] AND angle is modified a little bit so the gun is aligned with the line of sight

        angle -= weaponManager.getGunElevationAbovePlayerHands();

        //if user is reloading then disable aiming so it doesn't mess up the reloading animation
        if (animator.GetCurrentAnimatorStateInfo(1).shortNameHash == animationHashCodes.armsReloadingStateKey)
            angle = 0;

        //if the arm is scaled by -1 then you need to multiply the angle by negative 1 because unity will automatically invert the angle when scale is negative
        if (bodyParts.arms.transform.lossyScale.x < 0)
            angle *= -1;

        bodyParts.arms.transform.rotation = Quaternion.Slerp(bodyParts.arms.transform.rotation, Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg), Time.deltaTime*aimSensitivity) ;
        Debug.DrawRay(bodyParts.arms.transform.position, aimTargetPosition, Color.red);

    }

    //checks if the player is standing on some kind of platform
    //if player is grounded then he can jump by pressing the jump key
    bool isGrounded() {

        //player is grounded if there is something below him (distance to object beneath him must be really small)
        //cast a box downwards and if it hits anything then you know player is standing on top of something
        Vector2 boxOrigin = collider.bounds.center;
        boxOrigin.y -= collider.bounds.extents.y;

        Vector2 boxSize = collider.bounds.size;
        boxSize.y = 1.0f / 64.0f;

        //disable player collider beforehand so boxcast doesn't return player
        collider.enabled = false;

        float boxAngle = 0;
        RaycastHit2D objectBelowPlayer = Physics2D.BoxCast(boxOrigin, boxSize, boxAngle, Vector2.down, isGroundedBoxCastDistance);

        collider.enabled = true;

        return objectBelowPlayer.collider != null;
    }

    void OnDrawGizmos() {

        Vector2 boxOrigin = collider.bounds.center;
        boxOrigin.y -= collider.bounds.extents.y;

        Vector2 boxSize = collider.bounds.size;
        boxSize.y = 1.0f / 64.0f;

        Gizmos.DrawWireCube(boxOrigin, boxSize);
    }
}
