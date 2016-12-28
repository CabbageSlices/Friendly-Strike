using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//keeps track of different information about each teacm
public class TeamProperties : MonoBehaviour {

	public enum Teams {

        red = 0,
        green,
        blue,
        yellow,
        black,
        white,
        cyan,
        magenta
    }

    //color represented by each team
    static public Dictionary<Teams, Vector4> teamNamesToColors = new Dictionary<Teams, Vector4>() {

        {Teams.red, Color.red},
        {Teams.green, Color.green},
        {Teams.blue, Color.blue},
        {Teams.yellow, Color.yellow},
        {Teams.black, Color.black},
        {Teams.white, Color.white},
        {Teams.cyan, Color.cyan},
        {Teams.magenta, Color.magenta}
    };
}
