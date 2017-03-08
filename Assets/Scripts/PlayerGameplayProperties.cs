using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PlayerGameplayProperties {

    public float speed;//horizontal movement speed
    public float jumpSpeed; //initial vertical speed the player gets when he presses the jump button

    [Range(10, 100)]
    public int aimSensitivity; //how fast the aim will jump to player's intended target

    //id of the controller used to control this player
    //0 represents keyboard/mouse, all numbers bigger than 0 represent a joypad
    public int controllerId;

    //player's team id
    public TeamProperties.Teams team;

    public string playerName;
    public int playerMoney;
}
