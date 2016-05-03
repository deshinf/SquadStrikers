using UnityEngine;
using System.Collections;

public class TimerScript : MonoBehaviour {

	BoardHandler boardhandler;
	public int numberOfSteps = 21;
	public float offset = 12.5f;

	// Use this for initialization
	void Start () {
		boardhandler = GameObject.FindGameObjectWithTag ("BoardHandler").GetComponent<BoardHandler>();
	}

	//Called when turn changes. Adjusts its position to the appropriate position.
	public void Reposition() {
		int turn = boardhandler.turnNumber;
		RectTransform rect = gameObject.GetComponent<RectTransform> ();
		float parentWidth = rect.parent.gameObject.GetComponent<RectTransform> ().sizeDelta.x;
		//For some reason seems to take (y,x) not (x,y)
		if (turn <= numberOfSteps) {
			rect.anchoredPosition = new Vector2 (offset + (turn - 1) * parentWidth / (numberOfSteps) - parentWidth / 2, rect.anchoredPosition.y);
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}
