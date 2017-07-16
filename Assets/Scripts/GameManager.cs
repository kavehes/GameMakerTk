using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public LamaController Player1;
    public LamaController Player2;
    public Transform killPosition;
    public Transform highestPosition;
    bool end = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (end)
            return;
        float lowest = Mathf.Min(Player1.transform.position.y, Player2.transform.position.y);
        AkSoundEngine.SetRTPCValue("Lowest_Lama", (lowest - killPosition.position.y) / (highestPosition.position.y - killPosition.position.y));
        if (Player1.transform.position.y <= killPosition.position.y) {
            Debug.Log("Player 1 wins");
            Destroy(Player1.transform.parent.gameObject);
            Win();
        }
        else if (Player2.transform.position.y <= killPosition.position.y) {
            Debug.Log("Player 2 wins");
            Destroy(Player2.transform.parent.gameObject);
            Win();
        }
    }

    public void Win() {
        AkSoundEngine.PostEvent("Play_YAY", Camera.main.gameObject);
        StartCoroutine(End());
    }

    IEnumerator End() {
        end = true;
        yield return new WaitForSeconds(2);
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
