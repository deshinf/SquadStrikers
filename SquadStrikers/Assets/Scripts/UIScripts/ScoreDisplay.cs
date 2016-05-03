using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour {

	// Use this for initialization
	void Start () {
		UpdateDisplay ();
	}

	public void UpdateDisplay () {
		int score = GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().score;
		gameObject.GetComponent<Text>().text = "Substance X: " + score.ToString ();

	}

	// Update is called once per frame
	void Update () {
	
	}
}
