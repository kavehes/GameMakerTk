using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DistanceJoint2D))]
public class HeadScript : MonoBehaviour {

    List<Vector2> CollidingPoint = new List<Vector2>();

    List<DistanceJoint2D> NeckNodes = new List<DistanceJoint2D>();
    public GameObject Body;
    LamaController lama;

    float maxDistance;
    
    public int neckLayer;
    public LayerMask layersToCheck;

    void Start() {
        NeckNodes.Add(GetComponent<DistanceJoint2D>());
        lama = Body.GetComponent<LamaController>();
        maxDistance = lama.neckMaxLength;
    }

    void FixedUpdate() {
        SimplifyJoints();


        //Crée les joints, je suis plus trop sur de pourquoi on va pas au bout
        for (int i = 0; i < NeckNodes.Count - 1; i++) {
            Debug.Log(i);
            Transform childPos = NeckNodes[i].connectedBody.transform;
            //Change the distance to match the possible distances
            RaycastHit2D rchit = Physics2D.Raycast(NeckNodes[i].transform.position, (childPos.position - NeckNodes[i].transform.position).normalized, (childPos.position - NeckNodes[i].transform.position).magnitude, layersToCheck);

            if (rchit.collider != null && rchit.collider.gameObject != childPos.gameObject) {
                CreateJoint(i + 1, rchit.point, (rchit.point - (Vector2)rchit.collider.transform.position).normalized * 0.25f);
                break;
            }
        }
        RaycastHit2D hit = Physics2D.Raycast(NeckNodes[NeckNodes.Count - 1].transform.position, Body.transform.position - NeckNodes[NeckNodes.Count - 1].transform.position, maxDistance, layersToCheck);

        if (hit.collider != null && hit.collider.gameObject != Body) {

            CreateJoint(NeckNodes.Count, hit.point, (hit.point - (Vector2)hit.collider.transform.position).normalized * 0.25f);
            NeckNodes[NeckNodes.Count - 1].anchor = NeckNodes[NeckNodes.Count - 2].anchor;
            NeckNodes[NeckNodes.Count - 2].anchor = Vector2.zero;
        }
        if (headState() == LamaController.HeadState.Launched) {
            //Set the distance for the last joint
            float lastDistance = 0;
            for (int i = 1; i < NeckNodes.Count; i++) {
                lastDistance += (NeckNodes[i].transform.position - NeckNodes[i].connectedBody.transform.position).magnitude;
            }

            NeckNodes[0].distance = maxDistance - lastDistance;
        }
    }

    LamaController.HeadState headState() {
        return lama.headState;
    }

    void SimplifyJoints() {
        for (int i = 0; i < NeckNodes.Count - 2; i++){
            Vector3 sepLine = NeckNodes[i].transform.position - NeckNodes[i + 1].transform.position;
            bool cLeft = isLeft(NeckNodes[i].transform.position, NeckNodes[i + 1].transform.position, CollidingPoint[i+1]);
            if (cLeft != isLeft(NeckNodes[i].transform.position, NeckNodes[i + 1].transform.position, (Vector2)NeckNodes[i + 2].transform.position)) {
                NeckNodes[i].connectedBody = NeckNodes[i + 1].connectedBody;
                Destroy(NeckNodes[i + 1].gameObject);
                NeckNodes.RemoveAt(i + 1);
            }
            //PEnse à virer les gameObject aussi
        }
    }

    bool isLeft(Vector3 a, Vector3 b, Vector2 c) {
        return ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)) > 0;
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
        CollidingPoint.Add(pos);

        Rigidbody2D rig = joint.AddComponent<Rigidbody2D>();
        rig.isKinematic = true;

        NeckNodes.Insert(index,joint.AddComponent<DistanceJoint2D>());

        NeckNodes[index].connectedBody = NeckNodes[index-1].connectedBody;
        NeckNodes[index -1].connectedBody = rig;
    }

    bool CanExtend() {
        float nodesDistance = 0;
        for (int i = 0; i < NeckNodes.Count; i++) {
            nodesDistance += (NeckNodes[i].transform.position - NeckNodes[i].connectedBody.transform.position).magnitude;
        }
        return nodesDistance < maxDistance;
    }

    void OnDrawGizmos() {
        if(Body!= null && NeckNodes.Count >0) {
            for (int i = 0; i < NeckNodes.Count - 1; i++) {
                Gizmos.DrawLine(NeckNodes[i].transform.position, NeckNodes[i + 1].transform.position);
            }
            Gizmos.color = Color.red;
            Gizmos.DrawLine(NeckNodes[NeckNodes.Count - 1].transform.position, Body.transform.position);

        }
    }


}
