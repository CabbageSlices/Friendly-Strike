﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
        public int IsGroundedKey = Animator.StringToHash("IsGrounded");
    }

    //isGrounded does a boxcast beneath the player to check for platforms
    //this distance is the maximum distance to cast the box
    private float isGroundedBoxCastDistance = 0.2f;
    private AnimationHashCodes animationHashCodes = new AnimationHashCodes();

    //angle between the player and the target he is aiming at
    //used to fire a bullet IN RADIANS
    private float angleToFireBullets;

    //direction of the target the player is aiming towards relative to the player's arm origin
    //UN-NORMALIZD vector because when the player uses a joystick, this will store the user inputs on each axis
    //give initial value of the right vector since players start off facing to the right
    private Vector2 aimTargetPosition = new Vector2(1, 0);

    //copy of body that is used to create an explosion effect when player dies
    GameObject bodyCloneForDeathEffect;

    //keeps track of wether the player pressed the jump button and is jumping
    private bool isJumping = false;
    
    //which direction the player will move in if he presses the left or right button
    //this will change when the player stands on different slopes in order to becoem parallel to the slope
    //when he isn't standing on anything or standing on a flat surface the local axis should align with the global right axis
    private Vector2 localHorizontalDirection = new Vector2(1, 0);

    //object's own components, way to cache the object returned by GetComponent
    [SerializeField]private Rigidbody2D body;
    [SerializeField]new private BoxCollider2D collider;
    [SerializeField]private Animator animator;

    [SerializeField]private EquippedWeaponManager weaponManager;//used to handle weapon control
    [SerializeField]private PlayerBodyParts bodyParts;//body parts of the player

    public float speed = 5;
    public float jumpSpeed = 10; //initial vertical speed the player gets when he presses the jump button
    [Range(10, 100)]public int aimSensitivity = 1; //how fast the aim will jump to player's intended target

    //health manager reference
    public HealthManager healthManager;

    //id of the controller used to control this player
    //0 represents keyboard/mouse, all numbers bigger than 0 represent a joypad
    public int controllerId;

    //player's team id
    public TeamProperties.Teams team;

    //layers that contain objects that the player can use to jump on top of
    //this is basically the layers that the physics2D can raycast against when checking if player can jump, or if player is standing on a platform
    public LayerMask raycastLayers;

    //debugging
    public float timeScale;

    private float defaultGravity;

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

        if (healthManager == null)
            Debug.LogWarning("PlayerController missing healthManager reference");

        Time.timeScale = timeScale;

        defaultGravity = body.gravityScale;
    }

    void OnEnable() {

        subscribeToEvents();
    }

    void OnDisable() {

        unsubscribeFromEvents();
    }

    void subscribeToEvents() {

        healthManager.onZeroHealth += die;
        healthManager.onHealthRestore += revive;
    }

    void unsubscribeFromEvents() {

        healthManager.onZeroHealth -= die;
        healthManager.onHealthRestore -= revive;
    }
	
	// Update is called once per frame
	void Update () {

        bool isGroundedCached = isGrounded();
        handleInput(isGroundedCached);

        if (!isGroundedCached || isJumping)
            body.gravityScale = defaultGravity;

        Debug.Log(isGroundedCached + "    " + isJumping);

        animator.SetFloat(animationHashCodes.jumpVelocityKey, body.velocity.y);
        animator.SetBool(animationHashCodes.isWalkingKey, body.velocity.x != 0);
        animator.SetBool(animationHashCodes.IsGroundedKey, isGroundedCached && !isJumping);

        determineSpriteDirection();
        determineSpriteArmOrientation();
    }

    void handleInput(bool isGroundedCached) {

        float valueHorizontalAxis = Input.GetAxis("Horizontal" + controllerId);

        Vector2 velocity = new Vector2(0, body.velocity.y);

        //when player is grounded and not jumping we don't need a y velocity since he is on the ground and isn't trying to go upwards
        //but if he is falling/jumping then we don't want to override the y velocity because he will stop midair
        if (isGroundedCached && !isJumping)
            velocity.y = 0;

        //calculate the velocity player should have when moving across the surface of the ground he is standing on
        Vector2 horizontalVelocity = localHorizontalDirection * valueHorizontalAxis * speed;

        velocity += horizontalVelocity;

        if (Input.GetButton("Jump" + controllerId) && isGroundedCached) {

            isJumping = true;
            velocity.y = jumpSpeed;
        }

        if(Input.GetButtonDown("Reload" + controllerId) && weaponManager.canReload()) {

            animator.SetTrigger(animationHashCodes.reloadTriggerKey);
        }
        
        if(Input.GetButton("Fire" + controllerId) && canFire()) {

            fire();

        } else {

            animator.SetBool(animationHashCodes.fireKey, false);
        }

        calculateVectorToTarget();

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
        weaponManager.fire(angleToFireBullets, team);
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
        if(controllerId == 0) {

            aimTargetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) - bodyParts.arms.transform.position;
            return;
        }

        //for controllers
        //don't immediately use the input as the target position because we don't want the player's aim to jump around when he lets go of the control stick
        Vector2 currentAxisValue = new Vector2(Input.GetAxisRaw("AimHorizontal" + controllerId), Input.GetAxisRaw("AimVertical" + controllerId));

        //ignore values at center of axis that way when player lets go of the stick, the position of the target remains unchanged because
        //it will use the last recorded input values which are all non zero, and the current reading is zero which is ignored
        //this will let the user's aim stay close to what it was before he let go of the stick
        //rawAxis is used because the inputManager's deadzones will output 0 for values at the edge of the deadzone, and i don't want
        //to have values of 0 outside of my deadzone, i want the actual value of the controller.
        float deadzone = 0.15f;
        if(currentAxisValue.sqrMagnitude > deadzone * deadzone)
            aimTargetPosition = new Vector2(Input.GetAxis("AimHorizontal" + controllerId), Input.GetAxis("AimVertical" + controllerId));
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
        boxSize.y = 1.0f / 64.0f;//1 unit height

        //disable player collider beforehand so boxcast doesn't return player
        collider.enabled = false;

        float boxAngle = 0;
        RaycastHit2D objectBelowPlayer = Physics2D.BoxCast(boxOrigin, boxSize, boxAngle, Vector2.down, isGroundedBoxCastDistance, raycastLayers.value);

        collider.enabled = true;

        if (objectBelowPlayer.collider != null)
            localHorizontalDirection = new Vector2(objectBelowPlayer.normal.y, -objectBelowPlayer.normal.x);
        else
            localHorizontalDirection = new Vector2(1, 0);

        return objectBelowPlayer.collider != null;
    }

    //make a death body part explosion effect
    void die() {

        //first make a copy of all of the player's body parts
        GameObject bodyCopy = GameObject.Instantiate(bodyParts.bodyRoot, bodyParts.bodyRoot.transform.position, bodyParts.bodyRoot.transform.rotation) as GameObject;
        bodyCopy.GetComponent<Animator>().enabled = false; //disable animations so explosion effect can happen

        bodyCopy.transform.localScale = new Vector3(2, 2, 2);

        //now find all body parts that have a sprite renderer and are tagged player, since these are the parts that can make up the explosion effect
        Component[] childrenWithSprites = bodyCopy.GetComponentsInChildren<SpriteRenderer>();
        
        foreach(Component component in childrenWithSprites) {

            //ignore anything not tagged player because it could be some other children like a gun or something
            if (!component.gameObject.CompareTag("PlayerBodyPart"))
                continue;

            Rigidbody2D cloneRigidBody = component.gameObject.GetComponent<Rigidbody2D>() as Rigidbody2D;

            //player body part found, in order to apppy explosion effect we need to enable the rigidbody and collider
            cloneRigidBody.isKinematic = false;
            (component.gameObject.GetComponent<BoxCollider2D>() as BoxCollider2D).enabled = true;

            //push this for explosion effect, all parts should go in some random direction
            cloneRigidBody.velocity = Vector2.up * Random.Range(0, 20) + Vector2.right * Random.Range(-20, 20);
            cloneRigidBody.angularVelocity = Random.Range(-90, 90);
        }

        //disable self so player is hidden
        gameObject.SetActive(false);
    }

    public void revive() {

        gameObject.SetActive(true);

        if (bodyCloneForDeathEffect != null)
            GameObject.Destroy(bodyCloneForDeathEffect);

        bodyCloneForDeathEffect = null;
    }

    void OnDrawGizmos() {

        Vector2 boxOrigin = collider.bounds.center;
        boxOrigin.y -= collider.bounds.extents.y;

        Vector2 boxSize = collider.bounds.size;
        boxSize.y = 1.0f / 64.0f;//1 unit height

        Gizmos.DrawCube(boxOrigin, boxSize);
        Vector2 length = new Vector2(0, boxSize.y + isGroundedBoxCastDistance);
        Debug.DrawLine(boxOrigin, boxOrigin - length, Color.green);
    }

    void OnCollisionEnter2D(Collision2D collision) {

        //if the colliding object isn't soemthign the player can stand on, then ignore it
        if( ((1 << collision.gameObject.layer) & raycastLayers.value) == 0) 
            return;

        isJumping = false;
        body.gravityScale = 0;
    }
}
