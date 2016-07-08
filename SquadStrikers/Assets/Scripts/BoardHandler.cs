using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using System.Linq;
using Math = System.Math;
using UnityEngine.SceneManagement;

public class BoardHandler : MonoBehaviour {


	public enum LayoutType {Open,Maze,Rooms,Grid,RoomsMaze,RoomsOpen}
	LayoutType layout; //Basic layout type of the level.
	float[] layoutParameters; //Parameters used by layout type in creating level. Don't worry about these except when randomizing new levels.
	public enum ObjectiveType {Sprint,Survive,Boss,Slaughter,OptionalBoss,KeyCollect,TreasureHunt,Buttons}
	ObjectiveType objective; //Objective to complete the level.
	int[] objectiveParameters; //Paramters used by the objective, such as the number of turns to survive in a survival objective.
	//A by-value pair of ints representing a position on the gameBoard in squares.
	public GameObject defaultPlayerTeam;
	public bool canUndoMovement = false;
	public Coords undoPosition;
	public int undoSubstanceX;
	public bool loadedThisScene; //Is this board from a load game (or going to be replaced with such)?
	public bool suppressInitialization = false;
	public static BoardHandler GetBoardHandler() {
		return GameObject.FindGameObjectWithTag ("BoardHandler").GetComponent<BoardHandler> ();
	}

	[System.Serializable]
	public struct Coords {
		public int x,y;
		public override bool Equals(System.Object obj) 
		{
			return obj is Coords && this == (Coords)obj;
		}
		public static readonly Coords UP = new Coords {x=0,y=1};
		public static readonly Coords DOWN = new Coords {x=0,y=-1};
		public static readonly Coords RIGHT = new Coords {x=1,y=0};
		public static readonly Coords LEFT = new Coords {x=-1,y=0};
		public bool Equals(Coords c) 
		{
			return this == c;
		}
		public Coords(int xCoord,int yCoord) {
			x=xCoord;
			y=yCoord;
		}
		public override int GetHashCode() 
		{
			return x.GetHashCode() ^ y.GetHashCode();
		}
		public static bool operator ==(Coords a, Coords b) 
		{
			return a.x == b.x && a.y == b.y;
		}
		public static bool operator !=(Coords a, Coords b) 
		{
			return !(a == b);
		}
		public static Coords operator +(Coords a, Coords b)
		{
			return new Coords {x = a.x + b.x, y = a.y + b.y};
		}
		public static Coords operator -(Coords a, Coords b)
		{
			return new Coords {x = a.x - b.x, y = a.y - b.y};
		}
		public override string ToString ()
		{
			return "(" + x +"," + y + ")";
		}
	}

	[System.Serializable]
	public enum GameStates {MovementMode, ActionMode, EnemyTurn, TargetMode};
	//Movement Mode is when it is your turn and you are selecting a unit to move.
	//Action Mode happens after you have moved and forces you to select an action.
	//Enemy Turn is while the Enemy AI is processing its actions.
	//Target Mode is after you have selected an action are are specifying your target(s) for it.
	public int ultimateSpawnTurn;
	public int chaserTurn;
	public int doubleChaserTurn;
	public int eliteChaserTurn;
	public GameObject chaser;
	public GameObject eliteChaser;
	public GameObject ultimateSpawn;
	public float itemSpawnDepthTolerance;
	public float enemySpawnDepthTolerance;


	//This contains all of the state transition logic. State tranrsitions that aren't supposed to happen throw
	//exceptions.
	private GameStates _currentGameState;
	public GameStates gameState {
		get { return _currentGameState; }
		set {
			//Debug.Log (_currentGameState.ToString () + value.ToString ());
			if (_currentGameState == value) {
				return;
			} else {
				switch (value) {
				case GameStates.ActionMode:
					if (_currentGameState == GameStates.MovementMode) {
						Assert.IsTrue (is_selected);
						Untarget ();
						_currentGameState = value;
						GameObject.FindGameObjectWithTag ("ActionBar").GetComponent<ActionBar> ().Fill ();
					} else if (_currentGameState == GameStates.TargetMode) {
						Assert.IsTrue (is_selected);
						Untarget ();
						_currentGameState = value;
						GameObject.FindGameObjectWithTag ("ActionBar").GetComponent<ActionBar> ().Fill ();
					} else {
						throw new System.Exception ("Invalid State Transition: " + _currentGameState.ToString () + " to " + value.ToString ());
					}
					break;
				case GameStates.MovementMode:
					if (_currentGameState == GameStates.ActionMode || _currentGameState == GameStates.TargetMode) {
						Unselect ();
						GameObject.FindGameObjectWithTag ("ActionBar").GetComponent<ActionBar> ().Empty ();
						_currentGameState = value;
						GameObject.FindGameObjectWithTag ("StatsBar").GetComponent<StatsBar> ().Clear ();
						GameObject.FindGameObjectWithTag ("IOHandler").GetComponent<IOScript> ().SaveGame ();
					} else if (_currentGameState == GameStates.EnemyTurn) {
						_currentGameState = value;
						GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().NewTurn ();
						GameObject.FindGameObjectWithTag ("IOHandler").GetComponent<IOScript> ().SaveGame ();
						turnNumber += 1;
						SpawnChasers ();
						GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().Log ("Turn has begun. Turn number = " + (turnNumber).ToString ());
						GameObject.FindGameObjectWithTag ("Timer").GetComponent<TimerScript> ().Reposition ();
					} else {
						throw new System.Exception ("Invalid State Transition: " + _currentGameState.ToString () + " to " + value.ToString ());
					}
					break;
				case GameStates.EnemyTurn:
					if (_currentGameState == GameStates.MovementMode) {
						Unselect ();
						GameObject.FindGameObjectWithTag ("ActionBar").GetComponent<ActionBar> ().Empty ();
						_currentGameState = value;
						GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().Log ("Enemy Turn has begun. Turn number = " + turnNumber.ToString ());
						GameObject.FindGameObjectWithTag ("BoardHandler").GetComponentInChildren<EnemyAIHandler> ().TakeTurn (listOfEnemies ());
					} else {
						throw new System.Exception ("Invalid State Transition: " + _currentGameState.ToString () + " to " + value.ToString ());
					}
					break;
				case GameStates.TargetMode:
					if (_currentGameState == GameStates.ActionMode) {
						Untarget ();
						GameObject.FindGameObjectWithTag ("ActionBar").GetComponent<ActionBar> ().SetToCancel ();
						_currentGameState = value;
					} else {
						throw new System.Exception ("Invalid State Transition: " + _currentGameState.ToString () + " to " + value.ToString ());
					}
					break;
				default:
					throw new System.Exception ("Invalid State Transition: " + _currentGameState.ToString () + " to " + value.ToString ());
				}
			}
		}
	}
		
	private int _turnNumber=1;
	public int turnNumber { get {return _turnNumber; } private set {_turnNumber = value; }}
	public int mapHeight;
	public const int defaultMapHeight = 20;
	public int mapWidth;
	public const int defaultMapWidth = 20;
	public GameObject emptyTile;
	public GameObject wallTile;
	public GameObject substanceX;
	public GameObject[] spawnableEnemies;
	public float[] enemyStandardDepth;
	public float[] enemyRarity; //Lower is rarer.
	public GameObject[] spawnableItems;
	public float[] itemStandardDepth;
	public float[] itemRarity; //Lower is rarer.
	public GameObject goalTile; //These are the prefabs used for level generation.
	public float minWallDensity = 0.1f;
	public float maxWallDensity = 0.2f;
    public const float tileSize=0.5f; //Number of pixels per tile.

    public class tileState
    {
        public Tile tile;
        public Unit unit;
        //public List<Item> items; 
		public Item item;
		public SubstanceX substanceX; //score collectibles

		public tileState (Tile iTile, Unit iUnit){
			tile=iTile;
			unit=iUnit;
			item = null;
			substanceX = null;
		}

		public tileState (Tile iTile, Unit iUnit, Item iItem, SubstanceX iSubstanceX){
			tile=iTile;
			unit=iUnit;
			item = iItem;
			substanceX = iSubstanceX;
		}

		public void eraseSubstanceX () {
			Destroy(substanceX.gameObject);
			substanceX = null;
		}

