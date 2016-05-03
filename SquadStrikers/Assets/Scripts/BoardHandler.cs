﻿using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using System.Linq;
using Math = System.Math;
using UnityEngine.SceneManagement;

public class BoardHandler : MonoBehaviour {

	//A by-value pair of ints representing a position on the gameBoard in squares.
	public GameObject defaultPlayerTeam;

	public static BoardHandler GetBoardHandler() {
		return GameObject.FindGameObjectWithTag ("Board Handler").GetComponent<BoardHandler> ();
	}

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
	}
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
			Debug.Log (_currentGameState.ToString () + value.ToString ());
			if (_currentGameState == value) {
				return;
			} else {
				switch (value) {
				case GameStates.ActionMode:
					if (_currentGameState == GameStates.MovementMode) {
						Assert.IsTrue (is_selected);
						Untarget ();
						GameObject.FindGameObjectWithTag ("ActionBar").GetComponent<ActionBar> ().Fill ();
						_currentGameState = value;
					} else if (_currentGameState == GameStates.TargetMode) {
						Assert.IsTrue (is_selected);
						Untarget ();
						GameObject.FindGameObjectWithTag ("ActionBar").GetComponent<ActionBar> ().Fill ();
						_currentGameState = value;
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
					} else if (_currentGameState == GameStates.EnemyTurn) {
						_currentGameState = value;
						GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().NewTurn ();
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
			Debug.Log ("Here");
			is_selected = false;
		}
		Untarget ();
	}

	public void Untarget () {
		foreach (tileState tState in gameBoard) {
			tState.tile.isHighlighted = false;
			if (tState.unit) {
				tState.unit.targeting = Targeting.NoTargeting;
			}
		}
	}


	public void KeepSelectedStill() {
		Assert.IsTrue(is_selected);
		((PCHandler)getTileState (selected).unit).canMove = false;
		gameState = GameStates.ActionMode;
	}

	public void MoveSelectedTo(Tile tile) {
		Assert.IsTrue (is_selected);
		Coords c = FindTile (tile);
		MoveSelectedTo (c);
	}
	public void MoveSelectedTo(Coords c) {
		Assert.IsTrue (is_selected);
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
		if (getTileState(c).tile.isGoal) {
			if (GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().depth == 10) {
				UnityEngine.SceneManagement.SceneManager.LoadScene ("VictoryScreen");
			} else {
				((PCHandler)selectedUnit ()).Ascend ();
				Unselect ();
			}
		} else {
			gameState = GameStates.ActionMode;
		}
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

		while (stillToCheck.Count > 0) {
			int index = 0;
			Coords current = stillToCheck [index].Item1;
			int remainingMove = stillToCheck [index].Item2;
			HashSet<Tile> haveDoneThis = new HashSet<Tile> ();
			tileState tState = getTileState (current.x, current.y);
			if ((!tState.unit || tState.unit.isFriendly) && tState.tile.isPassable) {
				if (current != selected) remainingMove = remainingMove - tState.tile.movementCost;
				if (remainingMove > -1 && !tState.unit) { //Here it would be possible to move to the square
					if (!haveDoneThis.Contains(tState.tile)) {
						tState.tile.isHighlighted = true;
						haveDoneThis.Add(tState.tile);
						//Debug.Log("Highlighting (" + current.x.ToString() + "," + current.y.ToString() + ")");
					}
				}
				if (remainingMove > 0) { //Here it would be possible to move past the square.
					foreach (Coords c in new Coords[] {current + Coords.UP, current + Coords.DOWN, current + Coords.LEFT, current + Coords.RIGHT})
					{
						if (inBounds(c)) {
							stillToCheck.Add(new Tuple<Coords, int>(c,remainingMove));
							//Debug.Log("Adding (" + c.x.ToString() + "," + c.y.ToString() + ") from (" +
							//	current.x.ToString() + "," + current.y.ToString() + ") with remaining move" +
							//	remainingMove.ToString());

						}
					}
				}
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
		if (getTileState (finish).substanceX) {
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
		mapHeight = height;
		mapWidth = width;
		gameBoard = new tileState[mapHeight, mapWidth];
		for (int x = 0; x < mapWidth; x += 1) {
			for (int y = 0; y < mapHeight; y += 1) {
				Tile tile = ((GameObject) Instantiate (emptyTile, new Vector2 (x * tileSize, y * tileSize), Quaternion.identity)).GetComponent<Tile>();
				tileState tS = new tileState (tile.GetComponent<Tile> (), null as Unit);
				gameBoard [x, y] = tS;
			}
		}
		Randomize ();
		if (!isTraversable ()) {
			GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().depth -= 1;
			SceneManager.LoadScene ("MainScene");
			Debug.Log ("Could not traverse level.");
		}
	}

	//This is needed for the next method to compute the number of rows that should be used to spawn players.
	private int IntegerSquareRootRdUp (int x) {
		double sqrt = System.Math.Sqrt((double) x);
		if (System.Math.Abs(sqrt - (int) sqrt)<double.Epsilon)
		{
			return (int) sqrt;
		}
		return ((int) sqrt)+1;
	}

	//Builds a random map. Currently makes each square a wall completely independently.
	//TODO: Improve this.
	//TODO: Make sure level is passible.
	void Randomize() {
		int depth = GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().depth++;
		depth += 1;
		//The amount of space that should be set aside for the PCs to spawn in.
		int pCSpawnSize = IntegerSquareRootRdUp (PlayerTeamScript.TEAM_SIZE);
		List<Coords> possibleLocations = new List<Coords>();
		for (int xCoord = 0; xCoord < mapWidth; xCoord += 1) {
			for (int yCoord = 0; yCoord < mapHeight; yCoord += 1) {
				if ((xCoord<mapWidth-2 || yCoord<mapHeight-2)&&(xCoord>pCSpawnSize || yCoord>pCSpawnSize))
					//If the coordinates are not in the restricted area around either the players or the
					//end zone, then they are eligable to have something spawned in them.
					possibleLocations.Add(new Coords {x = xCoord, y = yCoord});
			}
		}
		int numberOfWalls = Random.Range ((int) (possibleLocations.Count * minWallDensity), (int) (possibleLocations.Count * maxWallDensity));
		for (int i = 0; i < numberOfWalls; i+= 1) {
			int index = Random.Range (0, possibleLocations.Count);
			Coords position = possibleLocations [index];
			possibleLocations.RemoveAt (index);
			changeTileType (position.x, position.y, wallTile);
		}
		int numberOfEnemies = Random.Range ((int) (possibleLocations.Count * minEnemyDensity(depth)), (int) (possibleLocations.Count * maxEnemyDensity(depth)));
		for (int i = 0; i < numberOfEnemies; i+= 1) {
			//Debug.Log ("Spawning");
			int index = Random.Range (0, possibleLocations.Count);
			Coords position = possibleLocations [index];
			possibleLocations.RemoveAt (index);
			Unit newEnemy = ((GameObject) Instantiate (enemyToSpawn(depth), new Vector2 (0f,0f), Quaternion.identity)).GetComponent<Unit>();
			addNewUnit (position.x, position.y, newEnemy);
		}
		int numberOfItems = Random.Range ((int) (possibleLocations.Count * minItemDensity(depth)), (int) (possibleLocations.Count * maxItemDensity(depth)));
		for (int i = 0; i < numberOfItems; i+= 1) {
			//Debug.Log ("Spawning");
			int index = Random.Range (0, possibleLocations.Count);
			Coords position = possibleLocations [index];
			possibleLocations.RemoveAt (index);
			Item newItem = ((GameObject) Instantiate (itemToSpawn(depth), new Vector2 (0f,0f), Quaternion.identity)).GetComponent<Item>();
			addNewItem (position.x, position.y, newItem);
		}
		foreach (int i in splitUpInteger(substanceXtotal(depth),substanceXclumps(depth))) {
			int index = Random.Range (0, possibleLocations.Count);
			Coords position = possibleLocations [index];
			possibleLocations.RemoveAt (index);
			SubstanceX sX = ((GameObject) Instantiate (substanceX, new Vector2 (position.x * tileSize, position.y * tileSize), Quaternion.identity)).GetComponent<SubstanceX>();
			sX.amount = i;
			getTileState (position).substanceX = sX;
		}

		//Note: Only spawns one team. If multiplayer ever added, this will need to change.
		PlayerTeamScript playerTeam = GameObject.FindWithTag("PlayerTeam").GetComponent<PlayerTeamScript>();
		for (int i = 0; i<PlayerTeamScript.TEAM_SIZE; i+=1)
		{
			addNewUnit (i/pCSpawnSize, i % pCSpawnSize, playerTeam.getTeamMember(i));
		}
		changeTileType (mapWidth - 1, mapHeight - 1, goalTile);
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
		//Default constructor
		Init (defaultMapHeight, defaultMapWidth);
	}

	//Needed so a button can call this.
	public void EndTurn () {
		if (gameState == GameStates.MovementMode) {
			gameState = GameStates.EnemyTurn;
		}
	}

	//Returns false if no targets found, otherwise true.
	public bool SwordTargeting () {
		gameState = GameStates.TargetMode;
		bool output = false;
		Coords[] targetableSquares;
		if (selectedUnit ().hasAbility (PCHandler.Ability.SwordMastery)) {
			targetableSquares = new Coords[] {
				selected + Coords.UP,
				selected + Coords.DOWN,
				selected + Coords.RIGHT,
				selected + Coords.LEFT,
				selected + Coords.UP + Coords.RIGHT,
				selected + Coords.UP + Coords.LEFT,
				selected + Coords.DOWN + Coords.RIGHT,
				selected + Coords.DOWN + Coords.LEFT,
			};
		} else {
			targetableSquares = new Coords[] {
				selected + Coords.UP,
				selected + Coords.DOWN,
				selected + Coords.RIGHT,
				selected + Coords.LEFT
			};
		}
		foreach (Coords c in targetableSquares) {
			if (c.x > -1 && c.y > -1 && c.x < mapWidth && c.y < mapHeight) {
				if (getTileState (c).unit) {
					if (getTileState (c).unit is Enemy) {
						getTileState (c).unit.GetComponent<Enemy> ().targeting = Targeting.HostileTargeting;
						output = true;
					}
				}
			}
		}
		return output;
	}


	//Returns false if no targets found, otherwise true.
	public bool MaceTargeting () {
		bool output = false;
		gameState = GameStates.TargetMode;
		Coords[] targetableSquares = new Coords[] {
				selected + Coords.UP,
				selected + Coords.DOWN,
				selected + Coords.RIGHT,
				selected + Coords.LEFT
			};
		foreach (Coords c in targetableSquares) {
			if (c.x > -1 && c.y > -1 && c.x < mapWidth && c.y < mapHeight) {
				if (getTileState (c).unit) {
					if (getTileState (c).unit is Enemy) {
						getTileState (c).unit.GetComponent<Enemy> ().targeting = Targeting.HostileTargeting;
						output = true;
					}
				}
			}
		}
		return output;
	}

	//Returns false if no targets found, otherwise true.
	public bool BoardTargeting () {
		gameState = GameStates.TargetMode;
		bool output = false;
		Coords[] targetableSquares = new Coords[] {
			selected + Coords.UP,
			selected + Coords.DOWN,
			selected + Coords.RIGHT,
			selected + Coords.LEFT
		};
		foreach (Coords c in targetableSquares) {
			if (c.x > -1 && c.y > -1 && c.x < mapWidth && c.y < mapHeight) {
				if (getTileState (c).unit) {
					if (getTileState (c).unit is Enemy) {
						getTileState (c).unit.GetComponent<Enemy> ().targeting = Targeting.HostileTargeting;
						output = true;
					}
				}
			}
		}
		return output;
	}

	public bool BowTargeting () {
		gameState = GameStates.TargetMode;
		bool output = false;
		Tuple<Coords,List<Coords>>[] targetableSquaresAndBlockers = new Tuple<Coords,List<Coords>>[] {
			new Tuple<Coords, List<Coords>>(selected + Coords.UP + Coords.UP,new List<Coords>{selected + Coords.UP}),
			new Tuple<Coords, List<Coords>>(selected + Coords.UP + Coords.UP + Coords.UP,new List<Coords>{selected + Coords.UP, selected + Coords.UP + Coords.UP}),

			new Tuple<Coords, List<Coords>>(selected + Coords.DOWN + Coords.DOWN,new List<Coords>{selected + Coords.DOWN}),
			new Tuple<Coords, List<Coords>>(selected + Coords.DOWN + Coords.DOWN + Coords.DOWN,new List<Coords>{selected + Coords.DOWN, selected + Coords.DOWN + Coords.DOWN}),

			new Tuple<Coords, List<Coords>>(selected + Coords.LEFT + Coords.LEFT,new List<Coords>{selected + Coords.LEFT}),
			new Tuple<Coords, List<Coords>>(selected + Coords.LEFT + Coords.LEFT + Coords.LEFT,new List<Coords>{selected + Coords.LEFT, selected + Coords.LEFT + Coords.LEFT}),

			new Tuple<Coords, List<Coords>>(selected + Coords.RIGHT + Coords.RIGHT,new List<Coords>{selected + Coords.RIGHT}),
			new Tuple<Coords, List<Coords>>(selected + Coords.RIGHT + Coords.RIGHT + Coords.RIGHT,new List<Coords>{selected + Coords.RIGHT, selected + Coords.RIGHT + Coords.RIGHT}),

			new Tuple<Coords, List<Coords>>(selected + Coords.UP + Coords.RIGHT,new List<Coords>{selected + Coords.UP, selected + Coords.RIGHT}),
			new Tuple<Coords, List<Coords>>(selected + Coords.UP + Coords.LEFT,new List<Coords>{selected + Coords.UP, selected + Coords.LEFT}),
			new Tuple<Coords, List<Coords>>(selected + Coords.DOWN + Coords.RIGHT,new List<Coords>{selected + Coords.DOWN, selected + Coords.RIGHT}),
			new Tuple<Coords, List<Coords>>(selected + Coords.DOWN + Coords.LEFT,new List<Coords>{selected + Coords.DOWN, selected + Coords.LEFT}),

			new Tuple<Coords, List<Coords>>(selected + Coords.UP + Coords.UP + Coords.RIGHT,new List<Coords>{selected + Coords.UP, selected + Coords.UP + Coords.RIGHT}),
			new Tuple<Coords, List<Coords>>(selected + Coords.UP + Coords.RIGHT + Coords.RIGHT,new List<Coords>{selected + Coords.RIGHT, selected + Coords.UP +Coords.RIGHT}),


			new Tuple<Coords, List<Coords>>(selected + Coords.UP + Coords.UP + Coords.LEFT,new List<Coords>{selected + Coords.UP, selected + Coords.UP + Coords.LEFT}),
			new Tuple<Coords, List<Coords>>(selected + Coords.UP + Coords.LEFT + Coords.LEFT,new List<Coords>{selected + Coords.LEFT, selected + Coords.UP +Coords.LEFT}),


			new Tuple<Coords, List<Coords>>(selected + Coords.DOWN + Coords.DOWN + Coords.LEFT,new List<Coords>{selected + Coords.DOWN, selected + Coords.DOWN + Coords.LEFT}),
			new Tuple<Coords, List<Coords>>(selected + Coords.DOWN + Coords.LEFT + Coords.LEFT,new List<Coords>{selected + Coords.LEFT, selected + Coords.DOWN +Coords.LEFT}),


			new Tuple<Coords, List<Coords>>(selected + Coords.DOWN + Coords.DOWN + Coords.RIGHT,new List<Coords>{selected + Coords.DOWN, selected + Coords.DOWN + Coords.RIGHT}),
			new Tuple<Coords, List<Coords>>(selected + Coords.DOWN + Coords.RIGHT + Coords.RIGHT,new List<Coords>{selected + Coords.RIGHT, selected + Coords.DOWN +Coords.RIGHT}),

			};
		foreach (Tuple<Coords,List<Coords>> t in targetableSquaresAndBlockers) {
			Coords c = t.Item1;
			if (c.x > -1 && c.y > -1 && c.x < mapWidth && c.y < mapHeight) {
				if (getTileState (c).unit) {
					if (getTileState (c).unit is Enemy) {
						bool canAttack = true;
						foreach (Coords blocker in t.Item2) {
							if (getTileState (blocker).unit || getTileState (blocker).tile.blocksLineOfFire) canAttack = false;
						}
						if (canAttack) {
							getTileState (c).unit.GetComponent<Enemy> ().targeting = Targeting.HostileTargeting;
							output = true;
						}
					}
				}
			}
		}
		return output;
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

	//Returns false if no targets found, otherwise true.
	public bool SpearTargeting () {
		gameState = GameStates.TargetMode;
		bool output = false;
		foreach (Coords direction in new Coords[] {Coords.DOWN, Coords.LEFT, Coords.RIGHT, Coords.UP}) {
			Coords c = selected + direction;
			if (inBounds (c) && getTileState(c).tile.isPassable) {
				if (getTileState (c).unit) {
					if (getTileState (c).unit is Enemy) {
						getTileState (c).unit.GetComponent<Enemy> ().targeting = Targeting.HostileTargeting;
						output = true;
					}
				}
				if ((selectedUnit().hasAbility(PCHandler.Ability.SpearMastery) || !getTileState(c).unit) && inBounds (c + direction) && !getTileState (c).tile.blocksLineOfFire) {
					c = selected + direction + direction;
					if (getTileState (c).unit) {
						if (getTileState (c).unit is Enemy) {
							getTileState (c).unit.GetComponent<Enemy> ().targeting = Targeting.HostileTargeting;
							output = true;
						}
					}
				}
			}
		}
		return output;
	}

	//Returns false if no targets found, otherwise true.
	public bool CardinalAttackTargeting (int range, bool ignoresUnitBlcoking) {
		gameState = GameStates.TargetMode;
		bool output = false;
		Coords currentTarget;
		foreach (Coords direction in new Coords[] {Coords.DOWN, Coords.LEFT, Coords.RIGHT, Coords.UP}) {
			currentTarget = selected;
			for (int i = 1; i <= range; i++) {
				currentTarget = currentTarget + direction;
				if (inBounds (currentTarget) && getTileState (currentTarget).unit && getTileState (currentTarget).unit is Enemy) {
					getTileState (currentTarget).unit.GetComponent<Enemy> ().targeting = Targeting.HostileTargeting;
					output = true;
					break;
				} else if (!(inBounds (currentTarget) && !getTileState (currentTarget).tile.blocksLineOfFire && (!getTileState (currentTarget).unit || ignoresUnitBlcoking))) {
					break;
				}
			}
		}
		return output;
	}

	//Returns false if no targets found, otherwise true.
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

	//Returns false if no targets found, otherwise true.
	public bool NonpenetratingCardinalBuffTargeting (int range) {
		gameState = GameStates.TargetMode;
		bool output = false;
		Coords currentTarget;
		foreach (Coords direction in new Coords[] {Coords.DOWN, Coords.LEFT, Coords.RIGHT, Coords.UP}) {
			currentTarget = selected;
			for (int i = 1; i <= range; i++) {
				currentTarget = currentTarget + direction;
				if (inBounds (currentTarget) && getTileState (currentTarget).unit && getTileState (currentTarget).unit.isFriendly) {
					getTileState (currentTarget).unit.GetComponent<Unit> ().targeting = Targeting.FriendlyTargeting;
					output = true;
					break;
				} else if (!(inBounds (currentTarget) && !getTileState (currentTarget).tile.blocksLineOfFire && !getTileState (currentTarget).unit)) {
					break;
				}
			}
		}
		return output;
	}

	//Returns false if no targets found, otherwise true.
	public bool NonpenetratingCardinalMixedTargeting (int range) {
		gameState = GameStates.TargetMode;
		bool output = false;
		Coords currentTarget;
		foreach (Coords direction in new Coords[] {Coords.DOWN, Coords.LEFT, Coords.RIGHT, Coords.UP}) {
			currentTarget = selected;
			for (int i = 1; i <= range; i++) {
				currentTarget = currentTarget + direction;
				if (inBounds (currentTarget) && getTileState (currentTarget).unit) {
					if (getTileState (currentTarget).unit.isFriendly) {
						getTileState (currentTarget).unit.GetComponent<Unit> ().targeting = Targeting.FriendlyTargeting;
					} else {
						getTileState (currentTarget).unit.GetComponent<Unit> ().targeting = Targeting.HostileTargeting;
					}
					output = true;
					break;
				} else if (!(inBounds (currentTarget) && !getTileState (currentTarget).tile.blocksLineOfFire && !getTileState (currentTarget).unit)) {
					break;
				}
			}
		}
		return output;
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
		Debug.Log ("RUN!" + move.ToString());
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
					Debug.Log ("Looking at (" + c.x.ToString () + "," + c.y.ToString () + ")");
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

	public float minEnemyDensity (int depth) {
		Debug.Log ("Depth " + depth.ToString ());
		//return 0f;
		return 1f - Mathf.Pow (0.96f, (float)depth);
	}
	public float maxEnemyDensity (int depth) {
		//return 0f;
		return 1f - Mathf.Pow (0.92f, (float)depth);
	}
	public GameObject enemyToSpawn (int depth) {
		float[] probabilities = new float[spawnableEnemies.Length];
		for (int i = 0; i < spawnableEnemies.Length; i++) {
			if (enemyStandardDepth [i] > depth) {
				probabilities [i] = enemyRarity [i] * Mathf.Exp (-Mathf.Pow (enemyStandardDepth [i] / depth,2f) * enemySpawnDepthTolerance);
			} else {
				probabilities [i] = enemyRarity [i] * Mathf.Exp (-Mathf.Abs (enemyStandardDepth [i] / depth) * enemySpawnDepthTolerance);
			}
		}
		float totalProbabilities = probabilities.Sum ();
		float roll = Random.value;
		for (int i = 0; i < spawnableEnemies.Length; i++) {
			roll -= probabilities [i] / totalProbabilities;
			if (roll < 0) {
				return spawnableEnemies [i];
			}
		}
		throw new System.Exception ("Something wrong wih my enemy spawning algorithm.");
	}


	public float minItemDensity (int depth) {
		return 1f - Mathf.Pow (0.995f, (float)(depth+3));
	}
	public float maxItemDensity (int depth) {
		return 1f - Mathf.Pow (0.99f, (float)(depth+2));
	}
	public GameObject itemToSpawn (int depth) {
		float[] probabilities = new float[spawnableItems.Length];
		for (int i = 0; i < spawnableItems.Length; i++) {
			probabilities [i] = itemRarity [i] * Mathf.Exp (-Mathf.Abs (itemStandardDepth [i] / depth * itemSpawnDepthTolerance));
		}
		float totalProbabilities = probabilities.Sum ();
		float roll = Random.value;
		for (int i = 0; i < spawnableItems.Length; i++) {
			roll -= probabilities [i] / totalProbabilities;
			if (roll < 0) {
				return spawnableItems [i];
			}
		}
		throw new System.Exception ("Something wrong wih my item spawning algorithm.");
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

	//Returns a random partition of total into clumps pieces. Each has at least 1 in it.
	public int[] splitUpInteger (int total, int clumps) {
		Assert.IsTrue(total >= clumps);
		int[] output = new int[clumps];
		for (int i = 0; i< clumps; i++) {
			output [i] = 1;
		}
		for (int i = 0; i<  total - clumps; i++) {
			output [Random.Range(0,clumps-1)] += 1;
		}
		return output;
	}

	//The number of clumps of substance X at this depth.
	public int substanceXclumps (int depth) {
		return 5 + 2 * depth;
	}
	//The total amount of substance X at this depth.
	public int substanceXtotal (int depth) {
		return 10 * depth;
	}

	public void BrownianMotion (Coords amHere, int move) {
		Debug.Log ("Brownian" + move.ToString ());
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
			Debug.Log ("Update");
			GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<PlayerTeamScript> ().NewLevel ();
			needToRefreshPlayerTeam = false;
		}
	}

	public bool isTraversable () {
		HashSet<Coords> seen = new HashSet<Coords>{ new Coords (0, 0) };
		List<Coords> toCheck = new List<Coords> { new Coords (0, 0) };
		while (toCheck.Count != 0) {
			int index = toCheck.Count - 1;
			foreach (Coords c in new List<Coords>{Coords.UP,Coords.LEFT,Coords.RIGHT,Coords.DOWN}) {
				if (inBounds (toCheck [index] + c) && getTileState (toCheck [index] + c).tile.isPassable && !seen.Contains(toCheck[index]+c)) {
					if (getTileState (toCheck [index] + c).tile.isGoal) {
						return true;
					}
					seen.Add (toCheck [index] + c);
					toCheck.Add(toCheck [index] + c);
				}
			}
			toCheck.RemoveAt(index);
		}
		return false;
	}

}