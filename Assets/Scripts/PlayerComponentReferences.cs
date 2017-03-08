using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//references to various components that the player should have
[System.Serializable]
public struct PlayerComponentReferences {

    //object's own components, way to cache the object returned by GetComponent
    public Rigidbody2D body;
    
    public BoxCollider2D collider;

    public PlayerAnimationController animationController;

    public PlayerInputHandler inputHandler;

    public EquippedWeaponManager weaponManager;//used to handle weapon control

    public PlayerBodyParts bodyParts;//body parts of the player
}