		public void destroyAllButPlayer () {
			Destroy (tile.gameObject);
			if (item != null) {
				Destroy (item.gameObject);
			}
			if (unit != null && unit is Enemy) {
				Destroy (unit.gameObject);
			}
			if (substanceX != null) {
				Destroy (substanceX.gameObject);
			}
		}

		[System.Serializable]
		public class tileStateSave {
			
			public string tile;
			public Enemy.EnemySave enemy;
			public int player; //Index of player, saved separately. -1 means no player.
			//public List<Item> items; 
			public Item.ItemSave item;
			public int substanceX; //score collectibles

			public tileStateSave() {
				tile = "emptyTile";
				enemy = null;
				player = -1;
				item = null;
				substanceX = 0;
			}

			public override string ToString () {
				string output = "{";
				output += tile;
				if (enemy != null) {
					output += ",";
					output += enemy._unitName;
				}
				if (player != -1) {
					output += ", Player";
					output += player.ToString ();
				}
				if (item != null) {
					output += ",";
					output += item.itemName;
				}
				if (substanceX != 0) {
					output += ", Substance X: ";
					output += substanceX.ToString ();
				}
				return output;
			}

			public tileStateSave (tileState ts) {



				if (ts.tile) {
					this.tile = ts.tile.tileName;
				} else {
					this.tile = null;
				}
				if (ts.unit && ts.unit is Enemy) {
					this.enemy = new Enemy.EnemySave((Enemy) ts.unit);
				} else {
					this.enemy = null;
				}
				if (!(ts.unit && ts.unit is PCHandler)) {
					this.player = -1; //No player
				} else {
					this.player = -2; //Error code if never changed.
					PlayerTeamScript pts = GameObject.FindGameObjectWithTag("PlayerTeam").GetComponent<PlayerTeamScript>();
					for (int i = 0; i < PlayerTeamScript.TEAM_SIZE; i++) {
						if (pts.getTeamMember(i) == ((PCHandler) ts.unit)) {
							this.player = i;
						}
					}
					Assert.IsFalse(this.player == -2);
				}
				if (ts.item) {
					this.item = Item.ItemSave.CreateFromItem(ts.item);
				} else {
					this.item = null;
				}
				if (ts.substanceX) {
					this.substanceX = ts.substanceX.amount;
				} else {
					this.substanceX = 0;
				}
			}
			public tileState ToTileState(Coords c) {
				tileState output = new tileState (null,null);
				if (this.tile != null) {
					GameObject newTile;
					if (GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<Database> ().GetTileByName (this.tile, out newTile)) {
						output.tile = ((GameObject)Instantiate (newTile, new Vector2 (c.x * tileSize, c.y * tileSize), Quaternion.identity)).GetComponent<Tile> ();
					} else {
						throw new UnityException("Tile not in database: " + tile);
					}
				} else {
					throw new UnityException("NO TILE FOUND AT (" + c.x + "," + c.y + ")");
					//output.tile = null;
				}
				if (this.enemy != null) {
					output.unit = enemy.ToGameObject (c).GetComponent<Enemy> ();
				} else if (this.player != -1) {
					//Debug.Log ("Player " + player + "loaded at (" + c.x + "," + c.y + ")");
					output.unit = GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().getTeamMember (this.player);
					output.unit.gameObject.transform.position = new Vector3 (c.x * tileSize, c.y * tileSize, -0.1f);
					((PCHandler)output.unit).canMove = ((PCHandler)output.unit).canMove; //I have no idea why this is neccessary, but it fixes a bug where units display as unfatigued after loading.
					//Debug.Log (output.unit.unitName + " Now at (" + output.unit.gameObject.transform.position.x + "," + output.unit.gameObject.transform.position.y + ")");
				} else {
					output.unit = null;
				}
				if (this.item != null) {
					//Debug.Log (this.item.itemName);
					output.item = this.item.ToGameObject ().GetComponent<Item> ();
					output.item.isOnFloor = true;
					output.item.gameObject.transform.position = new Vector3 (c.x * tileSize, c.y * tileSize, -0.05f);
				} else {
					output.item = null;
				}
				if (this.substanceX > 0) {
					output.substanceX = ((GameObject) Instantiate (GetBoardHandler().substanceX, new Vector2 (c.x * tileSize, c.y * tileSize), Quaternion.identity)).GetComponent<SubstanceX>();
					output.substanceX.amount = this.substanceX;
				} else {
					output.substanceX = null;
				}
				return output;
			}
		}
	}
		
	[SerializeField] private tileState[,] gameBoard;
	public Coords selected { get; private set;}
	private bool is_selected = false;

	public tileState getTileState (int x, int y) {
		return gameBoard[x, y];
	}

	public tileState getTileState (Coords c) {
		return gameBoard[c.x, c.y];
	}


	public void Unselect () {
		if (is_selected) {
			//Debug.Log ("Here");
			is_selected = false;
		}
		Untarget ();
	}

	public void Untarget () {
		foreach (tileState tState in gameBoard) {
			tState.tile.targeting = Targeting.NoTargeting;
			if (tState.unit) {
				tState.unit.targeting = Targeting.NoTargeting;
			}
		}
	}


	public void KeepSelectedStill() {
		Assert.IsTrue(is_selected);
		MoveSelectedTo (selected);
	}

	public void MoveSelectedTo(Tile tile) {
		Assert.IsTrue (is_selected);
		Coords c = FindTile (tile);
		MoveSelectedTo (c);
	}
	public void MoveSelectedTo(Coords c) {
		Assert.IsTrue (is_selected);

		canUndoMovement = true;
		if (getTileState (c).substanceX == null) {
			undoSubstanceX = 0;
		} else {
			undoSubstanceX = getTileState (c).substanceX.amount;
			//Debug.Log ("Substance X amount: " + undoSubstanceX);
		}
		undoPosition = selected;

		if (selectedUnit().hasAbility(PCHandler.Ability.CarefulStrike)) {
			if (selected == c) {
				selectedUnit ().carefulStrikeActive = true;
			} else {
				selectedUnit ().carefulStrikeActive = false;
			}
		}
		MoveUnit (selected, c);
		selected = c;
		((PCHandler)getTileState (c).unit).canMove = false;
		gameState = GameStates.ActionMode;
	}


	public void UndoMove() {
		Assert.IsTrue (canUndoMovement);
		Assert.IsTrue(is_selected);
		if (undoSubstanceX > 0) {
			SubstanceX sX = ((GameObject) Instantiate (substanceX, new Vector2 (selected.x * tileSize, selected.y * tileSize), Quaternion.identity)).GetComponent<SubstanceX>();
			sX.amount = undoSubstanceX;
			getTileState (selected).substanceX = sX;
			GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().score -= undoSubstanceX;
		}
		MoveSelectedTo (undoPosition);
		selectedUnit ().canMove = true;
		canUndoMovement = false;
	}

	public Coords FindTile (Tile toFind)
	//Looks for toFind in the board and returns its (x,y) positions through the out vaiables.
	{
		Coords c = new Coords();
		bool found = false;
		for (int x = 0; x < mapWidth; x += 1) {
			for (int y = 0; y < mapHeight; y += 1) {
				if (getTileState (x, y).tile == toFind) {
					c.x = x;
					c.y = y;
					Assert.IsFalse (found); //Fails if there are two copies of the unit
					found = true;
				}
			}
		}
		Assert.IsTrue (found); //Fails if the unit is not on the board.
		return c;
	}

	//Gives a list of all enemies in play
	public List<Enemy> listOfEnemies() {
		List<Enemy> output = new List<Enemy> ();
		for (int x = 0; x < mapWidth; x += 1) {
			for (int y = 0; y < mapHeight; y += 1) {
				if (getTileState (x, y).unit && getTileState (x, y).unit is Enemy) {
					output.Add ((Enemy)getTileState (x, y).unit);
				}
			}
		}
		return output;
	}

	public Coords FindUnit (Unit toFind)
	//Looks for toFind in the board and returns its (x,y) positions through the out vaiables.
	{
		Coords c = new Coords ();
		bool found = false;
		for (int x = 0; x < mapWidth; x += 1) {
			for (int y = 0; y < mapHeight; y += 1) {
				if (getTileState (x, y).unit == toFind) {
					c.x = x;
					c.y = y;
					Assert.IsFalse (found); //Fails if there are two copies of the unit
					found = true;
				}
			}
		}
		if (!found) {
			throw new System.Exception("Unit not found");
		}
		Assert.IsTrue (found); //Fails if the unit is not on the board.
		return c;
	}

