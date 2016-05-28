using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DeleteSaveScript : MonoBehaviour {

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
		myButton.onClick.AddListener( () => deleteSave());  // <-- you assign a method to the button OnClick event here
	}

	void deleteSave() {
		GameObject.FindGameObjectWithTag("IOHandler").GetComponent<IOScript>().DeleteSave(GameObject.FindGameObjectWithTag ("SaveNamePanel").GetComponent<InputField> ().text);
		GameObject.FindGameObjectWithTag ("SaveGamePanel").GetComponent<SaveGamesListScript> ().Fill ();
	}
}
