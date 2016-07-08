using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Coords = BoardHandler.Coords;
using BoardHandlerSave = BoardHandler.BoardHandlerSave;
using tileStateSave = BoardHandler.tileState.tileStateSave;
using LayoutType = BoardHandler.LayoutType;
using ObjectiveType = BoardHandler.ObjectiveType;
using UnityEngine.Assertions;
using System.Linq;

public class BoardGenerator : MonoBehaviour {

	int levelDepth;
	int globalAttempts = 0;
	int stepAttempts = 0;
	const int NUMBER_OF_BOSSES = 3;
	int keyProximityThreshold = 6;

	class LevelGenerationException : UnityException {
		public LevelGenerationException() {}
		public LevelGenerationException(string message) : base(message) { }
		public LevelGenerationException(string message, UnityException inner) : base(message, inner) { }
	}

	//Gives a BoardHandlerSave, an encoding of a level state for a brand new, randomly generated level of the appropriate depth.
	public BoardHandlerSave CreateRandomLevel(int depth) {
		try {
			levelDepth = depth;
			float difficultyRemaining = (float) System.Math.Pow(10 + 10 * depth,2);
			BoardHandlerSave board = new BoardHandlerSave();
			try {
				board.layout = chooseLayoutTypeAndParameters (ref difficultyRemaining, ref board.layoutParameters);
				board.objective = chooseObjectiveTypeAndParameters (ref difficultyRemaining, ref board.objectiveParameters, board.layout, board.layoutParameters);
			} catch (LevelGenerationException) {
				Assert.IsTrue (stepAttempts < 100);
				stepAttempts++;
				return CreateRandomLevel (depth);
			}
			Debug.Log("Here" + board.layout.ToString() + ":");
			foreach (float f in board.layoutParameters) {
				Debug.Log(f.ToString());
			}
			Debug.Log("Here" + board.objective.ToString() + ":");
			foreach (int i in board.objectiveParameters) {
				Debug.Log(i.ToString());
			}
			stepAttempts = 0;
			createBasicLayout (ref board, ref difficultyRemaining); //Includes setting starting Positions.
			stepAttempts = 0;
			fillWithItems (ref board, ref difficultyRemaining);
			stepAttempts = 0;
			fillWithEnemies (ref board, ref difficultyRemaining);
			stepAttempts = 0;
			fillWithSubstanceX (ref board, ref difficultyRemaining);
			stepAttempts = 0;
			cleanUp (ref board);
			return board;
		} catch (LevelGenerationException e) {
			if (globalAttempts < 100) {
				globalAttempts++;
				Debug.Log ("Something went wrong: " + e.Message + "attempts:" + globalAttempts);
				stepAttempts = 0;
				return CreateRandomLevel (depth);
			} else {
				throw new LevelGenerationException ("Level creation failed too many times. Last: " + e.Message);
			}
		}
	}


	//============================================= STEP 1: CHOOSING LAYOUT AND OBJECTIVES ================================================================================//

	LayoutType chooseLayoutTypeAndParameters (ref float difficultyRemaining, ref float[] layoutParameters) {
		Dictionary<LayoutType,float> possibleLayouts = new Dictionary<LayoutType, float> ();//The float is the weighting for the probability roll 
		foreach (LayoutType l in System.Enum.GetValues(typeof(LayoutType))) {
			if (difficultyRemaining >= minimumDifficulty (l)) {
				possibleLayouts.Add (l, (difficultyRemaining - minimumDifficulty (l)) * frequency (l));
			}
		}
		LayoutType layout = RandomSelection.Select<LayoutType> (possibleLayouts);
		switch (layout) {
		case LayoutType.Open:
			layoutParameters = new float[] { Random.Range (0.15f, 0.25f) }; //Wall density
			difficultyRemaining -= 0f;//TODO: figure out what these values should be.
			break;
		case LayoutType.Maze:
			layoutParameters = new float[] { Random.Range (0.35f, 0.43f) }; //Wall density
			difficultyRemaining -= 100f*levelDepth;
			break;
		case LayoutType.Grid:
			layoutParameters = new float[] { Random.Range (0f, 0.15f) }; //Turbulence
			difficultyRemaining -= 30f*levelDepth;
			break;
		case LayoutType.Rooms:
			layoutParameters = new float[] { Random.Range(0f,1f),Random.Range (4f, 8f) }; //Regularity and Size
			difficultyRemaining -= (200f - 10*layoutParameters[1])*levelDepth;
			break;
		case LayoutType.RoomsMaze:
			layoutParameters = new float[] { Random.Range(0.4f,0.6f), Random.Range(0f,1f),Random.Range (3f, 8f),Random.Range (0.4f, 0.55f)};
			//Room Density, Regularity and Size and Maze wall density
			difficultyRemaining -= (150f - 5f*layoutParameters[2])*levelDepth;
			break;
		case LayoutType.RoomsOpen:
			layoutParameters = new float[] { Random.Range(0.2f,0.7f), Random.Range(0f,1f),Random.Range (3f, 8f),Random.Range (0.15f, 0.25f)};
			difficultyRemaining -= (50f - 5f*layoutParameters[2])*levelDepth;
			break;
			//Room Density, Regularity and Size and Maze wall density
		}
		return layout;
	}

	float frequency(LayoutType l) {
		switch (l) {
		case LayoutType.Open:
			return 1f;//1f;
		case LayoutType.Maze:
			return 0.75f;//0.5f;
		case LayoutType.Grid:
			return 0.3f;//0.3f;
		case LayoutType.Rooms:
			return 0.75f;//0.25f;
		case LayoutType.RoomsMaze:
			return 0.5f;//1f;//0.5f;
		case LayoutType.RoomsOpen:
			return 0.5f;//0.7f;
		default:
			return 0f;
		}
	}