	public Coords FindItem (Item toFind)
	//Looks for toFind in the board and returns its (x,y) positions through the out vaiables.
	{
		Coords c = new Coords ();
		bool found = false;
		for (int x = 0; x < mapWidth; x += 1) {
			for (int y = 0; y < mapHeight; y += 1) {
				if (getTileState (x, y).item == toFind) {
					c.x = x;
					c.y = y;
					Assert.IsFalse (found); //Fails if there are two copies of the unit
					found = true;
				}
			}
		}
		Assert.IsTrue (found); //Fails if the unit is not on the board.
		return c;
	}

	//Selects this unit as the active unit to be moved and highlights the squares that the unit can be moved to.
	public void Select (PCHandler unit) {
		if (gameState == GameStates.MovementMode) {
			if (is_selected) {
				Unselect();
			}
			is_selected = true;
			selected = FindUnit (unit);
			//Debug.Log ("Selection at " + selected.ToString());
			GameObject.FindGameObjectWithTag ("StatsBar").GetComponent<StatsBar> ().displayStats (unit);
			highlightSquares ();
		}
	}

	//Highlights all squares that can be reached by the selected unit. Note: This is hideously inefficient,
	//but it shouldn't matter since move values are low. If needed, use Dijkstra. TODO: FIX. IT BROKEN.
	public void highlightSquares () {
		getTileState (selected).unit.targeting = Targeting.MovementTargeting;
		int move = getTileState (selected.x, selected.y).unit.move;
		List<Tuple<Coords,int>> stillToCheck = new List<Tuple<Coords,int>>();
		stillToCheck.Add(new Tuple<Coords, int>(selected,move));
		//Debug.Log("Adding (" + selected.x.ToString() + "," + selected.y.ToString() + ") with move " + move.ToString());
		HashSet<Tile> haveDoneThis = new HashSet<Tile> ();

		while (stillToCheck.Count > 0) {
			int index = 0;
			Coords current = stillToCheck [index].Item1;
			int remainingMove = stillToCheck [index].Item2;
			//Debug.Log ("Current is " + current.ToString ());
			tileState tState = getTileState (current.x, current.y);
			if ((!tState.unit || tState.unit.isFriendly) && tState.tile.isPassable) {
				if (current != selected)
					remainingMove = remainingMove - tState.tile.movementCost;
				if (remainingMove > -1 && !tState.unit) { //Here it would be possible to move to the square
					if (!haveDoneThis.Contains (tState.tile)) {
						tState.tile.targeting = Targeting.MovementTargeting;
						//Debug.Log ("Highlighting at " + current.ToString ());
						haveDoneThis.Add (tState.tile);
					}
				}
				if (remainingMove > 0) { //Here it would be possible to move past the square.
					foreach (Coords c in new Coords[] {current + Coords.UP, current + Coords.DOWN, current + Coords.LEFT, current + Coords.RIGHT}) {
						if (inBounds (c)) {
							stillToCheck.Add (new Tuple<Coords, int> (c, remainingMove));
							//Debug.Log("Adding (" + c.x.ToString() + "," + c.y.ToString() + ") from (" +
							//	current.x.ToString() + "," + current.y.ToString() + ") with remaining move" +
							//	remainingMove.ToString());

						}
					}
				}
			} else {
				//Debug.Log ("No good " + current.ToString ());
			}
			stillToCheck.RemoveAt (index);
		}
	}

	public void swapPlaces (Coords start, Coords finish)
	//Moves the unit at (xStart,yStart) to (xFinish,yFinish) and vice versa.
	{
		Unit target = getTileState (finish).unit;
		getTileState (finish).unit = getTileState (start).unit;
		getTileState (start).unit = target;
		getTileState (finish).unit.updatePosition ();
		target.updatePosition();
		if (getTileState (finish).substanceX && getTileState (finish).unit is PCHandler) {
			GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().score += getTileState (finish).substanceX.amount;
			GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().Log ("Gained " + getTileState (finish).substanceX.amount.ToString() + " Substance X.");
			getTileState (finish).eraseSubstanceX();
		}
		if (getTileState (start).substanceX && getTileState (start).unit is PCHandler) {
			GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().score += getTileState (finish).substanceX.amount;
			GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().Log ("Gained " + getTileState (finish).substanceX.amount.ToString() + " Substance X.");
			getTileState (finish).eraseSubstanceX();
		}
	}

	public void MoveUnit (Coords start, Coords finish)
        //Moves the unit at (xStart,yStart) to (xFinish,yFinish). Returns
        //an error if there is no such unit or if the destination is occupied
        //or is a wall. These things should not be passed in here, it just is
        //a last resort failFast.
	{
		if (start != finish) {
			Assert.IsNull (getTileState (finish).unit);
			Assert.IsTrue (getTileState (finish).tile.isPassable);
			Assert.IsNotNull (getTileState (start).unit);
			getTileState (finish).unit = getTileState (start).unit;
			getTileState (start).unit = null;
			getTileState (finish).unit.updatePosition ();
		}
		if (getTileState (finish).substanceX && getTileState(finish).unit is PCHandler) {
			GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().score += getTileState (finish).substanceX.amount;
			GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().Log ("Gained " + getTileState (finish).substanceX.amount.ToString() + " Substance X.");
			getTileState (finish).eraseSubstanceX();
		}
	}
	public PCHandler selectedUnit() {
		Assert.IsTrue(is_selected);
		Unit u = getTileState (selected).unit;
		Assert.IsTrue(u is PCHandler);
		return (PCHandler) u;
	}

	// Create Blank Map, Randomize it, then add essentials.
	void Init (int height, int width) {
		if (!suppressInitialization) {
			mapHeight = height;
			mapWidth = width;
			gameBoard = new tileState[mapWidth, mapHeight];
			for (int x = 0; x < mapWidth; x += 1) {
				for (int y = 0; y < mapHeight; y += 1) {
					Tile tile = ((GameObject) Instantiate (emptyTile, new Vector2 (x * tileSize, y * tileSize), Quaternion.identity)).GetComponent<Tile>();
					tileState tS = new tileState (tile.GetComponent<Tile> (), null as Unit);
					gameBoard [x, y] = tS;
				}
			}
//			Randomize ();
//			if (!isTraversable () && !loadedThisScene) {
//				GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().depth -= 1;
//				SceneManager.LoadScene ("MainScene");
//				Debug.Log ("Could not traverse level.");
//			}
			GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().depth += 6;
			BoardHandlerSave newLevel = gameObject.GetComponent<BoardGenerator> ().CreateRandomLevel (GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().depth);
			newLevel.ToGameObject ();
		} else {
			//Debug.Log ("Telling to Save");
			GameObject.FindGameObjectWithTag ("IOHandler").GetComponent<IOScript> ().needToSave = 6;
		}
	}

	//Instantiates GameObject as the tile at coordinates (x,y) and destroys the old tile there if there was one.
	void changeTileType(int x, int y, GameObject newTile) {
		tileState tileS = getTileState (x, y);
		if (tileS.tile) {
			Destroy (tileS.tile.gameObject);
		}
		tileS.tile = ((GameObject) Instantiate (newTile, new Vector2 (x * tileSize, y * tileSize), Quaternion.identity)).GetComponent<Tile>();
	}

	//There should be no item at (x,y) (for now, anyway). If so, add unit to that location.
	void addNewItem (int x, int y, Item item)
	{
		Assert.IsNull (getTileState (x, y).item);
		getTileState (x, y).item = item;
		item.updatePosition ();
	}

	//There should be no unit at (x,y). If so, add unit to that location.
	void addNewUnit (int x, int y, Unit unit)
	{
		Assert.IsNull (getTileState (x, y).unit);
		getTileState (x, y).unit = unit;
		unit.updatePosition ();
	}

	void Start () {
		if (GameObject.FindGameObjectsWithTag ("PlayerTeam").Count() == 0) {
			GameObject.Instantiate (defaultPlayerTeam);
		}
		loadedThisScene = (GameObject.FindGameObjectWithTag ("IOHandler").GetComponent<IOScript> ().needToLoad > 0);
		Init (defaultMapHeight, defaultMapWidth);
	}

