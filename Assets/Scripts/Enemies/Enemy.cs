using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Life))]
[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour, IGrabbable {

    protected Rigidbody2D rigid;
    protected bool grabbed = false;
    protected GameObject grabbingObject = null;
    protected Life life;
    protected bool thrown = false;

    protected Action currentAction;
    protected Action grabReaction;

    // Use this for initialization
    protected void Start () {
        life = GetComponent<Life>();
        rigid = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	protected void FixedUpdate () {
        if(!thrown && currentAction != null)
            currentAction();
	}

    public virtual void Bite(float damage) {
        life.Hit(damage);
    }

    public virtual void Grab(GameObject head) {
        grabbingObject = head;
        grabbed = true;
        if (grabReaction != null)
            grabReaction();
    }

    public virtual void UnGrab() {
        grabbingObject = null;
        grabbed = false;
    }

    public virtual bool isDead() {
        return life.isDead();
    }

    public virtual void Throw(Vector3 direction, float strength) {
        rigid.AddForce(direction * strength, ForceMode2D.Impulse);
        thrown = true;
    }
}
