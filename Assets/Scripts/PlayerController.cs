using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerAnimationController))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerController : MonoBehaviour {

    enum States {

        Grounded,
        Jumping,
        Falling,
        Dead
    }

    //isGrounded does a boxcast beneath the player to check for platforms
    //this distance is the maximum distance to cast the box
    private float isGroundedBoxCastDistance = 0.2f;

    //angle between the player and the target he is aiming at
    //used to fire a bullet, IN RADIANS
    private float angleToFireBullets;

    //direction of the target the player is aiming towards relative to the player's arm origin
    //UN-NORMALIZD vector because when the player uses a joystick, this will store the user inputs on each axis
    //give initial value of the right vector since players start off facing to the right
    //used to line up the guns line of sight with the player's line of sight to the target
    private Vector2 aimTargetPosition = new Vector2(1, 0);

    //copy of body that is used to create an explosion effect when player dies
    //when player dies he will explode into several pieces, and this gameobject will be the parent of all of the body parts,
    //that way we can delete this clone to remove all pieces
    GameObject bodyCloneForDeathEffect;

    //which direction the player will move in if he presses the left or right button
    //this will change when the player stands on different slopes in order to becoem parallel to the slope
    //when he isn't standing on anything or standing on a flat surface the local axis should align with the global right axis
    private Vector2 localHorizontalDirection = new Vector2(1, 0);

    //default player gravity that is set in the editor
    //when player lands on a platform we need to set his gravity to 0 otherwise he will slide down slopes
    //when player jumps or falls off a platform we reset gravity to whatever it was initially
    private float defaultGravity;

    States currentState = States.Falling;

    [SerializeField]
    private PlayerComponentReferences componentReferences;

    [SerializeField]
    public PlayerGameplayProperties gameplayProperties;

    //health manager reference
    public HealthManager healthManager;

    //reference to the player's status display box
    public StatusDisplayBoxController statusDisplayBox;

    //layers that contain objects that the player can use to jump on top of
    //this is basically the layers that the physics2D can raycast against when checking if player can jump, or if player is standing on a platform
    public LayerMask raycastLayers;

    public GameController gameController;

    void Start() {

        setupReferences();

        defaultGravity = componentReferences.body.gravityScale;
        componentReferences.inputHandler.setControllerId(gameplayProperties.controllerId);
    }

    void setupReferences() {

        componentReferences.body = GetComponent<Rigidbody2D>() as Rigidbody2D;
        componentReferences.collider = GetComponent<BoxCollider2D>() as BoxCollider2D;
        componentReferences.weaponManager = GetComponentInChildren<EquippedWeaponManager>() as EquippedWeaponManager;
        componentReferences.bodyParts = GetComponent<PlayerBodyParts>() as PlayerBodyParts;
        
        healthManager = GetComponentInChildren<HealthManager>() as HealthManager;

        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>() as GameController;
    }

    void OnEnable() {

        subscribeToEvents();
    }

    void OnDisable() {

        unsubscribeFromEvents();
    }

    void enterState(States next) {

        if (next == States.Falling || next == States.Jumping) {

            componentReferences.body.gravityScale = defaultGravity;
        }

        if (next == States.Jumping) {
            
            componentReferences.body.velocity = new Vector2(componentReferences.body.velocity.x, gameplayProperties.jumpSpeed);
        }

        if(next == States.Grounded) {

            componentReferences.body.gravityScale = 0;
        }

        if(next == States.Dead) {

            createBodyCloneForDeath();

            //disable player body so it isn't drawn, don't disable player game object  because we need healthbar to animate
            //once player healthbar reaches 0 we can stop drawing player
            componentReferences.bodyParts.bodyRoot.SetActive(false);

            //disable collider so he can't get shot again until he respawns
            componentReferences.collider.enabled = false;

            //disable rigid body so player won't move
            componentReferences.body.simulated = false;

            //gameObject.SetActive(false);
            gameController.onPlayerDeath();
        }

        currentState = next;
    }
    
    void exitState() {
        
        if(currentState == States.Dead) {

            componentReferences.collider.enabled = true;
            componentReferences.body.simulated = true;
            componentReferences.bodyParts.bodyRoot.SetActive(true);
            //gameObject.SetActive(true);
            healthManager.restoreHealth();

            if (bodyCloneForDeathEffect != null)
                GameObject.Destroy(bodyCloneForDeathEffect);

            bodyCloneForDeathEffect = null;
        }
    }

    void changeState(States toChange) {

        exitState();
        enterState(toChange);
    }

    void subscribeToEvents() {

        healthManager.onZeroHealth += die;
        TeamScoreManager.onTeamScoreChange += handleTeamScoreChange;

        componentReferences.inputHandler.onJumpPress += jump;
        componentReferences.inputHandler.onFirePress += fire;
        componentReferences.inputHandler.onFireRelease += releaseFireButton;
        componentReferences.inputHandler.onReloadPress += reload;
        componentReferences.weaponManager.onEquipWeapon += componentReferences.animationController.useAnimationForGun;

        subscribeStatusDisplayBoxToEvents();
    }

    //make status display box track different events so that the values in the box are updated automatically
    void subscribeStatusDisplayBoxToEvents() {

        if (statusDisplayBox == null)
            return;

        healthManager.onHealthChange += statusDisplayBox.setHealth;
        componentReferences.weaponManager.onAmmoChange += statusDisplayBox.setAmmo;
    }

    void unsubscribeFromEvents() {

        healthManager.onZeroHealth -= die;
        TeamScoreManager.onTeamScoreChange -= handleTeamScoreChange;

        componentReferences.inputHandler.onJumpPress -= jump;
        componentReferences.inputHandler.onFirePress -= fire;
        componentReferences.inputHandler.onFireRelease -= releaseFireButton;
        componentReferences.inputHandler.onReloadPress -= reload;
        componentReferences.weaponManager.onEquipWeapon -= componentReferences.animationController.useAnimationForGun;

        unsubscribeStatusDisplayBoxFromEvents();
    }

    void unsubscribeStatusDisplayBoxFromEvents() {

        if (statusDisplayBox == null)
            return;

        healthManager.onHealthChange -= statusDisplayBox.setHealth;
        componentReferences.weaponManager.onAmmoChange -= statusDisplayBox.setAmmo;
    }

    // Update is called once per frame
    void Update() {

        if(currentState == States.Grounded) {

            //determine the vector tangent to the surface the player is standing on that way he can move left and right along the surface
            localHorizontalDirection = getLocalHorizontalAxis();

            //if the vector is 0 it means that he isn't standing on anything so he eshould start falling
            if(localHorizontalDirection.sqrMagnitude < 0.01f) {

                changeState(States.Falling);
            }
        }

        if(currentState == States.Dead) {

            return;
        }
        
        handleInput();

        componentReferences.animationController.setJumpVelocityParameter(componentReferences.body.velocity.y);
        componentReferences.animationController.setIsGroundedParameter(currentState == States.Grounded);
        componentReferences.animationController.setIsWalkingParameter(componentReferences.body.velocity.x != 0);
        
        determineSpriteDirection();
        determineSpriteArmOrientation();
    }

    //reads the player's input to determine his movement and orientation
    void handleInput() {

        float valueHorizontalAxis = componentReferences.inputHandler.getValueHorizontalAxis();
        Vector2 velocity = new Vector2(0, componentReferences.body.velocity.y);

        if (currentState == States.Grounded) {

            //if player moves horizontally then determine the vector tangent to the surface he is standing  on
            //make player move along the surface beneath him

            //if player presses jump button, go to jumped state 
            velocity = localHorizontalDirection * valueHorizontalAxis * gameplayProperties.speed;
        }

        if(currentState == States.Jumping || currentState == States.Falling) {

            //if player moves horizontally then move him in global left or right direction
            //don't override the vertical velocity that way he can keep falling at wwhatever speed the physics engine
            velocity += new Vector2(1, 0) * valueHorizontalAxis * gameplayProperties.speed;
        }

        if(currentState == States.Dead) {

            //don't handle input
            return;
        }

        componentReferences.body.velocity = velocity;
    }

    bool canReload() {

        return componentReferences.weaponManager.canReload() && componentReferences.animationController.isAiming();
    }

    bool canFire() {

        return componentReferences.animationController.isAiming() && componentReferences.weaponManager.canFire();
    }

    bool canJump() {

        return currentState == States.Grounded;
    }

    //plays the fire animation, creates a bullet
    void fire() {

        bool fire = canFire();

        if (fire) {

            componentReferences.weaponManager.fire(angleToFireBullets, this);
        }

        componentReferences.animationController.setFireParameter(fire);
    }

    void releaseFireButton() {

        componentReferences.animationController.setFireParameter(false);
    }

    //the current ccalculated velocity of the player this frame
    void jump() {

        if(canJump())
            changeState(States.Jumping);
    }

    void reload() {

        if(canReload()) {

            componentReferences.animationController.playReloadAnimation();
        }
    }

    //flip the player's sprite to the left or right depending on which way he is moving
    void determineSpriteDirection() {

        //when player is moving left, scale body by -1 to make it face left, otherwise make the scale +1 to face right
        Vector3 previousScale = componentReferences.bodyParts.bodyRoot.transform.localScale;

        //multiply scale by -1 to set the correct sign, that way if player was scaled then his size remains the same
        if ((componentReferences.body.velocity.x < 0 && previousScale.x > 0) || (componentReferences.body.velocity.x > 0 && previousScale.x < 0)) {

            previousScale.x *= -1;
        }

        componentReferences.bodyParts.bodyRoot.transform.localScale = previousScale;
    }

    //rotates the arms so that they're pointing towards the target
    //flips the arms horizontally if target is to the left or right of the player
    //this will also take the body's current direction into account so that if player is already facing left and target is to the left, the arms won't be mirrored
    void determineSpriteArmOrientation() {

        aimTargetPosition = componentReferences.inputHandler.calculateAimVector(aimTargetPosition, componentReferences.bodyParts.arms.transform.position);

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
        if (componentReferences.bodyParts.arms.transform.lossyScale.x * aimTargetPosition.x < 0) {

            Vector3 scale = componentReferences.bodyParts.arms.transform.localScale;
            scale.x *= -1;

            componentReferences.bodyParts.arms.transform.localScale = scale;
        }

        //calcualte the angle above the horizontal of the arm
        //force angles between [-pi, pi] since mirroring the arm makes the rotation face left or right to get full 2pi radians coverage
        float angle = Mathf.Atan2(aimTargetPosition.y, Mathf.Abs(aimTargetPosition.x));
        angleToFireBullets = Mathf.Atan2(aimTargetPosition.y, aimTargetPosition.x);//this angle is different from the angle used to rotate the arms
        //Since angleToFireBullets is between [-pi, pi] and angle is bewteen [-pi/2, pi/2] AND angle is modified a little bit so the gun is aligned with the line of sight

        angle -= componentReferences.weaponManager.getGunElevationAbovePlayerHands();

        //if user is reloading then disable aiming so it doesn't mess up the reloading animation
        if (componentReferences.animationController.isReloading())
            angle = 0;

        //if the arm is scaled by -1 then you need to multiply the angle by negative 1 because unity will automatically invert the angle when scale is negative
        if (componentReferences.bodyParts.arms.transform.lossyScale.x < 0)
            angle *= -1;

        componentReferences.bodyParts.arms.transform.rotation = Quaternion.Slerp(componentReferences.bodyParts.arms.transform.rotation, Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg), Time.deltaTime * gameplayProperties.aimSensitivity);
        Debug.DrawRay(componentReferences.bodyParts.arms.transform.position, aimTargetPosition, Color.red);

    }

    //returns the vector that is tangent to the surface the player is currently standing on
    //if the vector returned has magnitude 0 then player is not standing on any surface
    Vector2 getLocalHorizontalAxis() {
        
        //cast a box downwards and if it hits anything then you know player is standing on top of something
        //the box will have width equal to the player's collider width
        //and height of 1 unit
        Vector2 boxOrigin = componentReferences.collider.bounds.center;
        boxOrigin.y -= componentReferences.collider.bounds.extents.y;

        float pixelToUnit = 1.0f / 64.0f;
        
        Vector2 boxSize = componentReferences.collider.bounds.size;
        boxSize.y = pixelToUnit;//1 unit height

        //start the box cast slightly below the player's position that way if he collides with something right beside him that is level with him, he won't collide with the bottom of the
        //object's collider and mess up the localHorizontalDirection calculation
        //boxOrigin.y -= boxSize.y;

        //some other function mighthave disabled collider, and we don't wanna forcibly enable collider incase it was disabled before, so keep track of whether it was previously enabled/disabled
        bool colliderEnabledCache = componentReferences.collider.enabled;

        //disable player collider beforehand so boxcast doesn't return player
        componentReferences.collider.enabled = false;

        float boxAngle = 0;//rotation of the box being cast
        RaycastHit2D[] objectsBelowPlayer = Physics2D.BoxCastAll(boxOrigin, boxSize, boxAngle, Vector2.down, isGroundedBoxCastDistance, raycastLayers.value);

        componentReferences.collider.enabled = colliderEnabledCache;

        //check to see if the boxcast hit the the top of any object, if it did then we can use that to determine the player's local axis
        //if the boxcast doesn't return the tops of any objects then it might have hit an object that is right beside the player by accident, and we don't consider that as grounded
        RaycastHit2D objectBelowPlayer = new RaycastHit2D();

        //find first object player collided with
        foreach (var hitBelowPlayer in objectsBelowPlayer) {

            //normal points slightly upwards
            if (hitBelowPlayer.normal.y > 0.2) {
                
                objectBelowPlayer = hitBelowPlayer;
                break;
            }
        }

        //if he collided with an object
        if (objectBelowPlayer.collider != null) {

            //align the players horizontal direction to be parallel with the  top of the ground
            //that way when he moves left/right he climbs up/down a slope and doesn't try to walk into it
            return new Vector2(objectBelowPlayer.normal.y, -objectBelowPlayer.normal.x);
        }

        return new Vector2(0, 0);
    }

    void OnDrawGizmos() {

        Vector2 boxOrigin = componentReferences.collider.bounds.center;
        boxOrigin.y -= componentReferences.collider.bounds.extents.y;

        Vector2 boxSize = componentReferences.collider.bounds.size;
        boxSize.y = 1.0f / 64.0f;//1 unit height
        boxOrigin.y -= boxSize.y;

        Gizmos.DrawCube(boxOrigin, boxSize);
        Vector2 length = new Vector2(0, boxSize.y + isGroundedBoxCastDistance);
        Debug.DrawLine(boxOrigin, boxOrigin - length, Color.green);
    }

    void createBodyCloneForDeath() {

        //first make a copy of all of the player's body parts
        GameObject bodyCopy = GameObject.Instantiate(componentReferences.bodyParts.bodyRoot, componentReferences.bodyParts.bodyRoot.transform.position, componentReferences.bodyParts.bodyRoot.transform.rotation) as GameObject;
        bodyCopy.GetComponent<Animator>().enabled = false; //disable animations so explosion effect can happen

        //GET RID OF THIS LATER
        //temp sprites were scaled down by 1/2 so scale them back up again
        bodyCopy.transform.localScale = new Vector3(2, 2, 2);

        //now find all body parts that have a sprite renderer and are tagged player, since these are the parts that can make up the explosion effect
        Component[] childrenWithSprites = bodyCopy.GetComponentsInChildren<SpriteRenderer>();

        foreach (Component component in childrenWithSprites) {

            //ignore anything not tagged player because it could be some other children like a gun or something
            if (!component.gameObject.CompareTag("PlayerBodyPart"))
                continue;

            Rigidbody2D cloneRigidBody = component.gameObject.GetComponent<Rigidbody2D>() as Rigidbody2D;

            //player body part found, in order to apppy explosion effect we need to enable the rigidbody and collider
            cloneRigidBody.isKinematic = false;
            cloneRigidBody.simulated = true;
            (component.gameObject.GetComponent<BoxCollider2D>() as BoxCollider2D).enabled = true;

            //push this for explosion effect, all parts should go in some random direction
            cloneRigidBody.velocity = Vector2.up * Random.Range(0, 20) + Vector2.right * Random.Range(-20, 20);
            cloneRigidBody.angularVelocity = Random.Range(-90, 90);
        }

        //destoryy previous body if there is still a reference to it since it means that it wasn't destoryed yet
        if (bodyCloneForDeathEffect != null)
            GameObject.Destroy(bodyCloneForDeathEffect);

        //store a reference to the new body so it can be deleted later
        bodyCloneForDeathEffect = bodyCopy;
    }

    //make a death body part explosion effect
    void die() {

        changeState(States.Dead);
    }

    public void respawn() {

        changeState(States.Grounded);
    }

    public bool isAlive() {

        return healthManager.getCurrentHealth() > 0;
    }

    public void receiveMoney(int amount) {

        gameplayProperties.playerMoney += amount;
        statusDisplayBox.setMoney(gameplayProperties.playerMoney);
    }

    void OnCollisionEnter2D(Collision2D collision) {

        //if the colliding object isn't something the player can stand on, then ignore it
        if (((1 << collision.gameObject.layer) & raycastLayers.value) == 0)
            return;

        //player is already grounded, no need to ground him again
        if(currentState == States.Grounded)
            return;

        changeState(States.Grounded);
        
        //if player is going upwards then he might have passed through a one way platform so he is still jumping
        //don't comment this out becaues it will cause infinite jumping if player starts moving up a slope and jumps,
        //but lands on the slope again while he is still goign upwards
        /*if(body.velocity.y > 0)
            return;*/
    }

    //assigns the given display box to this player and makes the box track this player's information
    public void assignStatusDisplayBox(StatusDisplayBoxController displayBoxController) {

        unsubscribeStatusDisplayBoxFromEvents();

        statusDisplayBox = displayBoxController;

        //set the initial values for the display box
        statusDisplayBox.setAmmo(componentReferences.weaponManager.getAmmo());
        statusDisplayBox.setTeamColor(gameplayProperties.team);
        statusDisplayBox.setPlayerName(gameplayProperties.playerName);
        statusDisplayBox.setHealth(healthManager.getCurrentHealth());
        statusDisplayBox.setMoney(gameplayProperties.playerMoney);
        statusDisplayBox.setScore(0);

        subscribeStatusDisplayBoxToEvents();
    }

    //when a team's score gets changed the status display box should reflect the change
    public void handleTeamScoreChange(TeamProperties.Teams teamWhoseScoreWasChanged, int newScore) {

        if (gameplayProperties.team == teamWhoseScoreWasChanged)
            statusDisplayBox.setScore(newScore);
    }

    public BoxCollider2D getCollider() {

        return componentReferences.collider;
    }

    public TeamProperties.Teams getTeam() {

        return gameplayProperties.team;
    }
}
