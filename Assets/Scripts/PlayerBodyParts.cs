using UnityEngine;
using System.Collections;

//keep references to player body parts that are used in code
public class PlayerBodyParts : MonoBehaviour {

    public GameObject arms;
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject bodyRoot;//root game object that contains each of the sprites of the player body

    void Start() {

        if (arms == null)
            Debug.LogWarning("PlayerBodyParts missing arms reference.");

        if (leftHand == null)
            Debug.LogWarning("PlayerBodyParts missing leftHand reference");

        if (rightHand == null)
            Debug.LogWarning("PlayerBodyParts missing rightHand reference");

        if (bodyRoot == null)
            Debug.LogWarning("PlayerBodyParts missing bodyRoot reference");
    }
}