	//Needed so a button can call this.
	public void EndTurn () {
		if (gameState == GameStates.MovementMode) {
			gameState = GameStates.EnemyTurn;
		} else if (gameState == GameStates.TargetMode) {
			gameObject.GetComponent<ActionHandler> ().PerformAction (new PCHandler.Action("Cancel","",null as Item),null);
			gameObject.GetComponent<ActionHandler> ().PerformAction (new PCHandler.Action("Do Nothing","",null as Item), null);
			gameState = GameStates.EnemyTurn;
		} else if (gameState == GameStates.ActionMode) {
			gameObject.GetComponent<ActionHandler> ().PerformAction (new PCHandler.Action("Do Nothing","",null as Item), null);
			gameState = GameStates.EnemyTurn;
		}
	}

	public bool isImpassibleOrOutOfBounds (Coords c) {
		return (!inBounds (c) || !getTileState (c).tile.isPassable);
	}

	//Returns 0 if not a guard point and returns the guard point's quality otherwise.
	public int guardPointQuality (Coords c) {
		if ((isImpassibleOrOutOfBounds (c + Coords.UP) && isImpassibleOrOutOfBounds (c + Coords.DOWN)) ||
		    (isImpassibleOrOutOfBounds (c + Coords.RIGHT) && isImpassibleOrOutOfBounds (c + Coords.LEFT))) {
			return 3;
		} else if ((isImpassibleOrOutOfBounds (c + Coords.UP) && isImpassibleOrOutOfBounds (c + Coords.DOWN + Coords.LEFT)) ||
		           (isImpassibleOrOutOfBounds (c + Coords.UP) && isImpassibleOrOutOfBounds (c + Coords.DOWN + Coords.RIGHT)) ||
		           (isImpassibleOrOutOfBounds (c + Coords.UP + Coords.LEFT) && isImpassibleOrOutOfBounds (c + Coords.DOWN)) ||
		           (isImpassibleOrOutOfBounds (c + Coords.UP + Coords.RIGHT) && isImpassibleOrOutOfBounds (c + Coords.DOWN)) ||
		           (isImpassibleOrOutOfBounds (c + Coords.RIGHT) && isImpassibleOrOutOfBounds (c + Coords.LEFT + Coords.UP)) ||
		           (isImpassibleOrOutOfBounds (c + Coords.RIGHT) && isImpassibleOrOutOfBounds (c + Coords.LEFT + Coords.DOWN)) ||
		           (isImpassibleOrOutOfBounds (c + Coords.RIGHT + Coords.UP) && isImpassibleOrOutOfBounds (c + Coords.LEFT)) ||
		           (isImpassibleOrOutOfBounds (c + Coords.RIGHT + Coords.DOWN) && isImpassibleOrOutOfBounds (c + Coords.LEFT))) {
			return 2;
		} else if ((isImpassibleOrOutOfBounds (c + Coords.UP + Coords.RIGHT) && isImpassibleOrOutOfBounds (c + Coords.DOWN + Coords.LEFT)) ||
		           (isImpassibleOrOutOfBounds (c + Coords.UP + Coords.LEFT) && isImpassibleOrOutOfBounds (c + Coords.DOWN + Coords.RIGHT))) {
			return 1;
		} else {
			return 0;
		}
	}

	public bool canAttackPlayerWithBow (out PCHandler target, Coords amHere) {
		Tuple<Coords,List<Coords>>[] targetableSquaresAndBlockers = new Tuple<Coords,List<Coords>>[] {
			new Tuple<Coords, List<Coords>>(amHere + Coords.UP + Coords.UP,new List<Coords>{amHere + Coords.UP}),
			new Tuple<Coords, List<Coords>>(amHere + Coords.UP + Coords.UP + Coords.UP,new List<Coords>{amHere + Coords.UP, amHere + Coords.UP + Coords.UP}),

			new Tuple<Coords, List<Coords>>(amHere + Coords.DOWN + Coords.DOWN,new List<Coords>{amHere + Coords.DOWN}),
			new Tuple<Coords, List<Coords>>(amHere + Coords.DOWN + Coords.DOWN + Coords.DOWN,new List<Coords>{amHere + Coords.DOWN, amHere + Coords.DOWN + Coords.DOWN}),

			new Tuple<Coords, List<Coords>>(amHere + Coords.LEFT + Coords.LEFT,new List<Coords>{amHere + Coords.LEFT}),
			new Tuple<Coords, List<Coords>>(amHere + Coords.LEFT + Coords.LEFT + Coords.LEFT,new List<Coords>{amHere + Coords.LEFT, amHere + Coords.LEFT + Coords.LEFT}),

			new Tuple<Coords, List<Coords>>(amHere + Coords.RIGHT + Coords.RIGHT,new List<Coords>{amHere + Coords.RIGHT}),
			new Tuple<Coords, List<Coords>>(amHere + Coords.RIGHT + Coords.RIGHT + Coords.RIGHT,new List<Coords>{amHere + Coords.RIGHT, amHere + Coords.RIGHT + Coords.RIGHT}),

			new Tuple<Coords, List<Coords>>(amHere + Coords.UP + Coords.RIGHT,new List<Coords>{amHere + Coords.UP, amHere + Coords.RIGHT}),
			new Tuple<Coords, List<Coords>>(amHere + Coords.UP + Coords.LEFT,new List<Coords>{amHere + Coords.UP, amHere + Coords.LEFT}),
			new Tuple<Coords, List<Coords>>(amHere + Coords.DOWN + Coords.RIGHT,new List<Coords>{amHere + Coords.DOWN, amHere + Coords.RIGHT}),
			new Tuple<Coords, List<Coords>>(amHere + Coords.DOWN + Coords.LEFT,new List<Coords>{amHere + Coords.DOWN, amHere + Coords.LEFT}),

			new Tuple<Coords, List<Coords>>(amHere + Coords.UP + Coords.UP + Coords.RIGHT,new List<Coords>{amHere + Coords.UP, amHere + Coords.UP + Coords.RIGHT}),
			new Tuple<Coords, List<Coords>>(amHere + Coords.UP + Coords.RIGHT + Coords.RIGHT,new List<Coords>{amHere + Coords.RIGHT, amHere + Coords.UP +Coords.RIGHT}),


			new Tuple<Coords, List<Coords>>(amHere + Coords.UP + Coords.UP + Coords.LEFT,new List<Coords>{amHere + Coords.UP, amHere + Coords.UP + Coords.LEFT}),
			new Tuple<Coords, List<Coords>>(amHere + Coords.UP + Coords.LEFT + Coords.LEFT,new List<Coords>{amHere + Coords.LEFT, amHere + Coords.UP +Coords.LEFT}),


			new Tuple<Coords, List<Coords>>(amHere + Coords.DOWN + Coords.DOWN + Coords.LEFT,new List<Coords>{amHere + Coords.DOWN, amHere + Coords.DOWN + Coords.LEFT}),
			new Tuple<Coords, List<Coords>>(amHere + Coords.DOWN + Coords.LEFT + Coords.LEFT,new List<Coords>{amHere + Coords.LEFT, amHere + Coords.DOWN +Coords.LEFT}),


			new Tuple<Coords, List<Coords>>(amHere + Coords.DOWN + Coords.DOWN + Coords.RIGHT,new List<Coords>{amHere + Coords.DOWN, amHere + Coords.DOWN + Coords.RIGHT}),
			new Tuple<Coords, List<Coords>>(amHere + Coords.DOWN + Coords.RIGHT + Coords.RIGHT,new List<Coords>{amHere + Coords.RIGHT, amHere + Coords.DOWN +Coords.RIGHT}),

		};
		foreach (Tuple<Coords,List<Coords>> t in targetableSquaresAndBlockers) {
			Coords c = t.Item1;
			if (c.x > -1 && c.y > -1 && c.x < mapWidth && c.y < mapHeight) {
				if (getTileState (c).unit) {
					if (getTileState (c).unit is PCHandler) {
						bool canAttack = true;
						foreach (Coords blocker in t.Item2) {
							if (getTileState (blocker).unit || getTileState (blocker).tile.blocksLineOfFire) canAttack = false;
						}
						if (canAttack) {
							target = getTileState (c).unit.GetComponent<PCHandler> ();
							return true;
						}
					}
				}
			}
		}
		target = null as PCHandler;
		return false;
	}

