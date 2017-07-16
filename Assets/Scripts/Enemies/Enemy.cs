using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Life))]
[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour, IGrabbable {

    public float Speed = 3;
    public Collider2D stickedCollider;
    int side = 0;
    int size = 1;
    public LayerMask wallLayer;

    Vector2 lastNormal;
    Vector3 stickedLastPosition;
    Quaternion stickedLastRotation;

    protected Rigidbody2D rigid;
    protected Collider2D collider;

    protected float moveSide = 1;
    protected Vector3 moveVector = new Vector3(1, 0);

    protected bool grabbed = false;
    protected GameObject grabbingObject = null;
    protected Life life;
    protected bool thrown = false;

    protected Action currentAction;
    protected Action grabReaction;

    bool changed = false;

    // Use this for initialization
    protected void Start () {
        life = GetComponent<Life>();
        rigid = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        currentAction = Wander;
	}

    // Update is called once per frame
    protected void FixedUpdate() {
        changed = false;
        if (!thrown && currentAction != null)
            currentAction();
        if (stickedCollider) {
            transform.position += (stickedCollider.transform.position-stickedLastPosition);
            transform.Rotate(Vector3.forward, Quaternion.Angle(stickedCollider.transform.rotation,stickedLastRotation));
            stickedLastPosition = stickedCollider.transform.position;
            stickedLastRotation = stickedCollider.transform.rotation;
        }
        if(!changed)
            StickToCollider();
        return;
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
        rigid.gravityScale = 1;
    }

    public int getSize() {
        return size;
    }

    protected bool hasSomethingInFront() {
        return (Physics2D.OverlapPoint(transform.position + moveVector.normalized * 0.22f) != null);
    }

    protected void Flip() {
        moveSide *= -1;
        Vector3 newScale = transform.localScale;
        newScale.x *= -1;
        transform.localScale = newScale;
    }

    void SetDirection(int side) {
        transform.localRotation = Quaternion.AngleAxis(90*side, Vector3.forward);
        moveVector = (Quaternion.AngleAxis(90 * side, Vector3.forward) * Vector3.right)*moveSide; 
    }

    void Move() {
        if (stickedCollider != null) {
            float movement = Speed;
            rigid.velocity = (Vector2)moveVector * movement;
            Collider2D col = Physics2D.OverlapPoint(collider.bounds.center + transform.right * collider.bounds.size.magnitude);
            if (col != null && (1 << col.gameObject.layer & wallLayer.value) != 0)
                StickToCollider(false, true);
        }
    }

    void StickToCollider(bool type = true, bool front = false) {
        changed = true;
        if (stickedCollider == null)
            return;
        Vector2 origin;
        if (type) origin = transform.position - 0.1f * transform.up;
        else origin = collider.bounds.center;
        if (stickedCollider.OverlapPoint(origin))
            return;
        Vector2 direction;
        if (!front) direction = ((Vector2)stickedCollider.bounds.center - origin);
        else direction = transform.right;
        float distance = Vector3.Distance(origin, stickedCollider.bounds.center);
        RaycastHit2D[] hit = Physics2D.RaycastAll(origin,direction, distance, wallLayer);
        int found = -1;
        for(int i=0; i<hit.Length; i++) {
            if(hit[i] == stickedCollider) {
                found = i;
                break;
            }
        }

        if (found == -1) {
            Debug.Log("quit");
            stickedCollider = null;
            lastNormal = new Vector2();
            rigid.gravityScale = 1;
            return;
        }
        if (Mathf.Abs(Vector2.Dot(hit[found].normal,lastNormal))>0.97f)
            return;
        Debug.Log("Sticking: " + hit[found].normal);

        float rot_z = Mathf.Atan2(hit[found].normal.y, hit[found].normal.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);
        moveVector = Quaternion.Euler(0f, 0f, rot_z - 90) * Vector3.right;
        rigid.MovePosition(hit[found].point);
        lastNormal = hit[found].normal;
    }


    void OnCollisionEnter2D(Collision2D coll) {
        if((wallLayer.value & 1<<coll.collider.gameObject.layer) != 0) {
            stickedCollider = coll.collider;
            stickedLastPosition = stickedCollider.transform.position;
            stickedLastRotation = stickedCollider.transform.rotation;
            rigid.gravityScale = 0;
            //StickToCollider(false);
            //Debug.Log("Sticked collider: " + coll.gameObject);
            thrown = false;
        }
    }

    void OnCollisionExit2D(Collision2D coll) {
        if (coll.collider == stickedCollider) {
            stickedCollider = null;
            rigid.gravityScale = 1;
        }
    }

    //Actions
    protected void Wander() {
        
        if (!grabbed) {
            /*if (hasSomethingInFront()) {
                Flip();
            }*/
        }
        Move();
    }
}
