using UnityEngine;
using System.Collections;

public class MiscKeyHandler : MonoBehaviour {
	GameObject pauseMenu;

	// Use this for initialization
	void Start () {
		pauseMenu = GameObject.FindGameObjectWithTag ("PauseMenu");
		pauseMenu.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			pauseMenu.SetActive (!pauseMenu.activeSelf);
		}
	}
}
