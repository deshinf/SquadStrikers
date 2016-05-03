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

	public void Log (string output) {
		Text txt = gameObject.GetComponentInChildren<Text>();
		RectTransform rt = gameObject.GetComponentInChildren<Text> ().gameObject.GetComponent<RectTransform> ();
		txt.text += System.Environment.NewLine + output;
		//Adjust the size of the text box to fit the text. sizeDelta is relative to the parent's
		//size, so we need that. That is it's parent's size, so we need that in turn. Ugh.
		rt.anchoredPosition = new Vector2 (0f, txt.preferredHeight/2);
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