	float minimumDifficulty(LayoutType l) {
		switch (l) {
		case LayoutType.Open:
			return 0f;
		case LayoutType.Maze:
			return 1000f;
		case LayoutType.Grid:
			return 500f;
		case LayoutType.Rooms:
			return 1000f;
		case LayoutType.RoomsMaze:
			return 1000f;
		case LayoutType.RoomsOpen:
			return 500f;
		default:
			return 0f;
		}
	}

	ObjectiveType chooseObjectiveTypeAndParameters (ref float difficultyRemaining, ref int[] objectiveParameters, LayoutType layout, float[] layoutParameters) {
		Dictionary<ObjectiveType,float> possibleObjectives = new Dictionary<ObjectiveType, float> ();//The int is 
		foreach (ObjectiveType o in System.Enum.GetValues(typeof(ObjectiveType))) {
			if (difficultyRemaining >= minimumDifficulty (o)) {
				possibleObjectives.Add (o, (difficultyRemaining - minimumDifficulty (o)) * frequency (o,layout,layoutParameters));
			}
		}
		ObjectiveType objective;
		try {
			objective = RandomSelection.Select<ObjectiveType> (possibleObjectives);
		} catch (UnityException e) {
			throw new LevelGenerationException ("Failed to select objective." + e.Message);
		}
		switch (objective) {
		case ObjectiveType.Sprint: //Double speed turn counter. Get across level and reach objective.
			objectiveParameters = new int[] {};
			difficultyRemaining -= 0f;//TODO: figure out what these values should be.
			break;
		case ObjectiveType.Survive: //Survive for objectiveParameter[0] turns against enemy forces. Enemies are more aggressive than usual.
			objectiveParameters = new int[] {Random.Range(8,8+2*levelDepth)};//Turns required
			difficultyRemaining -= (objectiveParameters[0]-12)*10f*levelDepth;
			break;
		case ObjectiveType.Slaughter: //Kill a certain number of enemies to unlock the exit. Otherwise as sprint.
			objectiveParameters = new int[] { Random.Range(10+2*levelDepth,20+5*levelDepth)};//Enemy kills required
			difficultyRemaining -= (objectiveParameters[0]-10)*5f*levelDepth;
			break;
		case ObjectiveType.Boss: //A powerful enemy must be defeated before the level exit can be used.
			objectiveParameters = new int[] { GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<Database> ().GetBossID(ref difficultyRemaining,levelDepth) };//Boss ID
			break;
		case ObjectiveType.OptionalBoss: //As sprint, but with a deluxe extra-strength boss who can be defeated for a permanent power-up.
			objectiveParameters = new int[] { GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<Database> ().GetOptionalBossID(ref difficultyRemaining,levelDepth) };//Optional Boss ID
			break;
		case ObjectiveType.KeyCollect: //Keys are scattered around the level. You must collect them before using the level exit.
			objectiveParameters = new int[] { Random.Range(1,1+(int) (System.Math.Sqrt((double) levelDepth)))};//Number of Keys
			difficultyRemaining -= 100f*Mathf.Pow(objectiveParameters[0],1.5f)*levelDepth;
			break;
		case ObjectiveType.Buttons: //Buttons are scattered around the level. They must all be pressed simultaneously. Harder version of Key Collect. Note: You recieve a game over if you have less living characters than buttons.
			objectiveParameters = new int[] { Random.Range(2,1+(int) (System.Math.Sqrt((double) levelDepth/2)))};//Number of Buttons
			difficultyRemaining -= 150f*Mathf.Pow(objectiveParameters[0],1.5f)*levelDepth;
			break;
		case ObjectiveType.TreasureHunt: //Optional clues may be collected which reveal successive clues. Collect them all for a deluxe extra-powerful legendary item. Otherwise as sprint.
			objectiveParameters = new int[] { Random.Range(1,1+(int) (System.Math.Sqrt((double) levelDepth)))};//Number of Clues (not counting the item)
			difficultyRemaining -= 0f;
			break;
		}
		if (difficultyRemaining < Mathf.Pow (10f + 10f * levelDepth, 2) / 2f) {
			throw new LevelGenerationException ("Too Difficult a Layout/Objective Combo");
		}
		return objective;
	}

	//The frequency of a given objective for fixed layout and parameters. Curently constant.
	float frequency(ObjectiveType objective, LayoutType layout, float[] layoutParameters) {
		switch (objective) {
		case ObjectiveType.Sprint:
			return 1f;
		case ObjectiveType.Survive:
			return 0f;//0.5f;
		case ObjectiveType.Slaughter:
			return 0f;//0.5f;
		case ObjectiveType.Boss:
			return 0f; //0.75f;
		case ObjectiveType.OptionalBoss:
			return 0f; //0.75f;
		case ObjectiveType.KeyCollect:
			return 0f; //0.4f;
		case ObjectiveType.Buttons:
			return 0f; //0.4f;
		case ObjectiveType.TreasureHunt:
			return 0f; //0.75f;
		default:
			return 0f;
		}
	}

	float minimumDifficulty(ObjectiveType o) {
		switch (o) {
		case ObjectiveType.Sprint:
			return 0f;
		case ObjectiveType.Survive:
			return 500f;
		case ObjectiveType.Slaughter:
			return 500f;
		case ObjectiveType.Boss:
			return 1000f;
		case ObjectiveType.OptionalBoss:
			return 500f;
		case ObjectiveType.KeyCollect:
			return 1500f;
		case ObjectiveType.Buttons:
			return 1500f;
		case ObjectiveType.TreasureHunt:
			return 500f;
		default:
			return 0f;
		}
	}

	//=================================================================================STEP 2: Create Map (Includes Goals and Start Points) ==============================================================================

	//This is needed for the next method to compute the number of rows that should be used to spawn players.
	private int IntegerSquareRootRdUp (int x) {
		double sqrt = System.Math.Sqrt((double) x);
		if (System.Math.Abs(sqrt - (int) sqrt)<double.Epsilon)
		{
			return (int) sqrt;
		}
		return ((int) sqrt)+1;
	}