	public bool canAttackPlayerWithCardinalAttack (out PCHandler target, Coords amHere, int range, bool ignoresUnitBlcoking) {
		Coords currentTarget;
		target = null;
		foreach (Coords direction in new Coords[] {Coords.DOWN, Coords.LEFT, Coords.RIGHT, Coords.UP}) {
			currentTarget = amHere;
			for (int i = 1; i <= range; i++) {
				currentTarget = currentTarget + direction;
				if (inBounds (currentTarget) && getTileState (currentTarget).unit && getTileState (currentTarget).unit is PCHandler) {
					target = getTileState (currentTarget).unit.GetComponent<PCHandler> ();
					return true;
				} else if (!(inBounds (currentTarget) && !getTileState (currentTarget).tile.blocksLineOfFire && (!getTileState (currentTarget).unit || ignoresUnitBlcoking))) {
					break;
				}
			}
		}
		return false;
	}


	//Determines whether an enemy unit at amHere with movementSpeed move could reach a square beside a player.
	//If so, gives one such square in moveTo and a corresponding player in target.
	public bool canMoveAndAttackPlayer (out PCHandler target, out Coords moveTo, Coords amHere, int move) {
		List<Tuple<Coords,int>> stillToCheck = new List<Tuple<Coords,int>>();
		stillToCheck.Add(new Tuple<Coords, int>(amHere,move));
		//Debug.Log("Adding (" + selected.x.ToString() + "," + selected.y.ToString() + ") with move " + move.ToString());
		while (stillToCheck.Count > 0) {
			int index = stillToCheck.Count - 1;
			Coords current = stillToCheck [index].Item1;
			int remainingMove = stillToCheck [index].Item2;
			tileState tState = getTileState (current.x, current.y);
			if ((!tState.unit || tState.unit.allowsHostilesThrough()) && tState.tile.isPassable) {
				if (current != amHere) remainingMove = remainingMove - tState.tile.movementCost;
				if (remainingMove > -1 && (!tState.unit || current == amHere)) { //Here it would be possible to move to the square
					foreach (Coords c in new Coords[] {current + Coords.UP, current + Coords.DOWN, current + Coords.LEFT, current + Coords.RIGHT})
					{
						if (inBounds(c) && getTileState (c).unit && getTileState (c).unit is PCHandler && (current == amHere || !tState.unit)) {
							target = (PCHandler) getTileState (c).unit;
							moveTo = current;
							return true;
						}
					}
				}
				if (remainingMove > 0) { //Here it would be possible to move past the square.
					foreach (Coords c in new Coords[] {current + Coords.UP, current + Coords.DOWN, current + Coords.LEFT, current + Coords.RIGHT})
					{
						if (c.x > -1 && c.y > -1 && c.x < mapWidth && c.y < mapHeight) {
							stillToCheck.Add(new Tuple<Coords, int>(c,remainingMove));

						}
					}
				}
			}
			stillToCheck.RemoveAt (index);
		}
		target = null;
		moveTo = amHere;
		return false;
	}

	public bool findBestGuardPointWithin (out Coords moveTo, Coords amHere, int move) {
		List<Tuple<Coords,int>> stillToCheck = new List<Tuple<Coords,int>>();
		stillToCheck.Add(new Tuple<Coords, int>(amHere,move));
		int bestGuardQuality = 0;
		moveTo = amHere;
		bool output = false;
		//Debug.Log("Adding (" + selected.x.ToString() + "," + selected.y.ToString() + ") with move " + move.ToString());
		while (stillToCheck.Count > 0) {
			int index = stillToCheck.Count - 1;
			Coords current = stillToCheck [index].Item1;
			int remainingMove = stillToCheck [index].Item2;
			tileState tState = getTileState (current.x, current.y);
			if ((!tState.unit || tState.unit.allowsHostilesThrough()) && tState.tile.isPassable) {
				if (current != amHere) remainingMove = remainingMove - tState.tile.movementCost;
				if (remainingMove > -1 && (!tState.unit || current == amHere)) { //Here it would be possible to move to the square
					if (guardPointQuality (current) > bestGuardQuality) {
						moveTo = current;
						bestGuardQuality = guardPointQuality (current);
						output = true;
					}
				}
				if (remainingMove > 0) { //Here it would be possible to move past the square.
					foreach (Coords c in new Coords[] {current + Coords.UP, current + Coords.DOWN, current + Coords.LEFT, current + Coords.RIGHT})
					{
						if (c.x > -1 && c.y > -1 && c.x < mapWidth && c.y < mapHeight) {
							stillToCheck.Add(new Tuple<Coords, int>(c,remainingMove));

						}
					}
				}
			}
			stillToCheck.RemoveAt (index);
		}
		return output;
	}

	//Determines whether an sniper at amHere with movementSpeed move could reach a square where they could attack a player.
	//If so, gives a retreat position from one such square in moveTo and a corresponding player in target.
	public bool canMoveAndSnipePlayerWithBow (out PCHandler target, out Coords moveTo, Coords amHere, int move) {
		List<Tuple<Coords,int>> stillToCheck = new List<Tuple<Coords,int>>();
		stillToCheck.Add(new Tuple<Coords, int>(amHere,move));
		//Debug.Log("Adding (" + selected.x.ToString() + "," + selected.y.ToString() + ") with move " + move.ToString());
		while (stillToCheck.Count > 0) {
			int index = stillToCheck.Count - 1;
			Coords current = stillToCheck [index].Item1;
			int remainingMove = stillToCheck [index].Item2;
			tileState tState = getTileState (current.x, current.y);
			if ((!tState.unit || tState.unit.allowsHostilesThrough()) && tState.tile.isPassable) {
				if (current != amHere) remainingMove = remainingMove - tState.tile.movementCost;
				if (remainingMove > -1 && (!tState.unit || current == amHere)) { //Here it would be possible to move to the square
					if (canAttackPlayerWithBow (out target, current)) {
						moveTo = WhereToRunAwayFrom(current,remainingMove,FindUnit(target));
						return true;
					}
				}
				if (remainingMove > 0) { //Here it would be possible to move past the square.
					foreach (Coords c in new Coords[] {current + Coords.UP, current + Coords.DOWN, current + Coords.LEFT, current + Coords.RIGHT})
					{
						if (c.x > -1 && c.y > -1 && c.x < mapWidth && c.y < mapHeight) {
							stillToCheck.Add(new Tuple<Coords, int>(c,remainingMove));

						}
					}
				}
			}
			stillToCheck.RemoveAt (index);
		}
		target = null;
		moveTo = amHere;
		return false;
	}


	//Determines whether an sniper at amHere with movementSpeed move could reach a square where they could attack a player.
	//If so, gives a retreat position from one such square in moveTo and a corresponding player in target.
	public bool canMoveAndAttackPlayerWithCardinalAttack (out PCHandler target, out Coords moveTo, Coords amHere, int move, int range, bool ignoresUnitBlocking) {
		List<Tuple<Coords,int>> stillToCheck = new List<Tuple<Coords,int>>();
		stillToCheck.Add(new Tuple<Coords, int>(amHere,move));
		//Debug.Log("Adding (" + selected.x.ToString() + "," + selected.y.ToString() + ") with move " + move.ToString());
		while (stillToCheck.Count > 0) {
			int index = stillToCheck.Count - 1;
			Coords current = stillToCheck [index].Item1;
			int remainingMove = stillToCheck [index].Item2;
			tileState tState = getTileState (current.x, current.y);
			if ((!tState.unit || tState.unit.allowsHostilesThrough()) && tState.tile.isPassable) {
				if (current != amHere) remainingMove = remainingMove - tState.tile.movementCost;
				if (remainingMove > -1 && (!tState.unit || current == amHere)) { //Here it would be possible to move to the square
					if (canAttackPlayerWithCardinalAttack(out target, current,range,ignoresUnitBlocking)) {
						moveTo = current;
						return true;
					}
				}
				if (remainingMove > 0) { //Here it would be possible to move past the square.
					foreach (Coords c in new Coords[] {current + Coords.UP, current + Coords.DOWN, current + Coords.LEFT, current + Coords.RIGHT})
					{
						if (c.x > -1 && c.y > -1 && c.x < mapWidth && c.y < mapHeight) {
							stillToCheck.Add(new Tuple<Coords, int>(c,remainingMove));

						}
					}
				}
			}
			stillToCheck.RemoveAt (index);
		}
		target = null;
		moveTo = amHere;
		return false;
	}

