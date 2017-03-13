using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputHandler : MonoBehaviour {

    public delegate void Action();

    public event Action onJumpPress;
    public event Action onFirePress;
    public event Action onReloadPress;

    //players current movmeent along the horizontal direction
    public enum HorizontalInputMovement {

        Left,
        Right,
        NotMoving
    };

    int controllerId;

    //strings for each input axis
    //these represent the names of the axis used to represent horizontal/vertical input, and button presses for jump, reload, and fire
    //these are the UNNUMBERED axis names, without the number suffix that represents the controller ID
    string horizontalAxis = "Horizontal";
    string jumpAxis = "Jump";
    string reloadAxis = "Reload";
    string fireAxis = "Fire";

    //for controllers only
    string horizontalAimAxis = "AimHorizontal";
    string verticalAimAxis = "AimVertical";

    // Update is called once per frame
    void Update() {
        
        if (Input.GetButtonDown(jumpAxis + controllerId) && onJumpPress != null)
            onJumpPress();

        if (Input.GetButton(fireAxis + controllerId) && onFirePress != null)
            onFirePress();

        if (Input.GetButtonDown(reloadAxis + controllerId) && onReloadPress != null)
            onReloadPress();
    }

    public void setControllerId(int id) {

        controllerId = id;
    }

    //get the input reading along the horizontal axis
    public float getValueHorizontalAxis() {

        return Input.GetAxis(horizontalAxis + controllerId);
    }

    //calculate the aim vector and stores the result in currentAimVector
    //armOrigin is the worldspace position of the eorigin of the playyer's arm, used to calcalculate the
    //vector to target when the user is using the keyboard.
    public Vector2 calculateAimVector(Vector2 currentAimVector, Vector3 armOrigin) {

        //for keyboard/mouse user
        if (controllerId == 0) {

            currentAimVector = Camera.main.ScreenToWorldPoint(Input.mousePosition) - armOrigin;
            return currentAimVector;
        }

        //for controllers
        //don't immediately use the input as the target position because we don't want the player's aim to jump around when he lets go of the control stick
        Vector2 currentAxisValue = new Vector2(Input.GetAxisRaw(horizontalAimAxis + controllerId), Input.GetAxisRaw(verticalAimAxis + controllerId));

        //ignore values at center of axis that way when player lets go of the stick, the position of the target remains unchanged because
        //it will use the last recorded input values which are all non zero, and the current reading is zero which is ignored
        //this will let the user's aim stay close to what it was before he let go of the stick
        //rawAxis is used because the inputManager's deadzones will output 0 for values at the edge of the deadzone, and i don't want
        //to have values of 0 outside of my deadzone, i want the actual value of the controller.
        float deadzone = 0.15f;
        if (currentAxisValue.sqrMagnitude > deadzone * deadzone)
            currentAimVector = new Vector2(Input.GetAxis(horizontalAimAxis + controllerId), Input.GetAxis(verticalAimAxis + controllerId));

        return currentAimVector;
    }
}
