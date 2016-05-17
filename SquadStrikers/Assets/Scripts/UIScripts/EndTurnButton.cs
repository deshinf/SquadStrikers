using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EndTurnButton : MonoBehaviour {
	Button myButton;

	// Use this for initialization
	void Start () {
	
	}


	void Awake()
	{
		myButton = GetComponent<Button>(); // <-- you get access to the button component here

		myButton.onClick.AddListener( () => {BoardHandler.GetBoardHandler().EndTurn();} );  // <-- you assign a method to the button OnClick event here
	}

	// Update is called once per frame
	void Update () {
	
	}
}
