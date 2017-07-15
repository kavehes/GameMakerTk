using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BasicEnemy : Enemy {

    public float moveDistance = 2.0f;

    float timer = 0.0f;
    Vector3 moveVector = new Vector3(1, 0);

	// Use this for initialization
	new void Start () {
        base.Start();
        currentAction = Normal;
	}

    void Normal() {
        float movement = 0.5f * Time.deltaTime;
        if (!grabbed) {
            if (timer > moveDistance || 
                Physics2D.OverlapPoint(transform.position + moveVector + Vector3.down * 2) == null ||
                Physics2D.OverlapPoint(transform.position + moveVector.normalized*0.22f) != null) {
                moveVector *= -1;
                timer = 0;
            }
            transform.position += moveVector * movement;
            timer += movement;
        }
    }

}
