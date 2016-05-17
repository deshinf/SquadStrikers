using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PauseManuScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Button saveButton = gameObject.transform.Find ("SaveButton").GetComponent<Button> ();
		Button loadButton = gameObject.transform.Find ("LoadButton").GetComponent<Button> ();
		saveButton.onClick.AddListener (() => GameObject.FindGameObjectWithTag ("IOHandler").GetComponent<IOScript> ().SaveGame ());
		loadButton.onClick.AddListener (() => GameObject.FindGameObjectWithTag ("IOHandler").GetComponent<IOScript> ().LoadGame ("test"));
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
