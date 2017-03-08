using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//list of hash codes
public static class PlayerAnimationHashCodes {

    public static int armsAimingStateKey = Animator.StringToHash("Aiming");//aiming state in any layer
    public static int armsReloadingStateKey = Animator.StringToHash("Reload");

    public static int fireKey = Animator.StringToHash("Fire");
    public static int reloadTriggerKey = Animator.StringToHash("Reload");
    public static int isWalkingKey = Animator.StringToHash("IsWalking");
    public static int jumpVelocityKey = Animator.StringToHash("JumpVelocity"); //velocity is used to determine if jumpUp animatin should be used or jumpDown animation
    public static int IsGroundedKey = Animator.StringToHash("IsGrounded");
}