using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SaveGameButton : MonoBehaviour {
	Button myButton;
	private string _saveName;
	public string saveName {
		get {
			return _saveName;
		}
		set {
			_saveName = value;
			gameObject.GetComponentInChildren<Text> ().text = _saveName;
		}
	}

	void Awake()
	{
		myButton = GetComponent<Button>(); // <-- you get access to the button component here

		myButton.onClick.AddListener( () => {setSaveName();} );  // <-- you assign a method to the button OnClick event here
	}

	void Update() {
	}

	void setSaveName()
	{
		//Debug.Log ("Setting " + saveName);
		GameObject.FindGameObjectWithTag ("SaveNamePanel").GetComponent<InputField> ().text = saveName;
	}
}