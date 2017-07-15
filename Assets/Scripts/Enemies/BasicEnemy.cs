using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BasicEnemy : Enemy {

	// Use this for initialization
	new void Start () {
        base.Start();
        currentAction = Wander;
	}

}