	void createBasicLayout (ref BoardHandlerSave board, ref float difficultyRemaining) {
		try {
			// The level is built in this dummy board first, in case something goes wrong.
			tileStateSave[,] boardConfig = new tileStateSave[board.mapHeight,board.mapWidth];
			for (int xcoord = 0; xcoord<board.mapWidth; xcoord++) {
				for (int ycoord = 0; ycoord < board.mapHeight; ycoord++) {
					boardConfig[xcoord,ycoord] = new tileStateSave();
				}
			}
			//The amount of space that should be set aside for the PCs to spawn in.
			int pCSpawnSize = IntegerSquareRootRdUp (PlayerTeamScript.TEAM_SIZE);
			List<Coords> possibleLocations = new List<Coords>();
			for (int xCoord = 0; xCoord < board.mapWidth; xCoord += 1) {
				for (int yCoord = 0; yCoord < board.mapHeight; yCoord += 1) {
					possibleLocations.Add(new Coords {x = xCoord, y = yCoord});
				}
			}
			switch(board.objective) { //Put down starting places
			case ObjectiveType.Sprint:
			case ObjectiveType.Boss:
			case ObjectiveType.OptionalBoss:
			case ObjectiveType.KeyCollect:
			case ObjectiveType.Slaughter:
			case ObjectiveType.Buttons:
			case ObjectiveType.TreasureHunt: //All these start players in the bottom left
				for (int i = 0; i<PlayerTeamScript.TEAM_SIZE; i+=1) {
					boardConfig[i/pCSpawnSize, i % pCSpawnSize].player = i;
					possibleLocations.Remove(new Coords(i/pCSpawnSize, i % pCSpawnSize));
				}
				break;
			case ObjectiveType.Survive: //Players start in the center
				for (int i = 0; i<PlayerTeamScript.TEAM_SIZE; i+=1) {
					boardConfig[i/pCSpawnSize + board.mapWidth/2 - pCSpawnSize/2, i % pCSpawnSize + board.mapHeight/2 - pCSpawnSize/2].player = i;
					possibleLocations.Remove(new Coords(i/pCSpawnSize + board.mapWidth/2 - pCSpawnSize/2, i % pCSpawnSize + board.mapHeight/2 - pCSpawnSize/2));
				}
				break;
			}
			float r,s;
			int x,y;
			List<Coords> possibleKeyLocations;
			Coords coord;
			List<Coords> newPossibleLocations;
			switch(board.objective) { //Put down goals.
			case ObjectiveType.Sprint:
				boardConfig[board.mapWidth - 1, board.mapHeight - 1].tile="goalTile";
				possibleLocations.Remove(new Coords(board.mapWidth - 1, board.mapHeight - 1));
				break;
			case ObjectiveType.Boss:
				boardConfig[board.mapWidth - 1, board.mapHeight - 1].tile="goalTile";
				x = Random.Range(board.mapWidth/2,board.mapWidth-2);
				y = Random.Range(board.mapHeight/2,board.mapHeight-2); //Starting coordinates for the boss
				boardConfig[x,y].enemy = new Enemy.EnemySave(GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<Database> ().GetBossByID(board.objectiveParameters[0]).GetComponent<Enemy>());
				boardConfig[x,y].tile = "keyTile";
				possibleLocations.Remove(new Coords (board.mapWidth - 1, board.mapHeight - 1));
				possibleLocations.Remove(new Coords(x,y));
				break;
			case ObjectiveType.OptionalBoss:
				boardConfig[board.mapWidth - 1, board.mapHeight - 1].tile="goalTile";
				r = Random.Range(0f,0.5f);
				s = Random.Range(0f,0.5f);
				if (r>=s) {
					x=(int) (board.mapWidth * (r+0.5f));
					y=(int) (board.mapHeight * s);
				} else {
					x=(int) (board.mapWidth * r);
					y=(int) (board.mapHeight * (s+0.5f));
				}
				//x = Random.Range(board.mapWidth,board.mapWidth/2);
				//y = Random.Range(board.mapHeight/2,board.mapHeight-2);
				//Starting coordinates for the boss
				boardConfig[x,y].enemy = new Enemy.EnemySave(GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<Database> ().GetOptionalBossByID(board.objectiveParameters[0]).GetComponent<Enemy>());
				boardConfig[x,y].tile = "keyTile";
				possibleLocations.Remove(new Coords (board.mapWidth - 1, board.mapHeight - 1));
				possibleLocations.Remove(new Coords(x,y));
				break;
			case ObjectiveType.KeyCollect:
				boardConfig[board.mapWidth - 1, board.mapHeight - 1].tile="goalTile";
				possibleLocations.Remove(new Coords(board.mapWidth - 1, board.mapHeight - 1));
				possibleKeyLocations = new List<Coords>(possibleLocations);
				newPossibleLocations = new List<Coords> {}; //Needed because you can't change a list while enumerating through it.
//				possibleKeyLocations = possibleKeyLocations.Where(c => c.x+c.y > keyProximityThreshold && c.x+c.y <= height + width - keyProximityThreshold).ToList();
				foreach(Coords c in possibleKeyLocations) {
					if (c.x+c.y > keyProximityThreshold && c.x+c.y < board.mapHeight + board.mapWidth - keyProximityThreshold) {
						newPossibleLocations.Add(c);
					}
				}
				possibleKeyLocations = newPossibleLocations;
				for (int i = 0; i < board.objectiveParameters[0]; i++) {
					if (possibleKeyLocations.Count <= 0) {
						throw new LevelGenerationException("Ran out of places to put keys");
					}
					coord = possibleKeyLocations[Random.Range(0,possibleKeyLocations.Count-1)];
					boardConfig[coord.x,coord.y].item = Item.ItemSave.CreateFromItem(GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<Database> ().GetItemByName("Key").GetComponent<Item>());
					boardConfig[coord.x,coord.y].tile="keyTile";
					possibleLocations.Remove(new Coords(coord.x,coord.y));
					newPossibleLocations = new List<Coords> {};
					foreach(Coords c in possibleKeyLocations) {
						if (System.Math.Abs(c.x-coord.x)+System.Math.Abs(c.y-coord.y) >= keyProximityThreshold) {
							newPossibleLocations.Add(c);
						}
					}
				}
				break;
			case ObjectiveType.Slaughter:
				boardConfig[board.mapWidth - 1, board.mapHeight - 1].tile="goalTile";
				possibleLocations.Remove(new Coords(board.mapWidth - 1, board.mapHeight - 1));
				break;
			case ObjectiveType.Buttons:
				possibleKeyLocations = new List<Coords>(possibleLocations);
				foreach(Coords c in possibleKeyLocations) {
					if (c.x+c.y < keyProximityThreshold+1) {
						possibleKeyLocations.Remove(c);
					}
				}
				for (int i = 0; i < board.objectiveParameters[0]; i++) {
					if (possibleKeyLocations.Count <= 0) {
						throw new LevelGenerationException("Ran out of places to put buttons");
					}
					coord = possibleKeyLocations[Random.Range(0,possibleKeyLocations.Count-1)];
					boardConfig[coord.x,coord.y].tile="buttonTile";
					possibleLocations.Remove(coord);
					foreach(Coords c in possibleKeyLocations) {
						if (System.Math.Abs(c.x-coord.x)+System.Math.Abs(c.y-coord.y) < keyProximityThreshold) {
							possibleKeyLocations.Remove(c);
						}
					}
				}
				break;
			case ObjectiveType.TreasureHunt:
				boardConfig[board.mapWidth - 1, board.mapHeight - 1].tile="goalTile";
				possibleLocations.Remove(new Coords(board.mapWidth - 1, board.mapHeight - 1));
				possibleKeyLocations = new List<Coords>(possibleLocations);
				newPossibleLocations = new List<Coords> {}; //Needed because you can't change a list while enumerating through it.
//				possibleKeyLocations = possibleKeyLocations.Where(c => c.x+c.y > keyProximityThreshold).ToList();
				foreach(Coords c in possibleKeyLocations) {
					if (c.x+c.y >= keyProximityThreshold+1) {
						newPossibleLocations.Add(c);
					}
				}
				possibleKeyLocations = newPossibleLocations;
				for (int i = 0; i < board.objectiveParameters[0]+1; i++) {
					if (possibleKeyLocations.Count <= 0) {
						throw new LevelGenerationException("Ran out of places to put hints");
					}
					coord = possibleKeyLocations[Random.Range(0,possibleKeyLocations.Count-1)];
					if (i == 0) {
						boardConfig[coord.x,coord.y].item=Item.ItemSave.CreateFromItem(GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<Database> ().GetItemByName ("Hint").GetComponent<Item>());
						boardConfig[coord.x,coord.y].tile="keyTile";
					} else {
						boardConfig[coord.x,coord.y].tile="hintTile";//Inactive Hints
					}
					possibleLocations.Remove(coord);
					newPossibleLocations = new List<Coords>{};
					foreach(Coords c in possibleKeyLocations) {
						if (System.Math.Abs(c.x-coord.x)+System.Math.Abs(c.y-coord.y) >= keyProximityThreshold) {
							newPossibleLocations.Add(c);
						}
					}
					possibleKeyLocations = newPossibleLocations;
				}
				break;
			case ObjectiveType.Survive:
				break;
			default:
				throw new LevelGenerationException("Unknown Objective Type: " + System.Enum.GetName(typeof(ObjectiveType),board.objective));
			}
			int numberOfWalls, numberOfRooms, numberOfDoors;
			int xPosition, yPosition;
			Coords doorPosition;
			switch(board.layout) {
			case LayoutType.Open:
			case LayoutType.Maze:
				//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				numberOfWalls = (int) (possibleLocations.Count * board.layoutParameters[0]);
				newPossibleLocations = new List<Coords> (possibleLocations); //Split so can spawn on 
				for (int i = 0; i < numberOfWalls; i+= 1) {
					int index = Random.Range (0, newPossibleLocations.Count);
					Coords position = newPossibleLocations [index];
					newPossibleLocations.RemoveAt (index);
					boardConfig[position.x,position.y].tile="wallTile";
				}
				break;
			case LayoutType.Grid:
				newPossibleLocations = new List<Coords> {}; //Needed because you can't change a list while enumerating through it.
				foreach (Coords c in possibleLocations) {
					if (c.x % 2 == 1 && c.y % 2 == 1 && Random.Range(0f,1f) < (1f - board.layoutParameters[0])
						|| (c.x % 2 == 0 || c.y % 2 == 0) && Random.Range(0f,1f) < board.layoutParameters[0])
					{
						boardConfig[c.x,c.y].tile="wallTile";
					} else {
						newPossibleLocations.Add(c);
					}
				}
				possibleLocations = newPossibleLocations;
				break;
			case LayoutType.Rooms:
				numberOfRooms = (int) (board.mapWidth * board.mapHeight / Mathf.Pow(board.layoutParameters[1],2f));
				for (int i=0; i<numberOfRooms; i++) {
					int RoomWidth = (int) (board.layoutParameters[1] + board.layoutParameters[1] * board.layoutParameters[0] * 0.5f * Mathf.Sqrt(-2f * Mathf.Log(Random.value)) * Mathf.Sin(2f * Mathf.PI * Random.value));
					int RoomHeight = (int) (board.layoutParameters[1] + board.layoutParameters[1] * board.layoutParameters[0] * 0.5f * Mathf.Sqrt(-2f * Mathf.Log(Random.value)) * Mathf.Sin(2f * Mathf.PI * Random.value));
					//Approximate Gaussian with mean room size and standard deviation room size * variance / 2
					if (RoomWidth < 1) RoomWidth = 1;
					if (RoomWidth > (int) (0.7 * board.mapWidth)) RoomWidth = (int) (0.7 * board.mapWidth);
					if (RoomHeight < 1) RoomHeight = 1;
					if (RoomHeight > (int) (0.7 * board.mapHeight)) RoomHeight = (int) (0.7 * board.mapHeight);
					xPosition = Random.Range(0,board.mapWidth - RoomWidth - 1);
					yPosition = Random.Range(0,board.mapHeight - RoomHeight - 1);
					//Debug.Log("Making Room of size " + RoomWidth + "x" + RoomHeight + " at (" + xPosition + "," + yPosition + ")"); 
					//FLOOR
					for (int j = 0; j < RoomWidth;j++) {
						for (int k = 0; k < RoomHeight; k++) {
							if (possibleLocations.Contains(new Coords (j+xPosition,k+yPosition)) && boardConfig[j+xPosition,k+yPosition].tile == "wallTile") {
								boardConfig[j+xPosition,k+yPosition].tile="emptyTile";
								//Debug.Log("Clearing (" + (j+xPosition) + "," + (k+yPosition) + ")");
							}
						}
					}
					//WALLS
					for (int j = xPosition-1; j < xPosition + RoomWidth+1; j++) {
						if (possibleLocations.Contains(new Coords (j, yPosition-1)) && boardConfig[j,yPosition-1].tile == "emptyTile") {
							boardConfig[j,yPosition-1].tile = "wallTile";
							//Debug.Log("Wall a at (" + j + "," + (yPosition-1));
						}
						if (possibleLocations.Contains(new Coords (j, yPosition+RoomHeight)) && boardConfig[j,yPosition+RoomHeight+1].tile == "emptyTile") {
							boardConfig[j,yPosition+RoomHeight].tile = "wallTile";
							//Debug.Log("Wall b at (" + j + "," + (yPosition+RoomWidth));
						}
					}
					for (int j = yPosition-1; j < yPosition + RoomHeight+1; j++) {
						if (possibleLocations.Contains(new Coords (xPosition-1,j)) && boardConfig[xPosition-1,j].tile == "emptyTile") {
							boardConfig[xPosition-1,j].tile = "wallTile";
							//Debug.Log("Wall c at (" + (xPosition-1) + "," + j);
						}
						if (possibleLocations.Contains(new Coords (xPosition+RoomHeight,j)) && boardConfig[xPosition+RoomHeight,j].tile == "emptyTile") {
							boardConfig[xPosition+RoomWidth,j].tile = "wallTile";
							//Debug.Log("Wall d at (" + (xPosition+RoomHeight) + "," + j);
						}
					}
					//DOORS
					numberOfDoors = Random.Range(1,(RoomWidth+RoomHeight)/2);
					doorPosition = new Coords(-1,-1);
					for (int j = 0; j<numberOfDoors; j++) {
						int position = Random.Range(0,RoomHeight*2+RoomWidth*2-1);
						if (position < RoomHeight) {
							doorPosition = new Coords(xPosition-1,yPosition+position);
							//Debug.Log("Door a at (" + doorPosition.ToString());
						} else if (position < RoomHeight + RoomWidth) {
							doorPosition = new Coords(xPosition+position-RoomHeight,yPosition+RoomHeight);
							//Debug.Log("Door b at (" + doorPosition.ToString());
						} else if (position < 2*RoomHeight + RoomWidth) {
							doorPosition = new Coords(xPosition + RoomWidth, yPosition+position - RoomWidth - RoomHeight);
							//Debug.Log("Door c at (" + doorPosition.ToString());
						} else {
							doorPosition = new Coords(xPosition+position-2*RoomHeight - RoomWidth,yPosition + RoomHeight);
							//Debug.Log("Door d at (" + doorPosition.ToString());
						}
						if (possibleLocations.Contains(doorPosition) && boardConfig[doorPosition.x,doorPosition.y].tile == "wallTile") {
							boardConfig[doorPosition.x,doorPosition.y].tile = "doorTile";
						}
					}
					//Remove Walls from spawning
					newPossibleLocations = new List<Coords> {};
					foreach(Coords c in possibleLocations) {
						if (boardConfig[c.x,c.y].tile != "wallTile") {
							newPossibleLocations.Add(c);
						}
					}
					possibleLocations = newPossibleLocations;
					//TODO: Clean up doors.
				}
				break;
			case LayoutType.RoomsOpen:
				//Room Density, Regularity and Size and Maze wall density
				//Open/Maze Part
				numberOfWalls = (int) (possibleLocations.Count * board.layoutParameters[3]);
				newPossibleLocations = new List<Coords> (possibleLocations); // Split so rooms can spawn over walls.
				for (int i = 0; i < numberOfWalls; i+= 1) {
					int index = Random.Range (0, newPossibleLocations.Count);
					Coords position = newPossibleLocations [index];
					newPossibleLocations.RemoveAt (index);
					boardConfig[position.x,position.y].tile="wallTile";
				}
				//Room Part
				numberOfRooms = (int) (board.mapWidth * board.mapHeight * board.layoutParameters[0] / Mathf.Pow(board.layoutParameters[2],2f));
				for (int i=0; i<numberOfRooms; i++) {
					int RoomWidth = (int) (board.layoutParameters[2] + board.layoutParameters[2] * board.layoutParameters[1] * 0.5f * Mathf.Sqrt(-2f * Mathf.Log(Random.value)) * Mathf.Sin(2f * Mathf.PI * Random.value));
					int RoomHeight = (int) (board.layoutParameters[2] + board.layoutParameters[2] * board.layoutParameters[1] * 0.5f * Mathf.Sqrt(-2f * Mathf.Log(Random.value)) * Mathf.Sin(2f * Mathf.PI * Random.value));
					//Approximate Gaussian with mean room size and standard deviation room size * variance / 2
					if (RoomWidth < 1) RoomWidth = 1;
					if (RoomWidth > (int) (0.7 * board.mapWidth)) RoomWidth = (int) (0.7 * board.mapWidth);
					if (RoomHeight < 1) RoomHeight = 1;
					if (RoomHeight > (int) (0.7 * board.mapHeight)) RoomHeight = (int) (0.7 * board.mapHeight);
					xPosition = Random.Range(0,board.mapWidth - RoomWidth - 1);
					yPosition = Random.Range(0,board.mapHeight - RoomHeight - 1);
					Debug.Log("Making Room of size " + RoomWidth + "x" + RoomHeight + " at (" + xPosition + "," + yPosition + ")"); 
					//FLOOR
					for (int j = 0; j < RoomWidth;j++) {
						for (int k = 0; k < RoomHeight; k++) {
							if (possibleLocations.Contains(new Coords (j+xPosition,k+yPosition)) && boardConfig[j+xPosition,k+yPosition].tile == "wallTile") {
								boardConfig[j+xPosition,k+yPosition].tile="emptyTile";
								//Debug.Log("Clearing (" + (j+xPosition) + "," + (k+yPosition) + ")");
							}
						}
					}
					//WALLS
					for (int j = xPosition-1; j < xPosition + RoomWidth+1; j++) {
						if (possibleLocations.Contains(new Coords (j, yPosition-1)) && boardConfig[j,yPosition-1].tile == "emptyTile") {
							boardConfig[j,yPosition-1].tile = "wallTile";
							//Debug.Log("Wall a at (" + j + "," + (yPosition-1));
						}
						if (possibleLocations.Contains(new Coords (j, yPosition+RoomHeight)) && boardConfig[j,yPosition+RoomHeight+1].tile == "emptyTile") {
							boardConfig[j,yPosition+RoomHeight].tile = "wallTile";
							//Debug.Log("Wall b at (" + j + "," + (yPosition+RoomWidth));
						}
					}
					for (int j = yPosition-1; j < yPosition + RoomHeight+1; j++) {
						if (possibleLocations.Contains(new Coords (xPosition-1,j)) && boardConfig[xPosition-1,j].tile == "emptyTile") {
							boardConfig[xPosition-1,j].tile = "wallTile";
							//Debug.Log("Wall c at (" + (xPosition-1) + "," + j);
						}
						if (possibleLocations.Contains(new Coords (xPosition+RoomHeight,j)) && boardConfig[xPosition+RoomHeight,j].tile == "emptyTile") {
							boardConfig[xPosition+RoomWidth,j].tile = "wallTile";
							//Debug.Log("Wall d at (" + (xPosition+RoomHeight) + "," + j);
						}
					}
					//DOORS
					numberOfDoors = Random.Range(1,(RoomWidth+RoomHeight)/2);
					doorPosition = new Coords(-1,-1);
					for (int j = 0; j<numberOfDoors; j++) {
						int position = Random.Range(0,RoomHeight*2+RoomWidth*2-1);
						if (position < RoomHeight) {
							doorPosition = new Coords(xPosition-1,yPosition+position);
							Debug.Log("Door a at (" + doorPosition.ToString());
						} else if (position < RoomHeight + RoomWidth) {
							doorPosition = new Coords(xPosition+position-RoomHeight,yPosition+RoomHeight);
							Debug.Log("Door b at (" + doorPosition.ToString());
						} else if (position < 2*RoomHeight + RoomWidth) {
							doorPosition = new Coords(xPosition + RoomWidth, yPosition+position - RoomWidth - RoomHeight);
							Debug.Log("Door c at (" + doorPosition.ToString());
						} else {
							doorPosition = new Coords(xPosition+position-2*RoomHeight - RoomWidth,yPosition + RoomHeight);
							Debug.Log("Door d at (" + doorPosition.ToString());
						}
						if (possibleLocations.Contains(doorPosition) && boardConfig[doorPosition.x,doorPosition.y].tile == "wallTile") {
							boardConfig[doorPosition.x,doorPosition.y].tile = "doorTile";
						}
					}
					//Remove Walls from spawning
					newPossibleLocations = new List<Coords> {};
					foreach(Coords c in possibleLocations) {
						if (boardConfig[c.x,c.y].tile != "wallTile") {
							newPossibleLocations.Add(c);
						}
					}
					possibleLocations = newPossibleLocations;
					//TODO: Clean up doors.
				}
				break;
			case LayoutType.RoomsMaze:
				//Room Density, Regularity and Size and Maze wall density
				//Open/Maze Part
				numberOfWalls = (int) (possibleLocations.Count * board.layoutParameters[3]);
				for (int i = 0; i < numberOfWalls; i+= 1) {
					int index = Random.Range (0, possibleLocations.Count);
					Coords position = possibleLocations [index];
					boardConfig[position.x,position.y].tile="wallTile";
				}
				//Room Part
				numberOfRooms = (int) (board.mapWidth * board.mapHeight * board.layoutParameters[0] / Mathf.Pow(board.layoutParameters[2],2f));
				for (int i=0; i<numberOfRooms; i++) {
					int RoomWidth = (int) (board.layoutParameters[2] + board.layoutParameters[2] * board.layoutParameters[1] * 0.5f * Mathf.Sqrt(-2f * Mathf.Log(Random.value)) * Mathf.Sin(2f * Mathf.PI * Random.value));
					int RoomHeight = (int) (board.layoutParameters[2] + board.layoutParameters[2] * board.layoutParameters[1] * 0.5f * Mathf.Sqrt(-2f * Mathf.Log(Random.value)) * Mathf.Sin(2f * Mathf.PI * Random.value));
					//Approximate Gaussian with mean room size and standard deviation room size * variance / 2
					if (RoomWidth < 1) RoomWidth = 1;
					if (RoomWidth > (int) (0.7 * board.mapWidth)) RoomWidth = (int) (0.7 * board.mapWidth);
					if (RoomHeight < 1) RoomHeight = 1;
					if (RoomHeight > (int) (0.7 * board.mapHeight)) RoomHeight = (int) (0.7 * board.mapHeight);
					xPosition = Random.Range(0,board.mapWidth - RoomWidth - 1);
					yPosition = Random.Range(0,board.mapHeight - RoomHeight - 1);
					Debug.Log("Making Room of size " + RoomWidth + "x" + RoomHeight + " at (" + xPosition + "," + yPosition + ")"); 
					//FLOOR
					for (int j = 0; j < RoomWidth;j++) {
						for (int k = 0; k < RoomHeight; k++) {
							if (possibleLocations.Contains(new Coords (j+xPosition,k+yPosition)) && boardConfig[j+xPosition,k+yPosition].tile == "wallTile") {
								boardConfig[j+xPosition,k+yPosition].tile="emptyTile";
								//Debug.Log("Clearing (" + (j+xPosition) + "," + (k+yPosition) + ")");
							}
						}
					}
					newPossibleLocations = new List<Coords> {}; //Needed because you can't change a list while enumerating through it.
					//					possibleLocations = possibleLocations.Where(c => boardConfig[c.x,c.y].tile != "wallTile").ToList();
					foreach(Coords c in possibleLocations) {
						if (boardConfig[c.x,c.y].tile != "wallTile") {
							newPossibleLocations.Add(c);
						}
					}
					possibleLocations = newPossibleLocations;
					//TODO: Clean up doors.
			}
			break;
			default:
				throw new LevelGenerationException("Invalid Layout Type: " + board.layout.ToString());
			}
			checkConnectivity(ref boardConfig);
			board.gameBoard = boardConfig;
			//Update Board
		} catch (LevelGenerationException e) {
			if (stepAttempts < 20) {
				stepAttempts++;
				Debug.Log("Basic Layout failed: " + e.Message);
				createBasicLayout (ref board, ref difficultyRemaining);
			} else {
				Debug.Log("Basic Layout failed over 20 times. Most recent: " + e.Message);
				throw new LevelGenerationException ("Basic Layout failed over 100 times. Most recent: " + e.Message);
			}
		}
	}

