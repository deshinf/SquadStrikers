using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RetireButtonScript : MonoBehaviour {

	// Use this for initialization
	Button myButton;

	void Start () {

	}

	// Update is called once per frame
	void Update () {

	}

	void Awake()
	{
		myButton = GetComponent<Button>(); // <-- you get access to the button component here
		myButton.onClick.AddListener( () => quitGame());  // <-- you assign a method to the button OnClick event here
	}

	void quitGame() {
		UnityEngine.SceneManagement.SceneManager.LoadScene ("DefeatScreen");
	}
}