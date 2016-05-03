using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Action = PCHandler.Action;

public class ActionBar : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	public GameObject actionButtonPrefab;
	public int gap = 0;
	public List<GameObject> actionButtons;

	public void Fill() {
		Empty ();
		List<Action> actions = GameObject.FindGameObjectWithTag("BoardHandler").GetComponent<BoardHandler>().selectedUnit().actionList;
		float buttonHeight = actionButtonPrefab.GetComponent<RectTransform> ().rect.height;
		float menuHeight = gameObject.GetComponent<RectTransform> ().rect.height;
		float verticalOffset = buttonHeight/2;
		foreach (Action a in actions) {
			GameObject button = ((GameObject) Instantiate (actionButtonPrefab, new Vector2 (0f, menuHeight/2 - verticalOffset), Quaternion.identity));
			button.GetComponent<ActionButton> ().action = a;
			button.transform.SetParent (gameObject.transform,false);
			actionButtons.Add (button);
			verticalOffset += buttonHeight;
		}
	}
	public void DropItemSelector () {
		Empty ();

		List<Action> actions = new List<Action>();
		List<Item> inventory = GameObject.FindGameObjectWithTag("BoardHandler").GetComponent<BoardHandler>().selectedUnit().inventory;
		foreach (Item i in inventory) {
			actions.Add (new Action("Drop Item", "", i));
		}
		actions.Add(new Action("Cancel", "Cancels this action and goes back to the action menu.", null));
		float buttonHeight = actionButtonPrefab.GetComponent<RectTransform> ().rect.height;
		float menuHeight = gameObject.GetComponent<RectTransform> ().rect.height;
		float verticalOffset = buttonHeight/2;
		foreach (Action a in actions) {
			GameObject button = ((GameObject) Instantiate (actionButtonPrefab, new Vector2 (0f, menuHeight/2 - verticalOffset), Quaternion.identity));
			button.GetComponent<ActionButton> ().action = a;
			button.transform.SetParent (gameObject.transform,false);
			actionButtons.Add (button);
			verticalOffset += buttonHeight;
		}

	}

	public void SetToCancel() {
		Empty ();
		float buttonHeight = actionButtonPrefab.GetComponent<RectTransform> ().rect.height;
		float menuHeight = gameObject.GetComponent<RectTransform> ().rect.height;
		GameObject button = ((GameObject) Instantiate (actionButtonPrefab, new Vector2 (0f, menuHeight/2 - buttonHeight/2), Quaternion.identity));
		button.GetComponent<ActionButton> ().action = new Action("Cancel", "Cancels this action and goes back to the action menu.", null);
		button.transform.SetParent (gameObject.transform,false);
		actionButtons.Add (button);
	}

	public void Empty() {
		foreach (GameObject button in actionButtons) {
			Object.Destroy (button);
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}
