using UnityEngine;
using System.Collections;

public class Manager : MonoBehaviour {
	// colors of our cell
	// here you can define anothers colors in order to create your own game
	public static Color[] tileColors = new Color[12] {
		new Color(0.733f, 0.894f, 0.855f),
		new Color(0.729f, 0.878f, 0.784f),
		new Color(0.749f, 0.794f, 0.475f),
		new Color(0.761f, 0.684f, 0.388f),
		new Color(0.765f, 0.586f, 0.373f),
		new Color(0.715f, 0.499f, 0.331f),
		new Color(0.729f, 0.412f, 0.027f),
		new Color(0.729f, 0.302f, 0.381f),
		new Color(0.729f, 0.224f, 0.314f),
		new Color(0.729f, 0.150f, 0.247f),
		new Color(0.729f, 0.100f, 0.181f),
		new Color(0.235f, 0.025f, 0.196f)};
	// default value for the high score
	private int highscore = 0;

	public static readonly float[] gridY = new float[4] {0.95f,2.65f,4.35f,6.05f};

	// these values define our board size - you can do a new game using a 5x5 or 6x6 size
	public static Tile[,] grid = new Tile[4,4];

	// our GO handles
	public GameObject scoreText;
	public GameObject highScoreText;
	public GameObject youWonText;
	public GameObject youLoseText;
	public GameObject tileFab;
    // restart button texture
    public Texture2D restartBtn;

	public static bool done;
	// restart values when the game starts or restarts
	bool spawnWaiting;
	int score = 0;
	bool go = true;
	bool winner;
	bool gameOver;

	// clear our grid
	void Start () {

		Cursor.visible = false; // hide cursor
		// we read in our player's preferences file the high score which was saved
		highscore = PlayerPrefs.GetInt("High Score");
		System.Array.Clear (grid, 0, grid.Length);
		done = false;
		// put and set score and best score lines position
		scoreText.transform.position = Camera.main.WorldToViewportPoint (new Vector3 (0.2f, 8f, 0f));
		scoreText.GetComponent<GUIText>().text = "Score : " + score;
		highScoreText.transform.position = Camera.main.WorldToViewportPoint (new Vector3 (0.2f, 7.6f, 0f));
		highScoreText.GetComponent<GUIText>().text = "Best score : " + highscore;

		// a little tip to call the first and second cell when a new game starts
		Spawn ();
		Spawn ();
		/////////////////////////////////////////////////
	}

	// line 71 of this coroutine function isn't included in our game. 
	// it's a little tip to simulate a randomly movement - not an IA (see line 146)
	IEnumerator TryToSolveMyProblem() {
		go = false;
	//	time between each movement - 0.33 second
		yield return new WaitForSeconds(0.33f);
    //  Move(Random.Range (0,3));
		go = true;
	}

	void Update () {
        // simple escape control which works for application and mobile devices
        if(Input.GetKey(KeyCode.Escape))
            Application.Quit();
        // moment until spawn is done
        if (done && spawnWaiting)
			Spawn ();
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
        // keys to play - arrows on keyboard
        if (done & !gameOver && go == true) {
			if(Input.GetKey(KeyCode.UpArrow))
                Move(0);
			if(Input.GetKey(KeyCode.DownArrow))
                Move(1);
			if(Input.GetKey(KeyCode.LeftArrow))
                Move(2);
			if(Input.GetKey(KeyCode.RightArrow))
                Move(3);
		}
#endif
        //////////////////////////////////////////////////////////
        // boolean winner true - if you have reached a cell 2048, you are the winner
        if(winner) {
			// defines position of the line "you win"
			youWonText.transform.position = Camera.main.WorldToViewportPoint (new Vector3 (4.8f, 7.6f, 0f));
			youWonText.GetComponent<GUIText>().text = "Well done!"; // text
			// save the high score
			if(score > highscore) {
				highscore = score;
				PlayerPrefs.SetInt("High Score", highscore);
			} 
			// launch a new game calling the same level
			if (Input.GetKeyDown("space"))
                Application.LoadLevel(0);
		}
		//////////////////////////////////////////////////////////
		// boolean gameOver true
		if(gameOver) {
			// define position of the line "game over"
			youLoseText.transform.position = Camera.main.WorldToViewportPoint (new Vector3 (4.8f, 7.6f, 0f));
			youLoseText.GetComponent<GUIText>().text = "Game Over"; // text
			// save the high score
			if(score > highscore) {
				highscore = score;
				PlayerPrefs.SetInt("High Score", highscore);
			}
			// launch a new game calling the same level
			if (Input.GetKeyDown("space"))
                Application.LoadLevel(0);
		}
	}
    // displays a button so as to restart the game if we have lost or reached the 2048 tile
    void OnGUI() {
#if UNITY_ANDROID || UNITY_IOS // just for mobile devices
        GUIStyle myStyle = new GUIStyle();
        if (gameOver == true || winner == true) { // center of the screen
            if (GUI.Button(new Rect(Screen.width/2 - Screen.width/8, Screen.height/2 - Screen.height/8, Screen.width/4, Screen.height/4), restartBtn, myStyle))
                Application.LoadLevel(0); // restarts the same level
        }
#endif
    }

