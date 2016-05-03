using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerTagScript : MonoBehaviour {

	public GridPanelScript[] otherGridPanels;

	// Use this for initialization
	void Start () {
		gameObject.GetComponent<Button>().onClick.AddListener( () => {choosePlayer();} );  // <-- you assign a method to the button OnClick event here
	}

	public void choosePlayer() {
		transform.parent.GetComponent<GridPanelScript> ().isActive = true;
		foreach (GridPanelScript gps in otherGridPanels) {
			gps.isActive = false;
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}
