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

    public HeadState headState = HeadState.Attached;
    
    public GameObject head;
    public Transform neckStart;
    public float neckSize = 10f;
    float neckLength;
    public float neckMaxLength = 10;

    bool FacingRight = false;

	void Update () {
        if (headState == HeadState.Attached) {
            head.transform.position = (AimCursor.current.transform.position - transform.position).normalized*neckSize + neckStart.position;
            if (Input.GetButtonDown("Fire1")){
                head.GetComponent<Rigidbody2D>().AddForce((AimCursor.current.transform.position - transform.position).normalized*7000);
                head.GetComponent<Rigidbody2D>().gravityScale = 1;
                headState = HeadState.Launched;
            }
        }
        if(AimCursor.current.transform.position.x > transform.position.x && !FacingRight || AimCursor.current.transform.position.x < transform.position.x && FacingRight) {
            Flip();
        }
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