	public Coords WhereToRunAwayFrom (Coords amHere, int move, Coords target) {
		//Debug.Log ("RUN!" + move.ToString());
		for (int i = 0; i < move; i++) {
			Coords moveVector = amHere - target;
			List<Coords> moves = new List<Coords> ();
			if (Math.Abs (moveVector.x) > Math.Abs (moveVector.x) || (Math.Abs (moveVector.x) > Math.Abs (moveVector.x) && Random.Range (0, 1) == 0)) {
				moves.Add(new Coords (Math.Sign (moveVector.x), 0));
				if (moveVector.y == 0) {
					if (Random.value < 0.5) {
						moves.Add (new Coords (0, 1));
						moves.Add (new Coords (0, -1));
					} else {
						moves.Add (new Coords (0, -1));
						moves.Add (new Coords (0, 1));
					}
				} else {
					moves.Add (new Coords (0, Math.Sign (moveVector.y)));
				}
			} else {			
				moves.Add(new Coords (0, Math.Sign (moveVector.y)));
				if (moveVector.x == 0) {
					if (Random.value < 0.5) {
						moves.Add (new Coords (0, -1));
						moves.Add (new Coords (0, 1));
					} else {
						moves.Add (new Coords (0, 1));
						moves.Add (new Coords (0, -1));
					}
				} else {
					moves.Add (new Coords (0, Math.Sign (moveVector.y)));
				}
			}
			foreach (Coords c in moves) {
				if (inBounds(amHere + c) && getTileState (amHere + c).tile.isPassable && !getTileState (amHere + c).unit) {
					amHere = amHere + c;
					break;
				}
			}
		}
		return amHere;
	}

	public bool inBounds(Coords c) {
		return (c.x > -1 && c.y > -1 && c.x < mapWidth && c.y < mapHeight);
	}

	//needed for the next method
	private class CoordComparator : IComparer<Coords>
	{
		int IComparer<Coords>.Compare(Coords c1, Coords c2)
		{
			if (c1.x.CompareTo (c2.x) == 0) {
				return c1.y.CompareTo (c2.y);
			} else {
				return c1.x.CompareTo (c2.x);
			}
		}
	}

	//Returns the square the a unit at amHere with movement speed move should move to when pursuing
	//the nearest player. Rediculously kludgy implementation due not not being able to find a working
	//data structure in C# that does what I wanted easily (priorityQueue with identical indexes).
	public Coords pursuitSquare (Coords amHere, int move) {
		int index = 0;
		HashSet<Coords> alreadySeen = new HashSet<Coords>{amHere};
		//The first Coords is the distance moved and an index. The second is the location
		//the algorithm should move to if it works out. The third is the point in the search.
		SortedList<Coords,Tuple<Coords,Coords>> frontier;
		frontier = new SortedList<Coords, Tuple<Coords, Coords>> (new CoordComparator ());
		frontier.Add(new Coords (0, index++),new Tuple<Coords, Coords>(amHere, amHere));
		while (frontier.Count>0) {
			Coords key = frontier.Keys.First ();
			int moveSpent = key.x;
			Coords stopPoint = frontier[key].Item1;
			Coords current = frontier[key].Item2;
			foreach (Coords c in new[] {current+Coords.UP, current+Coords.DOWN, current + Coords.LEFT, current + Coords.RIGHT}) {
				if (inBounds (c) && !alreadySeen.Contains (c)) {
					alreadySeen.Add (c);
					//Debug.Log ("Looking at (" + c.x.ToString () + "," + c.y.ToString () + ")");
					int newMoveSpent = moveSpent + getTileState (c).tile.movementCost;
					if (getTileState (c).unit && getTileState (c).unit is PCHandler) {
						return stopPoint;
					} else if (getTileState (c).tile.isPassable && !getTileState (c).unit) {
						if (newMoveSpent <= move) {
							frontier.Add (new Coords (newMoveSpent, index++), new Tuple<Coords, Coords> (c, c));
							alreadySeen.Add (c);
						} else {
							frontier.Add (new Coords (newMoveSpent, index++), new Tuple<Coords, Coords> (stopPoint, c));
							alreadySeen.Add (c);
						}
					} else if (getTileState (c).tile.isPassable && getTileState (c).unit && getTileState (c).unit.allowsHostilesThrough () && !alreadySeen.Contains (c)) {
						frontier.Add (new Coords (newMoveSpent, index++), new Tuple<Coords, Coords> (stopPoint, c));
						alreadySeen.Add (c);
					}
				}
			}
			frontier.Remove (key);
		}
		return amHere;
	}

	//Creates hostile chaser units in the bottom left of the board based on turnCount.
	public void SpawnChasers () {
		if (turnNumber == ultimateSpawnTurn) {
			Spawn(((GameObject) Instantiate (ultimateSpawn, new Vector2 (0f,0f), Quaternion.identity)).GetComponent<Unit>());
		} else if (turnNumber >= eliteChaserTurn) {
			Spawn(((GameObject) Instantiate (eliteChaser, new Vector2 (0f,0f), Quaternion.identity)).GetComponent<Unit>());
			Spawn(((GameObject) Instantiate (eliteChaser, new Vector2 (0f,0f), Quaternion.identity)).GetComponent<Unit>());
		} else if (turnNumber >= doubleChaserTurn) {
			Spawn(((GameObject) Instantiate (chaser, new Vector2 (0f,0f), Quaternion.identity)).GetComponent<Unit>());
			Spawn(((GameObject) Instantiate (chaser, new Vector2 (0f,0f), Quaternion.identity)).GetComponent<Unit>());
		} else if (turnNumber >= chaserTurn) {
			Spawn(((GameObject) Instantiate (chaser, new Vector2 (0f,0f), Quaternion.identity)).GetComponent<Unit>());
		}
	}

	public void Spawn (Unit u) {
		List<Coords> spawnCandidates = new List<Coords> ();
		for (int total = 0; total < mapWidth + mapHeight; total++) {
			for (int x = 0; x <= total; x++) {
				spawnCandidates.Add (new Coords (x, total - x));
			}
		}
		foreach (Coords c in spawnCandidates) {
			tileState tileS = getTileState (c);
			if (!tileS.unit && tileS.tile.isPassable) {
				tileS.unit = u;
				u.gameObject.transform.position = new Vector2 (c.x * tileSize, c.y * tileSize);
				return;
			}
		}
		Assert.IsTrue (false);
	}

	public HashSet<Unit> GetOtherUnitsAround(Coords start, int range) {
		return GetOtherUnitsAround (start, range, false);
	}

	public HashSet<Unit> GetOtherUnitsAround(Coords start, int range, bool ignoresTerrain) {
		return GetOtherUnitsAround (start, range, false, false);
	}

	//Returns all units within range of start except any on start.
	//If square is true, this measures distances including diaganals as 1 instead of 2.
	public HashSet<Unit> GetOtherUnitsAround(Coords start, int range, bool ignoresTerrain, bool square) {
		HashSet<Unit> output = new HashSet<Unit> ();
		List<Tuple<Coords,int>> stillToCheck = new List<Tuple<Coords,int>>();
		stillToCheck.Add(new Tuple<Coords, int>(start,range));
		//Debug.Log("Adding (" + selected.x.ToString() + "," + selected.y.ToString() + ") with move " + move.ToString());
		while (stillToCheck.Count > 0) {
			int index = 0;
			Coords current = stillToCheck [index].Item1;
			int remainingRange = stillToCheck [index].Item2;
			tileState tState = getTileState (current.x, current.y);
			if (!tState.tile.blocksLineOfFire || !ignoresTerrain) {
				if (current != start && tState.unit) output.Add (tState.unit);
				if (remainingRange > 0) { //Here it would be possible to move past the square.
					if (square) {
						foreach (Coords c in new Coords[] {current + Coords.UP, current + Coords.DOWN, current + Coords.LEFT, current + Coords.RIGHT,
							current + Coords.UP + Coords.RIGHT, current + Coords.DOWN + Coords.LEFT, current + Coords.LEFT + Coords.UP, current + Coords.RIGHT + Coords.DOWN}) {
							if (c.x > -1 && c.y > -1 && c.x < mapWidth && c.y < mapHeight) {
								stillToCheck.Add (new Tuple<Coords, int> (c, remainingRange - 1));
							}
						}
					} else {
						foreach (Coords c in new Coords[] {current + Coords.UP, current + Coords.DOWN, current + Coords.LEFT, current + Coords.RIGHT}) {
							if (c.x > -1 && c.y > -1 && c.x < mapWidth && c.y < mapHeight) {
								stillToCheck.Add (new Tuple<Coords, int> (c, remainingRange - 1));

							}
						}
					}
				}
			}
			stillToCheck.RemoveAt (index);
		}
		return output;
	}

