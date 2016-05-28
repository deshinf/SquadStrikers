using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Action = PCHandler.Action;

public class SaveGamesListScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Fill ();
	}
	public GameObject saveGameButtonPrefab;
	public int gap = 0;
	public List<GameObject> saveGameButtons;

	public void Fill() {
		Empty ();
		float buttonHeight = saveGameButtonPrefab.GetComponent<RectTransform> ().rect.height;
		float menuHeight = gameObject.GetComponent<RectTransform> ().rect.height;
		float verticalOffset = buttonHeight/2;
		int i = 0;
		foreach (string name in GameObject.FindGameObjectWithTag("IOHandler").GetComponent<IOScript>().ListOfSaves()) {
			GameObject button = ((GameObject) Instantiate (saveGameButtonPrefab, new Vector2 (0f, menuHeight/2 - verticalOffset), Quaternion.identity));
			button.GetComponent<SaveGameButton> ().saveName = name;
			button.transform.SetParent (gameObject.transform,false);
			button.GetComponent<RectTransform> ().anchorMin = new Vector2 (0.01f, (menuHeight - verticalOffset)/menuHeight);
			button.GetComponent<RectTransform> ().anchorMax = new Vector2 (0.99f, (menuHeight - verticalOffset)/menuHeight);
			button.GetComponent<RectTransform> ().offsetMin = new Vector2 (0f, -buttonHeight / 2);
			button.GetComponent<RectTransform> ().offsetMax = new Vector2 (0f, buttonHeight / 2);
			saveGameButtons.Add (button);
			verticalOffset += buttonHeight;
			i++;
		}
	}

	public void Empty() {
		foreach (GameObject button in saveGameButtons) {
			Object.Destroy (button);
		}
		saveGameButtons.Clear ();
	}


	// Update is called once per frame
	void Update () {

	}
}