	//Throws a Level Generation Exception if the level is disconnected. Otherwise, fills in all unreachable tiles and
	//throws an exception if this leaves too few tiles left.
	void checkConnectivity(ref tileStateSave[,] boardGrid) {
		Coords startPoint = new Coords(-1,-1);
		for (int x = 0; x < boardGrid.GetLength (0); x++) {
			for (int y = 0; y < boardGrid.GetLength (1); y++) {
				if (boardGrid [x, y].player == 1) {
					startPoint = new Coords (x, y);
				}
			}
		}
		if (startPoint == new Coords (-1, -1)) {
			throw new LevelGenerationException ("Could not find Player 1 spawn location");
		}
		HashSet<Coords> seen = new HashSet<Coords>{ startPoint };
		List<Coords> toCheck = new List<Coords> { startPoint };
		while (toCheck.Count != 0) {
			int index = toCheck.Count - 1;
			foreach (Coords c in new List<Coords>{Coords.UP,Coords.LEFT,Coords.RIGHT,Coords.DOWN}) {
				if ((toCheck [index] + c).x > -1 &&
					(toCheck [index] + c).y > -1 &&
					(toCheck [index] + c).x < boardGrid.GetLength (0) &&
					(toCheck [index] + c).y < boardGrid.GetLength (1) &&
					boardGrid[(toCheck [index] + c).x,(toCheck [index] + c).y].tile != "wallTile" &&
					!seen.Contains(toCheck[index]+c)) {
					seen.Add (toCheck [index] + c);
					toCheck.Add(toCheck [index] + c);
				}
			}
			toCheck.RemoveAt(index);
		}

		if (seen.Count <= 100) {
			throw new LevelGenerationException ("Not enough level");
		}

		for (int x = 0; x < boardGrid.GetLength (0); x++) {
			for (int y = 0; y < boardGrid.GetLength (1); y++) {
				if (!seen.Contains(new Coords(x,y))) {
					tileStateSave tile = boardGrid [x, y];
					if (tile.enemy != null ||
					    tile.player != -1 ||
					    tile.substanceX != 0 ||
					    tile.item != null ||
						!(tile.tile == "emptyTile" || tile.tile == "floorTile" || tile.tile == "wallTile" || tile.tile == "doorTile")) {
						throw new LevelGenerationException ("Could not reach tile" + tile.ToString ());
					} else {
						tile.tile = "wallTile";
					}
				}
			}
		}
	}


