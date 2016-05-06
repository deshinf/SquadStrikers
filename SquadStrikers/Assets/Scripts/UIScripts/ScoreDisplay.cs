using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour {

	private bool _first_update;

	// Use this for initialization
	void Start () {
	}

	public void UpdateDisplay () {
		int score = GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().score;
		gameObject.GetComponent<Text>().text = "Substance X: " + score.ToString ();

	}

	// Update is called once per frame
	void Update () {
		if (!_first_update) {
			UpdateDisplay ();
			_first_update = true;
		}
	}
}
