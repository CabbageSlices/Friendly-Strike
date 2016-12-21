using UnityEngine;
using System.Collections;

public class PlayerAnimationHashCodes : MonoBehaviour {

    //keys used by the animator to access the parameters used to change animation states
    [System.NonSerialized] public int isRunningKey = Animator.StringToHash("IsRunning");
}