    public void Move(int dir) {

		if (winner)
            return; // game is out - boolean winner is true (see line 81)
		if (gameOver)
            return; // game is out - boolean game over is true (see line 96)
		if (!done)
            return; // time between each movement is not yet reached

		StartCoroutine(TryToSolveMyProblem());
		// game continues
		done = false;
		Vector2 vector = Vector2.zero;
		bool moved = false;
		// define x and y values in our array
		int[] xArray = {0,1,2,3};
		int[] yArray = {0,1,2,3};
		// reverse our array when change direction by one key
		switch(dir) {
			case 0: vector = Vector2.up;
                System.Array.Reverse(yArray);
                break;
			case 1: vector = -Vector2.up;
                break;
			case 2: vector = -Vector2.right;
                break;
			case 3: vector = Vector2.right;
                System.Array.Reverse(xArray);
                break;
		}

		foreach(int x in xArray) {

			foreach(int y in yArray) {
				if(grid[x,y] != null) {
					grid[x,y].combined = false;
					Vector2 cell;
					Vector2 next = new Vector2(x, y);
					do {
						cell = next;
						next = new Vector2(cell.x + vector.x, cell.y + vector.y);
					} while (isInArea(next) && grid[Mathf.RoundToInt(next.x),Mathf.RoundToInt(next.y)] == null);

					int nx = Mathf.RoundToInt(next.x); int ny = Mathf.RoundToInt(next.y);
					int cx = Mathf.RoundToInt(cell.x); int cy = Mathf.RoundToInt(cell.y);
					// if this cell isn't occuped yet - we move it
					if(isInArea(next) && !grid[nx,ny].combined && grid[nx,ny].tileValue == grid[x,y].tileValue) {
						score += grid[x,y].tileValue * 2; // we increase the value of the new cell
						scoreText.GetComponent<GUIText>().text = "Score : " + score;
						grid[x,y].Move(nx,ny); //combined
						moved = true;
						if((grid[nx,ny].tileValue * 2) == 2048) {
							// 2048 is reached - end of the game - you win
							winner = true;
						}
					} else {
						if(grid[x,y].Move(cx,cy))
							moved = true; //move
					}
				}
			}
		}
		if(moved) { // while one cell can move, game continues
			spawnWaiting = true;
		} else {
			moved = false;
			for(int x = 0; x <= 3; x++) {
				for(int y = 0; y <= 3; y++) {
					if(grid[x,y] == null) {
						moved = true;
					} else {
						for(int z = 0; z <= 3; z++) {
							Vector2 Vtor = Vector2.zero;
							switch(z) { // keys
							case 0:
                                    Vtor = Vector2.up;
                                    break;
							case 1:
                                    Vtor = -Vector2.up;
                                    break;
							case 2:
                                    Vtor = Vector2.right;
                                    break;
							case 3:
                                    Vtor = -Vector2.right;
                                    break;
							}
							if(isInArea(Vtor + new Vector2(x,y)) && 
                                grid[x + Mathf.RoundToInt(Vtor.x), y + Mathf.RoundToInt(Vtor.y)] != null &&
                                grid[x,y].tileValue == grid[x + Mathf.RoundToInt(Vtor.x), y + Mathf.RoundToInt(Vtor.y)].tileValue)
								moved = true;
						}
					}
				}
			}
			// if we cannot move one cell, game is over
			if(!moved)
				gameOver = true;
		}
		done = true;
	}

	bool isInArea(Vector2 vec) {
		return 0 <= vec.x && vec.x <= 3 && 0 <= vec.y && vec.y <= 3;
	}

	void Spawn() {

		spawnWaiting = false;
		bool oc = true;
		int xx = 0;
		int yy = 0;
		// spawn a cell in our array random(x)/random(y)
		while (oc) {
			xx = Random.Range(0,4);
			yy = Random.Range(0,4);
			// do it if this cell isn't occuped
			if(grid[xx,yy] == null) oc = false;
		}
		// define value to spawn - 4 or sometimes 2
		int startValue = 4; // 88% to be a 4
		if(Random.value < 0.88f)
			startValue = 2; // 12% to be a 2

		GameObject tempTile = (GameObject)Instantiate (tileFab, gridToWorld (xx, yy), Quaternion.Euler (0, 0, 0));
		grid [xx, yy] = tempTile.GetComponent<Tile>();
		grid [xx, yy].tileValue = startValue;
	}

	public static Vector2 gridToWorld(int x, int y) {
		return new Vector2(1.7f * x  + 0.95f, gridY [y]);
	}
	
	public static Vector2 worldToGrid(float x, float y) {
		for (int i = 0; i <= 3; i++) {
			if(gridY[i] == y) y = i;
		}
		return new Vector2((x - 0.95f)/1.7f, y);
	}
}