	//============================================================STEP 3: Placing Items on the map=================================================

	void fillWithItems (ref BoardHandlerSave board, ref float difficultyRemaining) {
		try {
			int payoutPoints = (levelDepth + 4) * (levelDepth + 4);
			List<Coords> possibleLocations = new List<Coords> { };
			string debugOutput = "";
			for (int x = 0; x < board.mapWidth; x++) {
				debugOutput += System.Environment.NewLine;
				for (int y = 0; y < board.mapHeight; y++) {
					debugOutput += "{" + board.gameBoard[x,y].tile + board.gameBoard[x,y].player + "}  ";
					//Debug.Log(x.ToString() + "," + y + ";" + board.mapWidth + "," + board.mapHeight);
					if ((board.gameBoard [x, y].tile == "emptyTile" || board.gameBoard [x, y].tile == "doorTile") && board.gameBoard[x,y].player < 0) {
						possibleLocations.Add (new Coords (x, y));
					}
				}
			}
			if (board.objective == ObjectiveType.TreasureHunt) {
				payoutPoints -= levelDepth;
			}
			Debug.Log("possible Locations:" + possibleLocations.Count + ", depth: " + levelDepth + ", payoutPoints: " + payoutPoints + System.Environment.NewLine + debugOutput);
			while (payoutPoints > 0) {
				Item toDrop = GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<Database> ().GetRandomItemAtDepth(levelDepth).GetComponent<Item>();
				if (possibleLocations.Count > 0) {
					Coords c = possibleLocations [Random.Range (0, possibleLocations.Count - 1)];
					board.gameBoard [c.x, c.y].item = Item.ItemSave.CreateFromItem(toDrop);
					payoutPoints -= toDrop.value;
					//Debug.Log("Dropping " + toDrop.itemName + "(" + toDrop.value + ")" + "at" + c.ToString() + "PP: " + payoutPoints + " ; " + board.gameBoard[c.x,c.y].item.itemName);
					if (toDrop.value == 0) {
						throw new LevelGenerationException(toDrop.itemName + "has no value assigned.");
					}
				} else {
					throw new LevelGenerationException ("Not enough open spaces to drop items");
				}
			}
		}  catch (LevelGenerationException e) {
			if (stepAttempts < 1) {
				stepAttempts++;
				for (int x = 0; x < board.mapWidth; x++) {
					for (int y = 0; y < board.mapHeight; y++) {
						if (board.gameBoard [x, y].tile == "emptyTile" || board.gameBoard [x, y].tile == "doorTile") {
							board.gameBoard [x, y].item = null;
						}
					}
				}
				Debug.Log("Item Spawning failed: " + e.Message);
				fillWithItems (ref board, ref difficultyRemaining);
			} else {
				Debug.Log("Item Spawning failed over 100 times. Most recent: " + e.Message);
				throw new LevelGenerationException ("Item Spawning failed over 100 times. Most recent: " + e.Message);
			}
		}
	}

