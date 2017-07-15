using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LamaController : MonoBehaviour {
    
    public enum HeadState {
        Attached,
        Launched,
        Grabbing,
        CommingBack
    }
    public int playerNumber = 1;

    public HeadState headState = HeadState.Attached;
    [Header("Neck")]
    public GameObject head;
    public Transform neckStart;
    public float neckSize = 10f;
    float neckLength;
    public float neckMaxLength = 10;

    HeadScript headScript;
    [Header("Head launching")]
    public float headLaunchForce = 7000;
    [Header("Body")]
    public bool grounded;
    public bool ledging;
    public Transform ledgeChecker;
    public Transform wallChecker;
    public Transform Feet;
    public float feetRadius = 0.2f;
    public LayerMask groundLayer;
    public bool CanWalkOnLaunch = true;
    bool CommitToLedge;
    Rigidbody2D rigid;
    [Header("Movement")]
    public float balanceStrength = 10f;
    public float UpDownSpeed = 5f;

    [Header("Combat")]
    public int candySize = 0;
    public int equilibre = 5;

    bool FacingRight = false;


    void Start() {
        rigid = GetComponent<Rigidbody2D>();
        headScript = head.GetComponent<HeadScript>();
    }

    void FixedUpdate() {
        grounded = Physics2D.OverlapCircle(Feet.position, feetRadius,groundLayer);
        ledging = Physics2D.OverlapCircle(wallChecker.position, feetRadius, groundLayer) && !Physics2D.OverlapCircle(ledgeChecker.position, feetRadius, groundLayer);
        if (grounded) {
            rigid.velocity =new Vector2(Input.GetAxis("Horizontal_P" + playerNumber), rigid.velocity.y);
            switch (headState) {
                case HeadState.Launched:
                    if (CanWalkOnLaunch) {
                        headScript.ReduceFirstJoint(Mathf.Max(0, Input.GetAxis("Vertical_P" + playerNumber) * Time.deltaTime) * UpDownSpeed);
                    }
                    break;
                case HeadState.Grabbing:
                    headScript.maxDistance -= Mathf.Max(0, Input.GetAxis("Vertical_P" + playerNumber) * Time.deltaTime) * UpDownSpeed;
                    break;
            }
        }
        else {
            switch (headState) {
                case HeadState.Grabbing:
                    if (!ledging)
                        Swing();
                    else {
                        if (Input.GetAxis("Vertical_P" + playerNumber) > 0) {
                            transform.position += Vector3.up + (FacingRight? Vector3.right : Vector3.left)*1.5f;
                            headScript.DeleteJoint();
                        }
                        else {
                            Swing();
                        }
                    }
                    break;
                case HeadState.Launched:
                    Swing();
                    break;
            }
        }
    }

    void Update() {
        switch (headState) {
            case HeadState.Attached:
                head.transform.position = (AimCursor.current.transform.position - transform.position).normalized * neckSize + neckStart.position;
                if (AimCursor.current.transform.position.x > transform.position.x && !FacingRight || AimCursor.current.transform.position.x < transform.position.x && FacingRight) {
                    Flip();
                }
                //Launch the head
                if (Input.GetButtonDown("A_P" + playerNumber)) {
                    head.GetComponent<Rigidbody2D>().AddForce((AimCursor.current.transform.position - transform.position).normalized * headLaunchForce);
                    head.GetComponent<Rigidbody2D>().gravityScale = 1;
                    headState = HeadState.Launched;
                }
                break;
            case HeadState.Launched:
                if (Input.GetButtonDown("Y_P" + playerNumber)) {
                    foreach(CircleCollider2D c in headScript.cc) {
                        c.enabled = true;
                    }
                    head.GetComponent<HeadScript>().StartCoroutine(head.GetComponent<HeadScript>().Rewind());
                    headState = HeadState.CommingBack;
                }
                break;
            case HeadState.Grabbing:
                //UnGrab
                if (Input.GetButtonDown("Y_P" + playerNumber)) {
                    headScript.UnGrab();
                }
                //Eat enemy
                else if(Input.GetButtonDown("B_P" + playerNumber)) {
                    if (headScript.Eat()) {

                    }
                }
                break;
        }
	}

    void Swing() {
        rigid.AddForce(new Vector2(Input.GetAxis("Horizontal_P" + playerNumber) * balanceStrength, 0));
        headScript.maxDistance -= headScript.maxDistance - Input.GetAxis("Vertical_P" + playerNumber) * Time.deltaTime * UpDownSpeed < neckMaxLength ? Input.GetAxis("Vertical_P" + playerNumber) * Time.deltaTime * UpDownSpeed : 0;
    }

    void Flip() {
        Vector3 temp = transform.localScale;
        temp.x *= -1;
        transform.localScale = temp;
        FacingRight = !FacingRight;
    }

    void OnDrawGizmos() {
        if(head != null && neckStart != null) {
            //Gizmos.DrawLine(head.transform.position, neckStart.position);
        }
    }
}
