using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Life : MonoBehaviour {

    public float maxLife = 10;
    float currentLife;

	// Use this for initialization
	void Start () {
        currentLife = maxLife;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public bool Hit(float damage) {
        currentLife -= damage;
        return isDead();
    }

    public bool isDead() {
        return currentLife <= 0;
    }
}