	//============================================================STEP 4: Placing Enemies on the map=================================================

	void fillWithEnemies (ref BoardHandlerSave board, ref float difficultyRemaining) {
		float oldDifficulty = difficultyRemaining;
		try {
			//Debug.Log("Difficulty " + difficultyRemaining);
			List<Coords> possibleLocations = new List<Coords> { };
			for (int x = 0; x < board.mapWidth; x++) {
				for (int y = 0; y < board.mapHeight; y++) {
					if ((board.gameBoard [x, y].tile == "emptyTile" || board.gameBoard [x, y].tile == "doorTile") && board.gameBoard[x,y].player < 0) {
						possibleLocations.Add (new Coords (x, y));
					}
				}
			}
			while (difficultyRemaining > 0f) {
				Enemy toSpawn = GameObject.FindGameObjectWithTag ("PlayerTeam").GetComponent<Database> ().GetRandomEnemyAtDepth (levelDepth).GetComponent<Enemy>();
				if (possibleLocations.Count > 0) {
					Coords c = possibleLocations [Random.Range (0, possibleLocations.Count - 1)];
					board.gameBoard [c.x, c.y].enemy = new Enemy.EnemySave(toSpawn);
					difficultyRemaining -= toSpawn.difficulty;
					//Debug.Log("Spawning " + toSpawn.unitName + "(" + toSpawn.difficulty + ")" + "at" + c.ToString() + "DR: " + difficultyRemaining);
					if (toSpawn.difficulty == 0) {
						throw new LevelGenerationException(toSpawn.unitName + "has no value assigned.");
					}
				} else {
					throw new LevelGenerationException ("Not enough open spaces to spawn enemies");
				}
			}
			int enemies = 0;
			for (int x = 0; x < board.mapWidth; x++) {
				for (int y = 0; y < board.mapHeight; y++) {
					if (board.gameBoard[x,y].enemy != null) {
						enemies ++;
					}
				}
			}
			if (enemies < 10) {
				throw new LevelGenerationException ("Not enough enemies spawned");
			}
		}  catch (LevelGenerationException e) {
			if (stepAttempts < 100) {
				stepAttempts++;
				for (int x = 0; x < board.mapWidth; x++) {
					for (int y = 0; y < board.mapHeight; y++) {
						if (board.gameBoard [x, y].tile == "emptyTile" || board.gameBoard [x, y].tile == "doorTile") {
							Debug.Log ("Wiping Board");
							board.gameBoard [x, y].enemy = null;
						}
					}
				}
				Debug.Log("Enemy Placement failed: " + e.Message);
				fillWithEnemies (ref board, ref oldDifficulty);
			} else {
				Debug.Log("Enemy Placement failed over 100 times. Most recent: " + e.Message);
				throw new LevelGenerationException ("Enemy Spawning failed over 100 times. Most recent: " + e.Message);
			}
		}
	}


