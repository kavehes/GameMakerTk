using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LamaController : MonoBehaviour , IHittable {
    
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
    public float ledgingStrength;
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
    public float Speed = 3f;
    public float UpDownSpeed = 5f;

    [Header("Combat")]
    public int candySize = 0;
    public int equilibre = 5;
    public AimCursor cursor;

    bool cooldown;

    public bool FacingRight = false;


    void Start() {
        rigid = GetComponent<Rigidbody2D>();
        headScript = head.GetComponent<HeadScript>();
    }

    void FixedUpdate() {
        grounded = Physics2D.OverlapCircle(Feet.position, feetRadius,groundLayer);
        ledging = Physics2D.OverlapCircle(wallChecker.position, feetRadius, groundLayer) && !Physics2D.OverlapCircle(ledgeChecker.position, feetRadius, groundLayer);

        if (grounded) {
            if(!cooldown)
                rigid.velocity =new Vector2(Input.GetAxis("Horizontal_P" + playerNumber)*Speed, rigid.velocity.y);
            switch (headState) {
                case HeadState.Launched:
                    if (CanWalkOnLaunch) {
                        headScript.ShortenDistance(Mathf.Max(0, Input.GetAxis("Vertical_P" + playerNumber) * Time.deltaTime) * UpDownSpeed);
                    }
                    else if(!cooldown) rigid.velocity = Vector2.zero;
                    break;
                case HeadState.Grabbing:
                    headScript.ShortenDistance(Mathf.Max(0, Input.GetAxis("Vertical_P" + playerNumber) * Time.deltaTime) * UpDownSpeed);
                    break;
                case HeadState.Attached:
                    headScript.EnableJoint(false);
                    if(Input.GetButtonDown("B_P" + playerNumber)) {
                        Debug.Log("Hey");
                        headScript.Throw(cursor.aimDirection());
                    }
                    break;
            }
        }
        else {
            switch (headState) {
                case HeadState.Grabbing:
                    if (!ledging)
                        Swing();
                    else {
                        if (Input.GetAxis("Vertical_P" + playerNumber) > 0.5f) {
                            rigid.AddForce((Vector2.up) * ledgingStrength);
                            //transform.position += Vector3.up + (FacingRight? Vector3.right : Vector3.left)*1.5f;
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
                head.transform.position = (cursor.transform.position - neckStart.transform.position).normalized * neckSize + neckStart.position;
                if (cursor.transform.position.x > transform.position.x && !FacingRight || cursor.transform.position.x < transform.position.x && FacingRight) {
                    Flip();
                }
                //Launch the head
                if (Input.GetButtonDown("A_P" + playerNumber)) {
                    head.GetComponent<Rigidbody2D>().AddForce((cursor.transform.position - neckStart.transform.position).normalized * headLaunchForce);
                    head.GetComponent<Rigidbody2D>().gravityScale = 1;
                    headScript.EnableJoint(true);
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
                        head.GetComponent<HeadScript>().StartCoroutine(head.GetComponent<HeadScript>().Rewind());
                        headState = HeadState.CommingBack;
                    }
                }
                break;
        }
	}

    IEnumerator Cooldown() {
        yield return new WaitForSeconds(2f);
        Debug.Log("Please Proceed");
        cooldown = false;
        rigid.gravityScale = 1f;
    }

    void KnockBack(float HitStrength) {
        StartCoroutine(Cooldown());
        cooldown = true;
        rigid.AddForce((Vector3.right + Vector3.up)*HitStrength);
        rigid.gravityScale = 0.5f;
        grounded = false;
    }

    public void Hit(int candyS, GameObject obj) {
        switch (candyS) {
            case 1:
                KnockBack(30000);
                break;
            case 2:
                KnockBack(150000);
                break;
            case 3:
                KnockBack(300000);
                break;
            case 4:
                KnockBack(3000000);
                break;

        }
    }


    void Swing() {
        rigid.AddForce(new Vector2(Input.GetAxis("Horizontal_P" + playerNumber) * balanceStrength, 0));
        headScript.maxDistance -= headScript.maxDistance - Input.GetAxis("Vertical_P" + playerNumber) * Time.deltaTime * UpDownSpeed < neckMaxLength ? Input.GetAxis("Vertical_P" + playerNumber) * Time.deltaTime * UpDownSpeed : 0;
    }

    public void Flip() {
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