	public List<Coords> getOpenTilesAround(Coords c, int range) {
		List<Coords> output = new List<Coords> ();
		for (int x = -range; x < range + 1; x++) {
			for (int y = System.Math.Abs (x) - range; y < range - System.Math.Abs (x); y++) {
				if (inBounds(c + new Coords (x, y)) && !getTileState (c + new Coords (x, y)).unit && getTileState (c + new Coords (x, y)).tile.isPassable) {
					output.Add (c + new Coords (x, y));
				}
			}
		}
		return output;
	}

	public void BrownianMotion (Coords amHere, int move) {
		//Debug.Log ("Brownian" + move.ToString ());
		Coords target = amHere;
		for (int i = 0; i < move; i++) {
			List<Coords> moveCandidates = new List<Coords> ();
			foreach (Coords c in new List<Coords>{Coords.DOWN,Coords.UP,Coords.RIGHT,Coords.LEFT}) {
				if (inBounds (target + c) && getTileState (target + c).tile.isPassable && !getTileState (target + c).unit) {
					moveCandidates.Add (target + c);
				}
			}
			if (moveCandidates.Count == 0) {
				break;
			} else {
				target = moveCandidates[Random.Range(0,moveCandidates.Count-1)];
			}
		}
		MoveUnit (amHere, target);
	}
		
	private bool needToRefreshPlayerTeam = true;
	public void Update () {
		if (needToRefreshPlayerTeam) {
			//Debug.Log ("Update");
			GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().NewLevel ();
			needToRefreshPlayerTeam = false;
		}
	}

	//Highlights all targetable squares according to input parameters.
	//valid points are within range squares of startPoint, in cardinal directions if cardinal = true.
	//They are highlighted in the highlighting given by type and tiles that would be highlightable if an appropriate thing were on them are highlighted as per invalidType.
	//Returns null if no targets are found. Otherwise returns a default target.
	//If allowsDiagonals is on, diagonals count as 1 range. Otherwise, they count as 2. This has no effect in Cardinal Targeting.
	//Note: Minimum range doesn't affect self-targeting. To do that, set targetSelf to false.
	public GameObject Target(int range, bool cardinal, bool includeSelf, bool includeFriendlies, bool includeHostiles, bool includeFloors, bool includeWalls, bool penetratingUnits, bool penetratingWalls, Targeting type, Targeting invalidType, bool allowsDiagonals, int minimumRange = 0) {
		return Target(range, cardinal, includeSelf, includeFriendlies, includeHostiles, includeFloors, includeWalls, penetratingUnits, penetratingWalls, type, invalidType, allowsDiagonals, selected, minimumRange);
	}

	public GameObject Target(int range, bool cardinal, bool includeSelf, bool includeFriendlies, bool includeHostiles, bool includeFloors, bool includeWalls, bool penetratingUnits, bool penetratingWalls, Targeting type, Targeting invalidType, bool allowsDiagonals, Coords startPoint, int minimumRange = 0) {
		Untarget ();
		GameObject defaultTarget;
		if (cardinal) {
			defaultTarget = CardinalTarget (range, includeSelf, includeFriendlies, includeHostiles, includeFloors, includeWalls, penetratingUnits, penetratingWalls, type, invalidType, startPoint, minimumRange);
		} else {
			defaultTarget = NonCardinalTarget (range, includeSelf, includeFriendlies, includeHostiles, includeFloors, includeWalls, penetratingUnits, penetratingWalls, type, invalidType, allowsDiagonals, startPoint, minimumRange);
		}
		if (defaultTarget == null) {
			GameObject.FindGameObjectWithTag ("MessageBox").GetComponentInChildren<MessageBox> ().WarningLog ("No targets found");
		}
		return defaultTarget;
	}

	private GameObject CardinalTarget (int range, bool includeSelf, bool includeFriendlies, bool includeHostiles, bool includeFloors, bool includeWalls, bool penetratingUnits, bool penetratingWalls, Targeting type, Targeting invalidType, Coords startPoint, int minimumRange) {
		//gameState = GameStates.TargetMode;
		GameObject defaultTarget = null;
		Coords currentTarget;
		if (includeSelf) {
			if (getTileState (startPoint).unit) {
				getTileState (startPoint).unit.targeting = type;
				defaultTarget = getTileState (currentTarget).unit.gameObject;

			} else {
				getTileState (startPoint).tile.targeting = type;
				defaultTarget = getTileState (currentTarget).tile.gameObject;
			}
		}
		foreach (Coords direction in new Coords[] {Coords.DOWN, Coords.LEFT, Coords.RIGHT, Coords.UP}) {
			currentTarget = startPoint;
			for (int i = 1; i <= range; i++) {
				currentTarget = currentTarget + direction;
				if (i >= minimumRange) {
					if (inBounds (currentTarget) && getTileState (currentTarget).unit) {
						if ((includeHostiles && !getTileState (currentTarget).unit.isFriendly) || (includeFriendlies && getTileState (currentTarget).unit.isFriendly)) {
							getTileState (currentTarget).unit.GetComponent<Unit> ().targeting = type;
							if (defaultTarget == null) {
								defaultTarget = getTileState (currentTarget).unit.gameObject;
							}
						} else {
							getTileState (currentTarget).unit.GetComponent<Unit> ().targeting = invalidType;
						}
					} else if (inBounds (currentTarget) && ((includeFloors && !getTileState (currentTarget).tile.blocksLineOfFire) || (includeWalls && !getTileState (currentTarget).tile.blocksLineOfFire))) {
						getTileState (currentTarget).tile.GetComponent<Tile> ().targeting = type;
						if (defaultTarget == null) {
							defaultTarget = getTileState (currentTarget).tile.gameObject;
						}
					} else if (inBounds (currentTarget)) {
						if (!getTileState (currentTarget).tile.blocksLineOfFire) {
							getTileState (currentTarget).tile.GetComponent<Tile> ().targeting = invalidType;
						}
					}
					if (!inBounds (currentTarget) || (getTileState (currentTarget).unit && !penetratingUnits) || (getTileState (currentTarget).tile.blocksLineOfFire && !penetratingWalls)) {
						break;
					}
				} else if (!inBounds (currentTarget) || (getTileState (currentTarget).unit && !penetratingUnits) || (getTileState (currentTarget).tile.blocksLineOfFire && !penetratingWalls)) {
					break;
				}
			}
		}
		return defaultTarget;
	}

	private GameObject NonCardinalTarget (int range, bool includeSelf, bool includeFriendlies, bool includeHostiles, bool includeFloors, bool includeWalls, bool penetratingUnits, bool penetratingWalls, Targeting type, Targeting invalidType, bool allowsDiagonals, Coords startPoint, int minimumRange) {
		//gameState = GameStates.TargetMode;
		GameObject defaultTarget = null;
		Coords currentTarget;
		if (includeSelf) {
			if (getTileState (startPoint).unit) {
				getTileState (startPoint).unit.targeting = type;
				defaultTarget = getTileState (startPoint).unit.gameObject;

			} else {
				getTileState (startPoint).tile.targeting = type;
				defaultTarget = getTileState (startPoint).tile.gameObject;
			}
		}
		int crossRange;
		//Debug.Log ("Range = " + range);
		for (int i = -range; i <= range; i++) {
			//Debug.Log ("i = " + i);
			crossRange = allowsDiagonals ? range : range - Math.Abs (i);
			//Debug.Log ("Cross Range = " + crossRange);
			for (int j = -crossRange; j <= crossRange; j++) {
				//Debug.Log ("(" + i + "," + j + ")");
				if (i != 0 || j != 0) {
					if (Math.Abs(i) + Math.Abs(j) >= minimumRange) {
						bool unblocked = true;
						foreach (Coords blocking in interveningSquares(new Coords(i,j))) {
							if (!inBounds (blocking + startPoint) || (getTileState (blocking + startPoint).unit && !penetratingUnits) || (getTileState (blocking + startPoint).tile.blocksLineOfFire && !penetratingWalls)) {
								unblocked = false;
								break;
							}
						}
						if (unblocked) {
							currentTarget = new Coords (i, j) + startPoint;
							if (inBounds (currentTarget) && getTileState (currentTarget).unit) {
								if ((includeHostiles && !getTileState (currentTarget).unit.isFriendly) || (includeFriendlies && getTileState (currentTarget).unit.isFriendly)) {
									getTileState (currentTarget).unit.GetComponent<Unit> ().targeting = type;
									if (defaultTarget == null) {
										defaultTarget = getTileState (currentTarget).unit.gameObject;
									}
								} else {
									getTileState (currentTarget).unit.GetComponent<Unit> ().targeting = invalidType;
								}
							} else if (inBounds (currentTarget) && ((includeFloors && !getTileState (currentTarget).tile.blocksLineOfFire) || (includeWalls && !getTileState (currentTarget).tile.blocksLineOfFire))) {
								getTileState (currentTarget).tile.GetComponent<Tile> ().targeting = type;
								if (defaultTarget == null) {
									defaultTarget = getTileState (currentTarget).tile.gameObject;
								}			
							} else if (inBounds (currentTarget)) {
								if (!getTileState (currentTarget).tile.blocksLineOfFire) {
									getTileState (currentTarget).tile.GetComponent<Tile> ().targeting = invalidType;
								}
							}
						}
					}
				}
			}
		}
		return defaultTarget;
	}

