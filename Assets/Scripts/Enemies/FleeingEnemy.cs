﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleeingEnemy : Enemy {

    public float fleeingTime = 4f;

	// Use this for initialization
	new void Start () {
        base.Start();
        int dir = Random.Range(0, 2);
        if (dir == 0) moveVector = new Vector3(1, 0);
        else moveVector = new Vector3(-1, 0);
        currentAction = Wander;
        grabReaction = () => { StartCoroutine(FleeTime()); };
    }

    IEnumerator FleeTime() {
        float s = Speed;
        Speed *= 2;
        yield return new WaitForSeconds(fleeingTime);
        Speed = s;
    }
}
