using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimCursor : MonoBehaviour {

    public static AimCursor current;

    public float cursorZPosition = 5;

    void Start() {
        current = this;
    }

    void Update() {
        transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + Vector3.forward * cursorZPosition;
    }
}
