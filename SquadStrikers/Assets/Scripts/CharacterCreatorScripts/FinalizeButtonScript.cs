using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FinalizeButtonScript : MonoBehaviour {

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
		myButton.onClick.AddListener( () => finalizeCharacterCreation());  // <-- you assign a method to the button OnClick event here
	}

	void finalizeCharacterCreation() {
		PlayerTeamScript team = GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ();
		foreach ( GameObject g in GameObject.FindGameObjectsWithTag("CharacterCreationPanel")) {
			CharacterCreator cc = g.GetComponent<CharacterCreator> ();
			GameObject created = cc.CreateCharacter ();
			int i = cc.index;
			team.defaultTeam [i] = created;
		}
		team.saveName = GameObject.FindGameObjectWithTag ("SaveNamePanel").GetComponent<InputField> ().text;
		//SceneManager.LoadScene ("LevelUp");
		GameObject.FindGameObjectWithTag ("IOHandler").GetComponent<IOScript> ().setUpToLoadIfExists();
		SceneManager.LoadScene ("MainScene");
	}
}
