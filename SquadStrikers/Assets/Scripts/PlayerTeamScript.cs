using UnityEngine;
using System.Collections;
//This is where all the player characters are held, as well as Storage, major game decisions, etc.
public class PlayerTeamScript : MonoBehaviour {

	public const int TEAM_SIZE = 4;
	public int depth = 0;
	public GameObject[] defaultTeam = new GameObject[TEAM_SIZE];
	[SerializeField] private PCHandler[] _team = new PCHandler[TEAM_SIZE];
	[SerializeField] private int _score = 0;
	public string saveName = "test";
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
		if (GameObject.FindGameObjectWithTag ("BoardHandler")) {
			if (BoardHandler.GetBoardHandler ().gameState == BoardHandler.GameStates.MovementMode) {
				for (int i = 0; i < TEAM_SIZE; i++) {
					if (Input.GetKeyDown ((KeyCode)System.Enum.Parse (typeof(KeyCode), "Keypad" + (i + 1).ToString ())) ||
					   Input.GetKeyDown ((KeyCode)System.Enum.Parse (typeof(KeyCode), "Alpha" + (i + 1).ToString ()))) {
						_team [i].OnMouseDown ();
					}
				}
			}
		}
	}


	//======================================================IO Here=============================================================//
	[System.Serializable]
	public class PlayerTeamScriptSave {
		public int depth = 0;
		public PCHandler.PCHandlerSave[] _team;
		public int _score = 0;
		public string saveName = "test";

		public PlayerTeamScriptSave (PlayerTeamScript pts) {
			this.depth = pts.depth;
			this._team = new PCHandler.PCHandlerSave[TEAM_SIZE];
			for (int i = 0; i<TEAM_SIZE; i++) {
				this._team[i] = new PCHandler.PCHandlerSave(pts._team[i]);
			}
			this._score = pts._score;
			this.saveName = pts.saveName;
		}

		public GameObject ToGameObject() {
			UnityEngine.Assertions.Assert.IsTrue (GameObject.FindGameObjectWithTag ("PlayerTeam"));
			GameObject oldPlayerTeam = GameObject.FindGameObjectWithTag ("PlayerTeam");
			GameObject output = new GameObject ("PlayerTeam");
			output.AddComponent<Database> ();
			output.GetComponent<Database>().items = oldPlayerTeam.GetComponent<Database>().items;
			output.GetComponent<Database>().enemies = oldPlayerTeam.GetComponent<Database>().enemies;
			output.GetComponent<Database>().tiles = oldPlayerTeam.GetComponent<Database>().tiles;
			output.tag = "PlayerTeam";
			output.AddComponent<PlayerTeamScript>();
			PlayerTeamScript pts = output.GetComponent<PlayerTeamScript>();
			pts.defaultTeam = oldPlayerTeam.GetComponent<PlayerTeamScript>().defaultTeam;

			pts.depth = this.depth;
			pts._score = this._score;
			pts.saveName = this.saveName;
			pts._team = new PCHandler[TEAM_SIZE];
			for (int i = 0; i<TEAM_SIZE; i++) {
				pts._team[i] = this._team[i].ToGameObject().GetComponent<PCHandler>();
				pts._team [i].fatiguedSprite = oldPlayerTeam.GetComponent<PlayerTeamScript> ()._team [i].fatiguedSprite;
				pts._team [i].baseSprite = oldPlayerTeam.GetComponent<PlayerTeamScript> ()._team [i].baseSprite;
				pts._team [i].gameObject.GetComponent<SpriteRenderer> ().sprite = oldPlayerTeam.GetComponent<PlayerTeamScript> ()._team [i].baseSprite;
				oldPlayerTeam.GetComponent<PlayerTeamScript> ()._team [i].gameObject.transform.Find ("ActiveHighlighting").SetParent (pts._team [i].gameObject.transform);
				pts._team [i].gameObject.transform.Find ("ActiveHighlighting").transform.position = new Vector2 (0f, 0f);
				oldPlayerTeam.GetComponent<PlayerTeamScript> ()._team [i].gameObject.transform.Find ("InactiveHighlighting").SetParent (pts._team [i].gameObject.transform);
				pts._team [i].gameObject.transform.Find ("InactiveHighlighting").transform.position = new Vector2 (0f, 0f);
				pts._team [i].gameObject.transform.parent = pts.gameObject.transform;
			}
			oldPlayerTeam.tag = "Untagged";
			for (int i = 0; i < TEAM_SIZE; i++) {
				foreach (Item item in oldPlayerTeam.GetComponent<PlayerTeamScript>().getTeamMember(i).inventory) {
					Destroy(item.gameObject);
				}
			}
			GameObject.Destroy (oldPlayerTeam);
			return output;
		}
	}
}