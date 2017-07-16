using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LamaRenderer : MonoBehaviour {

    HeadScript hs;
    public LamaController lama;

    public int partCount;

    public Sprite part0;
    public Sprite part1;
    public Sprite part2;
    public Sprite part3;
    public Sprite part4;

    public Transform Neck;

    public int spritePerMeter = 5;

    SpriteRenderer[] renderers;

    SpriteRenderer headSPR;

    List<DistanceJoint2D> NeckNodes;

    void Start() {
        GameObject holder = new GameObject("Holder");
        holder.transform.parent = transform.parent;

        headSPR = GetComponent<SpriteRenderer>();
        hs = GetComponent<HeadScript>();
        NeckNodes = hs.NeckNodes;
        renderers = new SpriteRenderer[partCount];
        for (int i = 0; i < partCount; i++) {
            GameObject newPart = new GameObject("NeckPart_" + i);
            SpriteRenderer spr = newPart.AddComponent<SpriteRenderer>();
            int max = partCount;

            if(i == 0) { spr.sprite = part0; spr.sortingOrder = 4; }
            else if (i == 1) { spr.sprite = part1; spr.sortingOrder = 2; }
            else { spr.sprite = part2; }
            renderers[i] = spr;
            spr.transform.parent = holder.transform;
        }
    }

    void Update() {
        headSPR.flipX = !lama.FacingRight;
        int index = 0;
        float distance;
        Quaternion rotation;

        for (int i = 0; i < NeckNodes.Count - 1; i++) {
            rotation = Quaternion.FromToRotation(Vector3.down, NeckNodes[i].connectedBody.transform.position - NeckNodes[i].transform.position);
            distance = Vector3.Distance(NeckNodes[i].transform.position, NeckNodes[i].connectedBody.transform.position);

            if (i == 0)
                headSPR.transform.rotation = rotation;

            if (spritePerMeter > 0) {
                for (int j = 0; j < Mathf.CeilToInt(spritePerMeter * distance); j++) {
                    if (index < partCount) {
                        renderers[index].gameObject.SetActive(true);
                        renderers[index].flipX = !lama.FacingRight;
                        renderers[index].transform.position = Vector3.Lerp(NeckNodes[i].transform.position, NeckNodes[i].connectedBody.transform.position, (float)j / Mathf.CeilToInt(spritePerMeter * distance));
                        renderers[index].transform.rotation = rotation;
                    }
                    index++;
                }
            }
            else { spritePerMeter = 1; }
        }
        //And the last one
        rotation = Quaternion.FromToRotation(Vector3.down, Neck.position - NeckNodes[NeckNodes.Count - 1].transform.position);
        distance = Vector3.Distance(NeckNodes[NeckNodes.Count - 1].transform.position,Neck.position);
        if(NeckNodes.Count == 1)
            headSPR.transform.rotation = rotation;

        if (spritePerMeter > 0) {
            for (int j = 0; j < Mathf.CeilToInt(spritePerMeter * distance); j++) {
                if (index < partCount) {
                    renderers[index].gameObject.SetActive(true);
                    renderers[index].flipX = !lama.FacingRight;
                    renderers[index].transform.position = Vector3.Lerp(NeckNodes[NeckNodes.Count - 1].transform.position, Neck.position, (float)j / Mathf.CeilToInt(spritePerMeter * distance));
                    renderers[index].transform.rotation = rotation;
                    if(j == Mathf.CeilToInt(spritePerMeter * distance) - 1) { renderers[index].sprite = part3; renderers[index].sortingOrder = 2; }
                    else if (j == Mathf.CeilToInt(spritePerMeter * distance)) { renderers[index].sprite = part4; renderers[index].sortingOrder = 4; }
                    else { renderers[index].sprite = part2; renderers[index].sortingOrder = 0; }
                }
                index++;
            }
        }
        else { spritePerMeter = 1; }

        Neck.transform.rotation = rotation;

        while (index < partCount) {
            renderers[index].gameObject.SetActive(false);
            index++;
        }
    }
    
}
