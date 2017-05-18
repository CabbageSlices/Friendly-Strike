using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//class that provides a functionality for a player object to play different animations
//class should be put as a parent
public class PlayerAnimationController : MonoBehaviour {

    //player's arms are animated accordign to the weapon he is holding
    //each type of weapon corresponds to a different animation layer, and we want to disable all arm animation layers except the layer corresponding to the equipped weapon
    //map each type of weapon to an animation layer id
    //remember that layer 0 is for the players  body, and will always play regardless of the type of weapon he is holding
    Dictionary<GunProperties.Type, int> gunTypeToLayer = new Dictionary<GunProperties.Type, int>() {

        { GunProperties.Type.Pistol, 1},
        { GunProperties.Type.SMG, 2}
    };

    [SerializeField]
    //animator used dto animate the player's body
    private Animator animator;

    // Use this for initialization
    void Start () {
		
        if(animator == null)
            animator = gameObject.GetComponentInChildren<Animator>() as Animator;
        
        useAnimationForGun(GunProperties.Type.SMG);
	}

    //disable the animations on all the arm layers
    void disableArmLayers() {
        
        foreach (GunProperties.Type type in gunTypeToLayer.Keys) {

            animator.SetLayerWeight((int)type, 0.0f);
        }
    }

    public void setJumpVelocityParameter(float velocity) {

        animator.SetFloat(PlayerAnimationHashCodes.jumpVelocityKey, velocity);
    }

    public void setIsWalkingParameter(bool isWalking) {

        animator.SetBool(PlayerAnimationHashCodes.isWalkingKey, isWalking);
    }

    public void setIsGroundedParameter(bool isGrounded) {

        animator.SetBool(PlayerAnimationHashCodes.IsGroundedKey, isGrounded);
    }

    public void playReloadAnimation() {

        animator.SetTrigger(PlayerAnimationHashCodes.reloadTriggerKey);
    }

    public void setFireParameter(bool fire) {

        animator.SetBool(PlayerAnimationHashCodes.fireKey, fire);
    }

    //make ethe player play the arm animations that correspond to the given type of weapon
    public void useAnimationForGun(GunProperties.Type type) {

        disableArmLayers();

        //enable the layer that corresponds to the given weapon
        animator.SetLayerWeight(gunTypeToLayer[type], 100.0f);
    }

    public bool isReloading() {

        return animator.GetCurrentAnimatorStateInfo(1).shortNameHash == PlayerAnimationHashCodes.armsReloadingStateKey;
    }

    public bool isAiming() {

        return animator.GetCurrentAnimatorStateInfo(1).shortNameHash == PlayerAnimationHashCodes.armsAimingStateKey;
    }
}
