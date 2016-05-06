using UnityEngine;
using UnityEngine.UI; // <-- you need this to access UI (button in this case) functionalities
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ActionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
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

		myButton.onClick.AddListener( () => {triggerAction();} );  // <-- you assign a method to the button OnClick event here
	}

	public void OnPointerEnter (PointerEventData data) {
		if (!(action.actionName == "Cancel")) {
			GameObject.FindGameObjectWithTag ("StatsBar").GetComponent<StatsBar> ().ActionDescription (action);
		}
	}

	public void OnPointerExit (PointerEventData data) {
		GameObject.FindGameObjectWithTag ("StatsBar").GetComponent<StatsBar> ().Revert ();
	}

	void Update() {
		//if (Input.GetKey (KeyCode.Escape) && action.actionName == "Do Nothing") {
		//	GameObject.FindGameObjectWithTag ("BoardHandler").GetComponent<ActionHandler> ().PerformAction (action);		
		//}
		if (hasHotkey) {
			List<char> numbers = new List<char> (){ '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
			if (numbers.Contains (hotkey)) {
				//if (Input.GetKeyDown ((KeyCode) System.Enum.Parse(typeof(KeyCode),"Keypad" + hotkey.ToString()))) {
				if (Input.GetKeyDown ((KeyCode)System.Enum.Parse (typeof(KeyCode), "Alpha" + hotkey.ToString ()))) {
					triggerAction ();
				}
			} else if (Input.GetKeyDown ((KeyCode)System.Enum.Parse (typeof(KeyCode), hotkey.ToString ().ToUpper ()))) {
				triggerAction ();
			}
		}
	}

	void triggerAction()
	{
		GameObject.FindGameObjectWithTag ("BoardHandler").GetComponent<ActionHandler> ().PerformAction (action);
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