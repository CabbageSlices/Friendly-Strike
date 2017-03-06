using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class sadf : MonoBehaviour {

    public Scrollbar bar;
    public Scrollbar bar2;
    public float speed;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
        bar.value += Input.GetAxis("Horizontal0") * speed * Time.deltaTime;
        bar2.value += Input.GetAxis("Vertical0") * speed * Time.deltaTime;
    }
}
