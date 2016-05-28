using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EndTurnButton : MonoBehaviour {
	Button myButton;
	public KeyCode[] hotkeys;
	// Use this for initialization
	void Start () {
		hotkeys = new KeyCode[]{ KeyCode.Space };
	}


	void Awake()
	{
		myButton = GetComponent<Button>(); // <-- you get access to the button component here

		myButton.onClick.AddListener( () => {BoardHandler.GetBoardHandler().EndTurn();} );  // <-- you assign a method to the button OnClick event here
	}

	// Update is called once per frame
	void Update () {
		foreach(KeyCode k in hotkeys) {
			if (Input.GetKeyDown (k)) {
				BoardHandler.GetBoardHandler().EndTurn();
			}
		}
	}
}
