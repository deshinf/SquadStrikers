using UnityEngine;
using System.Collections;
//This is where all the player characters are held, as well as Storage, major game decisions, etc.
public class PlayerTeamScript : MonoBehaviour {

	public const int TEAM_SIZE = 4;
	public int depth = 0;
	public GameObject[] defaultTeam = new GameObject[TEAM_SIZE];
	[SerializeField] private PCHandler[] _team = new PCHandler[TEAM_SIZE];
	[SerializeField] private int _score = 0;
	public int score {
		get { return _score; }
		set {
			_score = value;
			foreach (GameObject go in GameObject.FindGameObjectsWithTag("ScoreDisplay")) {
				go.GetComponent<ScoreDisplay> ().UpdateDisplay ();
			}
		}
	}

	public void checkWonLevelOrGameOver () {
		bool someoneAscended = false;// 0 is game over, 1 is won level
		for (int i = 0; i < TEAM_SIZE; i++) {
			PCHandler current = getTeamMember (i);
			if (!current.dead && !current.ascended) {
				return;
			} else if (!current.dead) {
				someoneAscended = true;
			}
		}
		if (!someoneAscended) {
		UnityEngine.SceneManagement.SceneManager.LoadScene ("DefeatScreen");
		} else {
			if (depth >= 10) {
				UnityEngine.SceneManagement.SceneManager.LoadScene ("VictoryScreen");
			} else {
				UnityEngine.SceneManagement.SceneManager.LoadScene ("LevelUp");
			}
		}
	}

	//Returns the team member unless none exists, in which case, makes one.
	public PCHandler getTeamMember(int n) {
		if (n > _team.Length-1) {
			throw new System.Exception("Tried to access too high a player number." + n.ToString ());
		}
		if (!_team[n]) {
			//Initializes off the board in order to add to the board.
			_team[n] = ((GameObject) Instantiate (defaultTeam[n], new Vector2 (-2000f,-2000f), Quaternion.identity)).GetComponent<PCHandler>();
			_team [n].transform.parent = transform;
			//_team [n].unitName = "Character " + (n+1).ToString ();
		}
		return _team [n];
	}

	public void NewTurn() {
		for (int i = 0; i < TEAM_SIZE; i++) {
			PCHandler current = getTeamMember (i);
			if (!current.dead && !current.ascended) {
				current.refresh ();
			}
		}
	}

	public void NewLevel() {
		Debug.Log ("New Level");
		for (int i = 0; i < TEAM_SIZE; i++) {
			PCHandler current = getTeamMember (i);
			if (current.dead) {
				current.dead = false;
				current.currentHP = 1;
			}
			current.ascended = false;
			current.targeting = Targeting.NoTargeting;
		}
		NewTurn();
	}

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad (gameObject);
	}

	// Update is called once per frame
	void Update () {
	}
}
