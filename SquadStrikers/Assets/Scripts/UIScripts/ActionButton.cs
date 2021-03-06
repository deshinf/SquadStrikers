﻿using UnityEngine;
using UnityEngine.UI; // <-- you need this to access UI (button in this case) functionalities
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ActionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	
	public Color baseColour, highlightColour, pressedColour, selectedBaseColour, selectedHighlightColour, selectedPressedColour;
	[SerializeField] private bool _isSelected;
	public bool isSelected {
		get { return _isSelected; }
		set {
			_isSelected = value;
			ColorBlock cb = gameObject.GetComponent<Button> ().colors;
			if (_isSelected) {
				Debug.Log ("Colour Change");
				cb.normalColor = selectedBaseColour;
				cb.highlightedColor = selectedHighlightColour;
				cb.pressedColor = selectedPressedColour;
			} else {
				cb.normalColor = baseColour;
				cb.highlightedColor = highlightColour;
				cb.pressedColor = pressedColour;
			}
			gameObject.GetComponent<Button> ().colors = cb;
		}
	}

	public char[] hotkeys;
	Button myButton;
	private PCHandler.Action _action;
	public char hotkey;
	public bool hasHotkey;
	public PCHandler.Action action {
		get {
			return _action;
		}
		set {
			string text = "";
			if (hasHotkey) {
				text = "(" + hotkey.ToString () + ") ";
			}
			_action = value;
			if (_action.attachedItem) {
				if (_action.actionName == "Pick up Item") {
					text += "Pick up " + _action.attachedItem.itemName;
				} else if (_action.actionName == "Drop Item") {
					if (_action.attachedItem == GameObject.FindGameObjectWithTag ("BoardHandler").GetComponent<BoardHandler> ().selectedUnit ().ancientMagic) {
						//Active Ancient Magics can only be discharged, not dropped.
						text += "Discharge " + _action.attachedItem.itemName;
					}
					text += "Drop " + _action.attachedItem.itemName;
				} else if (_action.actionName == "Discharge Ancient Magic") {
					text += "Discharge " + _action.attachedItem.itemName;
				} else if (_action.actionName == "Activate Ancient Magic") {
					text += "Activate " + _action.attachedItem.itemName;
				} else {
					text += _action.attachedItem.itemName;
				}
			} else {
				 text += _action.actionName;
			}
			gameObject.GetComponentInChildren<Text> ().text = text;
		}
	}

	void Awake()
	{
		myButton = GetComponent<Button>(); // <-- you get access to the button component here

		myButton.onClick.AddListener( () => {respond();} );  // <-- you assign a method to the button OnClick event here
	}

	public void OnPointerEnter (PointerEventData data) {
		if (!(action.actionName == "Cancel")) {
			GameObject.FindGameObjectWithTag ("StatsBar").GetComponent<StatsBar> ().displayUnitAndAction(GameObject.FindGameObjectWithTag("BoardHandler").GetComponent<BoardHandler>().selectedUnit(), action);
		}
	}

	public void OnPointerExit (PointerEventData data) {
		GameObject.FindGameObjectWithTag ("StatsBar").GetComponent<StatsBar> ().displayStats(GameObject.FindGameObjectWithTag("BoardHandler").GetComponent<BoardHandler>().selectedUnit());
	}

	void Update() {
		//if (Input.GetKey (KeyCode.Escape) && action.actionName == "Do Nothing") {
		//	GameObject.FindGameObjectWithTag ("BoardHandler").GetComponent<ActionHandler> ().PerformAction (action);		
		//}
		if (hasHotkey) {
			List<char> numbers = new List<char> (){ '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
			if (numbers.Contains (hotkey)) {
				if (Input.GetKeyDown ((KeyCode)System.Enum.Parse (typeof(KeyCode), "Keypad" + hotkey.ToString ())) ||
				    Input.GetKeyDown ((KeyCode)System.Enum.Parse (typeof(KeyCode), "Alpha" + hotkey.ToString ()))) {
					respond ();
				}
			} else if (Input.GetKeyDown ((KeyCode)System.Enum.Parse (typeof(KeyCode), hotkey.ToString ().ToUpper ()))) {
				respond ();
			}
			if (isSelected && (Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.KeypadEnter))) {
				respond ();
			}
		}
	}

	public void respond()
	{
		if (!isSelected) {
			isSelected = GameObject.FindGameObjectWithTag ("BoardHandler").GetComponent<ActionHandler> ().SelectAction (action);
		} else {
			GameObject.FindGameObjectWithTag ("BoardHandler").GetComponent<ActionHandler> ().TriggerAbility (null);
		}
	}

	public void setHotkey(int i) {
		if (i < hotkeys.Length) {
			hotkey = hotkeys [i];
			hasHotkey = true;
		} else {
			hotkey = '\0';
			hasHotkey = false;
		}
	}
}