	//When given a relative vector, this function returns a list of all squares that get in the way (in the same relative coordinate system). It the test of whether a line between the centers of the locations would intersect each square at any point.
	public List<Coords> interveningSquares (Coords c) {
		int reversedX = 1; //-1 if reversed. 1 otherwise.
		int reversedY = 1;
		List<Coords> flippedOutput = new List<Coords>();
		List<Coords> output = new List<Coords>();
		if (c.x < 0) {
			reversedX = -1;
			c.x = -c.x;
		}
		if (c.y < 0) {
			reversedY = -1;
			c.y = -c.y;
		}

		//Debug.Log ("INTERVENING:" + c.ToString());
		for (int i = 0; i <= c.x; i++) {
			for (int j = 0; j <= c.y; j++) {
				if (!(i == c.x && j == c.y) && !(i==0 && j == 0)) {
					//Debug.Log ("Checking: (" + i + "," + j + ")");
					//Debug.Log (Mathf.Atan2 (((float)j) + 0.5f, ((float)i) - 0.5f).ToString() + ", " + Mathf.Atan2 ((float)c.y, (float)c.x) + ", " + Mathf.Atan2 (((float)j) - 0.5f, ((float)i) + 0.5f));
					if (Mathf.Atan2 (((float)j) + 0.5f,((float)i) - 0.5f) > Mathf.Atan2 ((float)c.y,(float)c.x) - 0.0001 //Note: Even 10,000*Epsilon didn't worked, so I used this instead.
						&& Mathf.Atan2 (((float)j) - 0.5f,((float)i) + 0.5f) < Mathf.Atan2 ((float)c.y, (float)c.x) + 0.0001) {
						flippedOutput.Add (new Coords (i, j));
					}
				}
			}
		}
		foreach (Coords flippedCoord in flippedOutput) {
			output.Add(new Coords(flippedCoord.x * reversedX, flippedCoord.y * reversedY));
		}
		//foreach (Coords inter in output) {
		//	Debug.Log (inter.ToString ());
		//}
		return output;
	}


	//======================================================IO Here=============================================================//
	[System.Serializable]
	public class BoardHandlerSave {
		private int _turnNumber=1;
		public int mapHeight;
		public int mapWidth;
		public tileState.tileStateSave[,] gameBoard;
		public LayoutType layout;
		public float[] layoutParameters;
		public ObjectiveType objective;
		public int[] objectiveParameters;

		public BoardHandlerSave() {
			this._turnNumber = 0;
			this.mapHeight = defaultMapHeight;
			this.mapWidth = defaultMapWidth;
			this.gameBoard = new tileState.tileStateSave[mapWidth,mapHeight];
			for (int i = 0; i < mapWidth; i++) {
				for (int j = 0; j < mapHeight; j++) {
					this.gameBoard[i,j] = new tileState.tileStateSave();
				}
			}

		}

		public BoardHandlerSave (BoardHandler bh) {
			this._turnNumber = bh._turnNumber;
			this.mapHeight = bh.mapHeight;
			this.mapWidth = bh.mapWidth;
			this.gameBoard = new tileState.tileStateSave[mapWidth,mapHeight];
			this.layout = bh.layout;
			this.layoutParameters = (float[]) (bh.layoutParameters.Clone());
			this.objective = bh.objective;
			this.objectiveParameters = (int[]) (bh.objectiveParameters.Clone());

			for (int i = 0; i < mapWidth; i++) {
				for (int j = 0; j < mapHeight; j++) {
					this.gameBoard[i,j] = new tileState.tileStateSave(bh.gameBoard[i,j]);
				}
			}
		}

		public GameObject ToGameObject() {
			//NOTE: THIS MUST BE LOADED AFTER THE PLAYER TEAM IS LOADED.
			UnityEngine.Assertions.Assert.IsTrue (GameObject.FindGameObjectWithTag ("BoardHandler"));
			GameObject oldBoardHandler = GameObject.FindGameObjectWithTag ("BoardHandler");
			GameObject output = new GameObject ("BoardHandler");
			output.tag = "BoardHandler";
			output.AddComponent<ActionHandler> ();
			output.AddComponent<EnemyAIHandler> ();
			output.GetComponent<EnemyAIHandler> ().delayAfterActing = oldBoardHandler.GetComponent<EnemyAIHandler> ().delayAfterActing;	
			output.AddComponent<MiscKeyHandler> ();
			output.AddComponent<BoardHandler> ();
			BoardHandler bh = output.GetComponent<BoardHandler> ();
			BoardHandler oldBH = oldBoardHandler.GetComponent<BoardHandler> ();
			bh.chaserTurn = oldBH.chaserTurn;
			bh.doubleChaserTurn = oldBH.doubleChaserTurn;
			bh.eliteChaserTurn = oldBH.eliteChaserTurn;
			bh.ultimateSpawnTurn = oldBH.ultimateSpawnTurn;
			bh.chaser = oldBH.chaser;
			bh.eliteChaser = oldBH.eliteChaser;
			bh.ultimateSpawn = oldBH.ultimateSpawn;
			bh.itemSpawnDepthTolerance = oldBH.itemSpawnDepthTolerance;
			bh.enemySpawnDepthTolerance = oldBH.enemySpawnDepthTolerance;
			bh.emptyTile = oldBH.emptyTile;
			bh.wallTile = oldBH.wallTile;
			bh.substanceX = oldBH.substanceX;
			bh.spawnableEnemies = oldBH.spawnableEnemies;
			bh.enemyStandardDepth = oldBH.enemyStandardDepth;
			bh.enemyRarity = oldBH.enemyRarity;
			bh.spawnableItems = oldBH.spawnableItems;
			bh.itemStandardDepth = oldBH.itemStandardDepth;
			bh.itemRarity = oldBH.itemRarity;
			bh.goalTile = oldBH.goalTile;
			bh.minWallDensity = oldBH.minWallDensity;
			bh.maxWallDensity = oldBH.maxWallDensity;



			bh._turnNumber = this._turnNumber;
			bh.mapHeight = this.mapHeight;
			bh.mapWidth = this.mapWidth;
			bh.layout = this.layout;
			bh.layoutParameters = (float[]) (this.layoutParameters.Clone());
			bh.objective = this.objective;
			bh.objectiveParameters = (int[]) (this.objectiveParameters.Clone());
			bh.gameBoard = new tileState[mapWidth, mapHeight];
			bh.suppressInitialization = true;
			bh.needToRefreshPlayerTeam = false;
			bh.loadedThisScene = true;

			for (int i = 0; i < mapWidth; i++) {
				for (int j = 0; j < mapHeight; j++) {
					bh.gameBoard [i, j] = (this.gameBoard [i, j]).ToTileState (new Coords (i, j));
				}
			}
			for (int i = 0; i < mapWidth; i++) {
				for (int j = 0; j < mapHeight; j++) {
					oldBoardHandler.GetComponent<BoardHandler> ().getTileState (new Coords (i, j)).destroyAllButPlayer ();
				}
			}
			oldBoardHandler.tag = "Untagged";
			GameObject.Destroy (oldBoardHandler);
			return output;
		}
	}
}