using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnemyAIHandler : MonoBehaviour {

	public float delayAfterActing = 0.2f;

	// Use this for initialization
	void Start () {
	
	}

	public void TakeTurn (List<Enemy> enemies) {
		StartCoroutine (TakeTurnCorountine (enemies));
	}

	private IEnumerator TakeTurnCorountine (List<Enemy> enemies)
	{
		foreach (Enemy enemy in enemies) {
			enemy.Refresh ();
			if (enemy.Act ()) {
				yield return new WaitForSeconds (delayAfterActing);	
			}
		}
		Debug.Log ("Should hand back control");
		gameObject.GetComponent<BoardHandler> ().gameState = BoardHandler.GameStates.MovementMode;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
