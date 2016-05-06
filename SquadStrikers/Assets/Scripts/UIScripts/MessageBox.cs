using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public float defaultHeight = 0.15f;

	private bool _is_expanded = false;
	public bool is_expanded {
		get { return _is_expanded; }
		set {
			if (value != _is_expanded) {
				if (value) {
					gameObject.transform.parent.gameObject.GetComponent<RectTransform> ().anchorMax = new Vector2 (1f, 1f);
					gameObject.transform.parent.gameObject.GetComponent<RectTransform> ().sizeDelta = new Vector2 (0f, 0f);
					gameObject.transform.parent.Find ("Message Expand Button").GetComponentInChildren<Text> ().text = "Less";
				} else {
					gameObject.transform.parent.gameObject.GetComponent<RectTransform> ().anchorMax = new Vector2 (1f, defaultHeight);
					gameObject.transform.parent.gameObject.GetComponent<RectTransform> ().sizeDelta = new Vector2 (0f, 0f);
					gameObject.transform.parent.Find ("Message Expand Button").GetComponentInChildren<Text> ().text = "More";
				}
			}
			_is_expanded = value;
		}
	}

	public void toggleSize() {
		Debug.Log ("szie change");
		is_expanded = !is_expanded;
	}

	public void Log (string output) {
		Text txt = gameObject.GetComponentInChildren<Text>();
		RectTransform rt = gameObject.GetComponentInChildren<Text> ().gameObject.GetComponent<RectTransform> ();
		txt.text += System.Environment.NewLine + output;
		//Adjust the size of the text box to fit the text. sizeDelta is relative to the parent's
		//size, so we need that. That is it's parent's size, so we need that in turn. Ugh.
		rt.anchoredPosition = new Vector2(0f,0f);
		rt.sizeDelta = new Vector2 (0f, txt.preferredHeight);
//		rt.sizeDelta = new Vector2 (0f, txt.preferredHeight - gameObject.transform.parent.parent.gameObject.GetComponent<RectTransform>().rect.height);
	}

	//TODO: Finish these.
	public void WarningLog (string output) {
		Log ("<color=red>" + output + "</color>");
	}

	public void ErrorLog (string output) {
		Log ("<color=red><b>" + output + "</b></color>");
	}

	public void AlertLog (string output) {
		Log ("<color=blue>" + output + "</color>");
	}
}
