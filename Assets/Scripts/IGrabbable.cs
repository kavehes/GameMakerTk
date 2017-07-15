using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IGrabbable {

    bool isDead();
    void Grab(GameObject head);
    void UnGrab();
    void Bite(float damage);
    void Throw(Vector3 direction, float strength);
}
