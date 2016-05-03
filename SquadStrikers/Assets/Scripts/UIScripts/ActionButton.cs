using UnityEngine;
using UnityEngine.UI; // <-- you need this to access UI (button in this case) functionalities
using UnityEngine.EventSystems;

public class ActionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	Button myButton;
	private PCHandler.Action _action;
	public PCHandler.Action action {
		get {
			return _action;
		}
		set {
			_action = value;
			if (_action.attachedItem) {
				if (_action.actionName == "Pick up Item") {
					gameObject.GetComponentInChildren<Text> ().text = "Pick up " + _action.attachedItem.itemName;
				} else if (_action.actionName == "Drop Item") {
					if (_action.attachedItem == GameObject.FindGameObjectWithTag ("BoardHandler").GetComponent<BoardHandler> ().selectedUnit ().ancientMagic) {
						//Active Ancient Magics can only be discharged, not dropped.
						gameObject.GetComponentInChildren<Text> ().text = "Discharge " + _action.attachedItem.itemName;
					}
					gameObject.GetComponentInChildren<Text> ().text = "Drop " + _action.attachedItem.itemName;
				} else if (_action.actionName == "Discharge Ancient Magic") {
					gameObject.GetComponentInChildren<Text> ().text = "Discharge " + _action.attachedItem.itemName;
				} else if (_action.actionName == "Activate Ancient Magic") {
					gameObject.GetComponentInChildren<Text> ().text = "Activate " + _action.attachedItem.itemName;
				} else {
					gameObject.GetComponentInChildren<Text> ().text = _action.attachedItem.itemName;
				}
			} else {
				gameObject.GetComponentInChildren<Text> ().text = _action.actionName;
			}
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
		if (Input.GetKey (KeyCode.Escape) && action.actionName == "Do Nothing") {
			GameObject.FindGameObjectWithTag ("BoardHandler").GetComponent<ActionHandler> ().PerformAction (action);		
		}
	}

	void triggerAction()
	{
		GameObject.FindGameObjectWithTag ("BoardHandler").GetComponent<ActionHandler> ().PerformAction (action);
	}
}