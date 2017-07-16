using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
    public List<Transform> spawnPositions;
    public GameObject enemy1;
    public GameObject enemy2;
    public GameObject enemy3;
    public GameObject enemy4;

    List<GameObject> spawned = new List<GameObject>();
    public int total = 10;
    public float YLimit = -20;

    float timer = 0;
	
	// Update is called once per frame
	void Update () {
        timer += Time.deltaTime;
        deleteEmpty();
        if (timer > 1.0f) {
            Respawn();
            timer = 0;
        }
	}

    void deleteEmpty() {
        for(int i = 0; i < spawned.Count; i++) {
            if (spawned[i] == null) {
                spawned.RemoveAt(i);
                i--;
            }
            else if (spawned[i].transform.position.y < YLimit) {
                Destroy(spawned[i].gameObject);
                spawned.RemoveAt(i);
                i--;
            }
        }
    }

    void Respawn() {
        if(spawned.Count < total) {
            int randomNum = Random.Range(0, 100);
            GameObject toSpawn;
            if (randomNum < 40) toSpawn = enemy1;
            else if (randomNum < 70) toSpawn = enemy2;
            else if (randomNum < 90) toSpawn = enemy3;
            else toSpawn = enemy4;
            GameObject obj = Instantiate(toSpawn);
            obj.transform.position = spawnPositions[Random.Range(0, spawnPositions.Count)].position;
            spawned.Add(obj);
        }
    }
}
