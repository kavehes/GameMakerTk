using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimCursor : MonoBehaviour {

    public int playerNumber = 1;
    
    public float cursorZPosition = 5;
    public LamaController lama;

    SpriteRenderer spriteRen;

    public Vector3 offset;

    void Start() {
        spriteRen = GetComponent<SpriteRenderer>();
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject g in players) {
            LamaController l = g.GetComponent<LamaController>();
            if(l.playerNumber == playerNumber) {
                lama = l;
                lama.cursor = this;
            }
        }
        offset = Vector2.up * 2;
    }

    public Vector2 aimDirection() {
        return offset;
    }

    void Update() {
        if(lama.headState == LamaController.HeadState.Attached) {
            spriteRen.color = Color.white;
        }
        else {
            spriteRen.color = Color.clear;
        }
        //transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + Vector3.forward * cursorZPosition;
        if ((Mathf.Abs(Input.GetAxis("Horizontal2_P" + playerNumber)) + Mathf.Abs(Input.GetAxis("Vertical2_P" + playerNumber))) > 0.5f)
            offset = new Vector3(Input.GetAxis("Horizontal2_P" + playerNumber), Input.GetAxis("Vertical2_P" + playerNumber), 0).normalized*4f;
        transform.position = lama.neckStart.transform.position + offset;
    }
}