	//============================================================STEP 5: Placing Substance X on the map=================================================

	void fillWithSubstanceX (ref BoardHandlerSave board, ref float difficultyRemaining) {
		try {
			List<Coords> possibleLocations = new List<Coords> { };
			for (int x = 0; x < board.mapWidth; x++) {
				for (int y = 0; y < board.mapHeight; y++) {
					if ((board.gameBoard [x, y].tile == "emptyTile" || board.gameBoard [x, y].tile == "doorTile")
						&& board.gameBoard[x,y].player < 0
						&& board.gameBoard[x,y].item == null) {
						possibleLocations.Add (new Coords (x, y));
					}
				}
			}
			foreach (int i in splitUpInteger(substanceXtotal(levelDepth),substanceXclumps(levelDepth))) {
				if (possibleLocations.Count > 0) {
					Coords c = possibleLocations [Random.Range (0, possibleLocations.Count-1)];
					possibleLocations.Remove (c);
					board.gameBoard[c.x,c.y].substanceX = i;
				} else {
					throw new LevelGenerationException("Ran out of places to put Substance X");
				}
			}
		} catch (LevelGenerationException e) {
			if (stepAttempts < 100) {
				stepAttempts++;
				for (int x = 0; x < board.mapWidth; x++) {
					for (int y = 0; y < board.mapHeight; y++) {
						board.gameBoard [x, y].substanceX = 0;
					}
				}
				Debug.Log ("Substance X placement failed: " + e.Message);
				fillWithSubstanceX (ref board, ref difficultyRemaining);
			} else {
				Debug.Log ("Substance X placement failed over 100 times. Most recent: " + e.Message);
				throw new LevelGenerationException ("Substance X placement failed over 100 times. Most recent: " + e.Message);
			}
		}
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

	//=====================================================STEP 6: Clean Up==================================================================

	void cleanUp (ref BoardHandlerSave board) {
		for (int x = 0; x < board.mapWidth; x++) {
			for (int y = 0; y < board.mapHeight; y++) {
				if (board.gameBoard [x, y].tile == "keyTile" || board.gameBoard [x, y].tile == "doorTile") {
					board.gameBoard [x, y].tile = "emptyTile";
				}
			}
		}
		

	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}