using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class VictoryScreenScript : MonoBehaviour {


	public void QuitGame () {
		Application.Quit ();
	}

	public void RestartGame () {
		foreach (GameObject go in GameObject.FindGameObjectsWithTag("PlayerTeam")) {
			Destroy (go);
		}
		SceneManager.LoadScene ("CharacterCreatorScene");
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
