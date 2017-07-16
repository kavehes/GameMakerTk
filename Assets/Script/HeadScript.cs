using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DistanceJoint2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class HeadScript : MonoBehaviour {

    List<Vector2> CollidingPoint = new List<Vector2>();

    public List<DistanceJoint2D> NeckNodes = new List<DistanceJoint2D>();
    public GameObject Body;
    LamaController lama;

    public float maxDistance;

    Rigidbody2D rigid;

    public int neckLayer;
    public LayerMask layersToCheck;

    public float throwStrength = 7000;

    [Header("Grabbing")]
    GameObject grabbedObject;

    public CircleCollider2D[] cc;

    void Awake() {
        NeckNodes.Add(GetComponent<DistanceJoint2D>());
        lama = Body.GetComponent<LamaController>();
        maxDistance = lama.neckMaxLength;
        rigid = GetComponent<Rigidbody2D>();
        cc = GetComponents<CircleCollider2D>();
        NeckNodes[0].distance = maxDistance;
    }

    void FixedUpdate() {
        //HARDFIX
        if (headState() == LamaController.HeadState.Attached && NeckNodes.Count > 1) {
            NeckNodes[0].connectedBody = lama.GetComponent<Rigidbody2D>();
            NeckNodes[0].connectedAnchor = NeckNodes[NeckNodes.Count - 1].connectedAnchor;
            for (int i = 1; i < NeckNodes.Count; i++) {
                Destroy(NeckNodes[i].gameObject);
                NeckNodes.RemoveAt(i);
                CollidingPoint.RemoveAt(i - 1);
            }
        }
        if(headState() != LamaController.HeadState.Attached){
            SimplifyJoints();
            if (headState() != LamaController.HeadState.CommingBack) {
                for (int i = 0; i < NeckNodes.Count - 1; i++) {
                    Transform childPos = NeckNodes[i].connectedBody.transform;
                    RaycastHit2D rchit = Physics2D.Raycast(NeckNodes[i].transform.position, (childPos.position - NeckNodes[i].transform.position).normalized, (childPos.position - NeckNodes[i].transform.position).magnitude, layersToCheck);

                    if (rchit.collider != null && rchit.collider.gameObject != childPos.gameObject) {
                        CreateJoint(i + 1, rchit.point, getRectOffset(rchit.collider, rchit.point));
                        break;
                    }
                }

                RaycastHit2D hit = Physics2D.Raycast(NeckNodes[NeckNodes.Count - 1].transform.position, Body.transform.position - NeckNodes[NeckNodes.Count - 1].transform.position, (Body.transform.position - NeckNodes[NeckNodes.Count - 1].transform.position).magnitude, layersToCheck);

                if (hit.collider != null && hit.collider.gameObject != Body) {
                    CreateJoint(NeckNodes.Count, hit.point, getRectOffset(hit.collider, hit.point));
                    NeckNodes[NeckNodes.Count - 1].connectedAnchor = NeckNodes[NeckNodes.Count - 2].connectedAnchor;
                    NeckNodes[NeckNodes.Count - 2].connectedAnchor = Vector2.zero;
                }
            }

            if (headState() == LamaController.HeadState.CommingBack || headState() == LamaController.HeadState.Launched) {
                NeckNodes[0].distance = maxDistance - GetDistance(1, NeckNodes.Count);
            }

            else if (headState() == LamaController.HeadState.Grabbing) {
                MaxOnlyManager(false, true, true);
                maxDistance = Mathf.Clamp(maxDistance, lama.neckSize, lama.neckMaxLength);
                if (grabbedObject != null) {
                    //transform.position = grabbedObject.transform.position;
                    transform.position = grabbedObject.GetComponent<BoxCollider2D>().bounds.center;
                    rigid.velocity = Vector2.zero;

                    NeckNodes[NeckNodes.Count - 1].distance = maxDistance - GetDistance(0, NeckNodes.Count - 1);
                    NeckNodes[NeckNodes.Count - 1].distance = Mathf.Clamp(NeckNodes[NeckNodes.Count - 1].distance, lama.neckSize, lama.neckMaxLength);

                    if (GetDistance(0, NeckNodes.Count) <= lama.neckSize * 2f) {
                        Debug.Log("Ungrabbing");
                        foreach (CircleCollider2D c in cc) {
                            c.enabled = true;
                        }
                        grabbedObject.GetComponent<IGrabbable>().UnGrab();
                        lama.headState = LamaController.HeadState.CommingBack;
                        StartCoroutine(Rewind());
                    }
                    if(NeckNodes.Count == 1) {
                        Debug.Log(lama.GetComponent<Rigidbody2D>().velocity);
                        Vector3 direction = lama.transform.position - NeckNodes[0].transform.position + (Vector3)lama.GetComponent<Rigidbody2D>().velocity*100;
                        direction = Quaternion.FromToRotation(direction, direction + Vector3.down) * direction;
                        if (direction.magnitude > NeckNodes[0].distance)
                            lama.transform.position = NeckNodes[0].transform.position + direction.normalized * NeckNodes[0].distance;
                    }
                    maxDistance = Mathf.Clamp(maxDistance, lama.neckSize, lama.neckMaxLength);
                }
                else {
                    Debug.LogError("No grabedObject");
                    lama.headState = LamaController.HeadState.CommingBack;
                    StartCoroutine(Rewind());
                }
            }
        }
        if (NeckNodes.Count > 1) {
            if (lama.transform.position.x < NeckNodes[NeckNodes.Count - 1].transform.position.x && !lama.FacingRight) {
                lama.Flip();
            }
            if (lama.transform.position.x > NeckNodes[NeckNodes.Count - 1].transform.position.x && lama.FacingRight) {
                lama.Flip();
            }
        }

        SimplifyJoints();
    }
    /// <summary>
    /// Delete the last joint when climbing a ledge, may fuck things up
    /// </summary>
    public void DeleteJoint(int index) {
        NeckNodes[index-1].connectedBody = NeckNodes[index].connectedBody;
        NeckNodes[index-1].connectedAnchor = NeckNodes[index].connectedAnchor;
        Destroy(NeckNodes[index].gameObject);
        NeckNodes.RemoveAt(index);
        CollidingPoint.RemoveAt(index - 1);
    }

    public void EnableJoint(bool b) {
        NeckNodes[0].enabled = b;
    }

    /// <summary>
    /// Delete the last joint
    /// </summary>
    public void DeleteJoint() {
        if (NeckNodes.Count > 1) {
            int index = NeckNodes.Count - 1;
            NeckNodes[index - 1].connectedBody = NeckNodes[index].connectedBody;
            NeckNodes[index - 1].connectedAnchor = NeckNodes[index].connectedAnchor;
            Destroy(NeckNodes[index].gameObject);
            NeckNodes.RemoveAt(index);
            CollidingPoint.RemoveAt(index - 1);
        }
    }

    public void ShortenDistance(float value) {
        maxDistance = GetDistance(0, NeckNodes.Count);
        maxDistance -= value;
    }

    void MaxOnlyManager(bool first,bool middle, bool last) {
        for(int i = 0; i < NeckNodes.Count; i++) {
            if (i == 0)
                NeckNodes[i].maxDistanceOnly = first;
            if(i == NeckNodes.Count -1)
                NeckNodes[i].maxDistanceOnly = last;
            else
                NeckNodes[i].maxDistanceOnly = middle;
        }
    }

    LamaController.HeadState headState() {
        return lama.headState;
    }

    Vector2 getRectOffset(Collider2D col, Vector2 point) {
        Vector2 v;
        GameObject obj = col.gameObject;
        v = point -(Vector2) obj.transform.position;
        v = new Vector2(Mathf.Sign(v.x), Mathf.Sign(v.y));
        
        return v*0.4f;
    }

    /// <summary>
    /// Bring the head back to the body
    /// </summary>
    public IEnumerator Rewind() {
        NeckNodes[0].distance = (NeckNodes[0].transform.position - NeckNodes[0].connectedBody.transform.position).magnitude;
        maxDistance = GetDistance(0, NeckNodes.Count);
        while (GetDistance(0, NeckNodes.Count)> lama.neckSize) {
            maxDistance -= Time.deltaTime*10;
            yield return new WaitForFixedUpdate();
        }
        lama.headState = LamaController.HeadState.Attached;
        rigid.gravityScale = 0;
        maxDistance = lama.neckMaxLength;
        NeckNodes[0].distance = lama.neckMaxLength;
        Debug.Log("Head is Back");
        yield return new WaitForFixedUpdate();
    }

    void SimplifyJoints() {
        for (int i = 0; i < NeckNodes.Count - 2; i++){
            bool cLeft = isLeft(NeckNodes[i].transform.position, NeckNodes[i + 1].transform.position, CollidingPoint[i]);
            if (cLeft != isLeft(NeckNodes[i].transform.position, NeckNodes[i + 1].transform.position, (Vector2)NeckNodes[i + 1].connectedBody.transform.position)) {
                NeckNodes[i].connectedBody = NeckNodes[i + 1].connectedBody;
                Destroy(NeckNodes[i + 1].gameObject);
                NeckNodes.RemoveAt(i + 1);
                CollidingPoint.RemoveAt(i);
                Debug.Log("Simplify 1");
                }
        }
        //Last case
        if (NeckNodes.Count > 1) {
            int ii = NeckNodes.Count - 2;
            bool ccLeft = isLeft(NeckNodes[ii].transform.position, NeckNodes[ii + 1].transform.position, CollidingPoint[ii]);
            if (ccLeft != isLeft(NeckNodes[ii].transform.position, NeckNodes[ii + 1].transform.position, (Vector2)Body.transform.position)) {
                NeckNodes[ii].connectedBody = NeckNodes[ii + 1].connectedBody;
                Destroy(NeckNodes[ii + 1].gameObject);
                NeckNodes.RemoveAt(ii + 1);
                CollidingPoint.RemoveAt(ii);
                Debug.Log("Simplify 2");
            }
        }

        for (int i = 1; i < NeckNodes.Count; i++) {
            if(NeckNodes[i].distance < 0.1f) {
                NeckNodes[i - 1].connectedBody = NeckNodes[i].connectedBody;
                NeckNodes[i - 1].anchor= NeckNodes[i].anchor;
                Destroy(NeckNodes[i].gameObject);
                NeckNodes.RemoveAt(i);
                CollidingPoint.RemoveAt(i - 1);
                Debug.Log("Simplify 3");
            }
        }
        if(NeckNodes[0].distance < 0.1f && NeckNodes.Count > 1) {
            NeckNodes[0].connectedBody = NeckNodes[1].connectedBody;
            NeckNodes[0].anchor = NeckNodes[1].anchor;
            Destroy(NeckNodes[1].gameObject);
            NeckNodes.RemoveAt(1);
            CollidingPoint.RemoveAt(0);
            Debug.Log("Simplify 4");
        }
    }

    bool isLeft(Vector3 a, Vector3 b, Vector2 c) {
        return ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)) > 0;
    }

    public void UnGrab() {
        if (grabbedObject != null) {
            Debug.Log("Ungrabbing");
            foreach (CircleCollider2D c in cc) {
                c.enabled = true;
            }
            grabbedObject.GetComponent<IGrabbable>().UnGrab();
            lama.headState = LamaController.HeadState.CommingBack;
            StartCoroutine(Rewind());
        }
    }

    /// <summary>
    /// Eat an ennemy
    /// </summary>
    public bool Eat() {
        if (grabbedObject != null) {
            AkSoundEngine.PostEvent("Play_Gobe", gameObject);
            grabbedObject.GetComponent<IGrabbable>().Bite(10f);
            grabbedObject.SetActive(false);
            return true;           
        }
        else
            return false;
    }

    public void Throw(Vector2 direction) {
        if(grabbedObject != null) {
            Debug.Log("Throwing stuff");
            AkSoundEngine.PostEvent("Play_Spit", gameObject);
            grabbedObject.SetActive(true);
            grabbedObject.transform.position = transform.position;
            grabbedObject.GetComponent<IGrabbable>().Throw((Vector3)direction, throwStrength);
        }
        else {
            Debug.Log("Wait what");

        }
    }

    void CreateJoint(int index, Vector2 pos, Vector2 offset) {
        GameObject joint = new GameObject("Joint " + NeckNodes.Count);
        
        joint.transform.position = pos + offset;
        joint.transform.parent = transform.parent;
        joint.layer = neckLayer;
        /*
        CircleCollider2D cc2d = joint.AddComponent<CircleCollider2D>();
        cc2d.radius = 0.25f;
        */
        CollidingPoint.Insert(index-1,pos);

        Rigidbody2D rig = joint.AddComponent<Rigidbody2D>();
        rig.isKinematic = true;

        NeckNodes.Insert(index,joint.AddComponent<DistanceJoint2D>());
        NeckNodes[index].connectedBody = NeckNodes[index-1].connectedBody;
        NeckNodes[index].connectedAnchor = NeckNodes[index - 1].connectedAnchor;
        NeckNodes[index - 1].connectedAnchor = Vector2.zero;
        NeckNodes[index].distance = (NeckNodes[index - 1].connectedBody.transform.position - NeckNodes[index].transform.position).magnitude;
        NeckNodes[index].autoConfigureDistance = false;
        NeckNodes[index -1].connectedBody = rig;
    }

    bool CanExtend() {
        float nodesDistance = 0;
        for (int i = 0; i < NeckNodes.Count; i++) {
            nodesDistance += (NeckNodes[i].transform.position - NeckNodes[i].connectedBody.transform.position).magnitude;
        }
        return nodesDistance < maxDistance;
    }

    public void ReduceFirstJoint(float value) {
        NeckNodes[NeckNodes.Count-1].distance -= value;
        NeckNodes[0].distance += value;
    }

    float GetDistance(int start, int end) {
        if (start < 0)
            start = 0;
        if (end > NeckNodes.Count)
            end = NeckNodes.Count;

        float lastDistance = 0;
        for (int i = start; i < end; i++) {
            lastDistance += (NeckNodes[i].transform.position - NeckNodes[i].connectedBody.transform.position).magnitude;
        }

        return lastDistance;
    }

    void OnDrawGizmos() {
        if (Body != null && NeckNodes.Count > 0) {
            for (int i = 0; i < NeckNodes.Count - 1; i++) {
                Gizmos.DrawLine(NeckNodes[i].transform.position, NeckNodes[i + 1].transform.position);
            }
            Gizmos.color = Color.red;
            Gizmos.DrawLine(NeckNodes[NeckNodes.Count - 1].transform.position, Body.transform.position);

        }
        for(int i = 0; i < CollidingPoint.Count; i++) {
            Gizmos.DrawSphere(CollidingPoint[i], 0.1f);
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (headState() == LamaController.HeadState.Launched) {
            IGrabbable grab = other.GetComponent<IGrabbable>();
            if (grab != null) {
                Debug.Log("Grabbed");
                //Alors on grab le bouzingue
                grab.Grab(gameObject);
                grabbedObject = other.gameObject;

                foreach (CircleCollider2D c in cc) {
                    c.enabled = false;
                }
                maxDistance = GetDistance(0, NeckNodes.Count);

                rigid.velocity = Vector2.zero;

                lama.headState = LamaController.HeadState.Grabbing;
            }
        }
    }
}
