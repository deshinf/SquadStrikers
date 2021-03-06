﻿using UnityEngine;
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
		int i = 0;
		foreach (Action a in actions) {
			GameObject button = ((GameObject) Instantiate (actionButtonPrefab, new Vector2 (0f, menuHeight/2 - verticalOffset), Quaternion.identity));
			button.GetComponent<ActionButton>().setHotkey (i);
			button.GetComponent<ActionButton> ().action = a;
			button.transform.SetParent (gameObject.transform,false);
			button.GetComponent<RectTransform> ().anchorMin = new Vector2 (0.01f, (menuHeight - verticalOffset)/menuHeight);
			button.GetComponent<RectTransform> ().anchorMax = new Vector2 (0.99f, (menuHeight - verticalOffset)/menuHeight);
			button.GetComponent<RectTransform> ().offsetMin = new Vector2 (0f, -buttonHeight / 2);
			button.GetComponent<RectTransform> ().offsetMax = new Vector2 (0f, buttonHeight / 2);
			actionButtons.Add (button);
			verticalOffset += buttonHeight;
			i++;
		}
		SelectDoNothing ();
	}

	public void SelectDoNothing () {
		UnselectAll ();
		foreach (GameObject a in actionButtons) {
			bool found = false;
			if (a.GetComponent<ActionButton> ().action.actionName == "Do Nothing") {
				GameObject.FindGameObjectWithTag ("BoardHandler").GetComponent<ActionHandler> ().SelectAction (a.GetComponent<ActionButton>().action);
				a.GetComponent<ActionButton> ().isSelected = true;
				found = true;
				break;
			}
			if (!found) {
				throw new UnityException ();
			}
		}
	}

	public void UnselectAll () {
		foreach (GameObject b in actionButtons) {
			b.GetComponent<ActionButton> ().isSelected = false;
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
		button.GetComponent<ActionButton> ().setHotkey (0);
		button.GetComponent<ActionButton> ().action = new Action("Cancel", "Cancels this action and goes back to the action menu.", null);
		button.transform.SetParent (gameObject.transform,false);
		actionButtons.Add (button);
	}

	public void Empty() {
		foreach (GameObject button in actionButtons) {
			Object.Destroy (button);
		}
		actionButtons.Clear ();
	}

	// Update is called once per frame
	void Update () {
	
	}
}
