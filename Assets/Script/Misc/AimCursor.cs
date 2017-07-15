using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimCursor : MonoBehaviour {

    public int playerNumber = 1;

    public static AimCursor current;

    public float cursorZPosition = 5;
    public LamaController lama;

    void Start() {
        current = this;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject g in players) {
            LamaController l = g.GetComponent<LamaController>();
            if(l.playerNumber == playerNumber) {
                lama = l;
            }
        }
    }

    void Update() {
        //transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + Vector3.forward * cursorZPosition;
        if ((Mathf.Abs(Input.GetAxis("Horizontal2_P" + playerNumber)) + Mathf.Abs(Input.GetAxis("Vertical2_P" + playerNumber))) > 0.5f)
            transform.position = lama.transform.position + new Vector3(Input.GetAxis("Horizontal2_P" + playerNumber), Input.GetAxis("Vertical2_P" + playerNumber), 0).normalized*2f;
    }
}
