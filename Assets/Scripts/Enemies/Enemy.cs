using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Life))]
public class Enemy : MonoBehaviour, IGrabbable {

    bool grabbed = false;
    GameObject grabbingObject = null;
    Life life;

    // Use this for initialization
    void Start () {
        life = GetComponent<Life>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Bite(float damage) {
        life.Hit(damage);
    }

    public void Grab(GameObject head) {
        grabbingObject = head;
        grabbed = true;
    }

    public void UnGrab() {
        grabbingObject = null;
        grabbed = false;
    }

    public bool isDead() {
        return life.isDead();
    }
}
