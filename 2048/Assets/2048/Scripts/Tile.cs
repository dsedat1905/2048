using UnityEngine;
using System.Collections;
// a sound controller is needed
[RequireComponent(typeof(AudioSource))]

// for each cell we create an instantiation which is able to move or combine with others
public class Tile : MonoBehaviour {

	public GameObject textFab;
	// simple sound when a cell moves
	public AudioClip FX;
	// value which is written on each cell
	public int tileValue;
	// same value? Do a combinaison...
	public bool combined;
	
	Vector2 movePosition;
	bool combine;
	Tile cTile;
	bool grow;

	void Start () {
		// position of our cell and creation
		movePosition = transform.position;
		textFab = (GameObject)Instantiate (textFab,transform.position,Quaternion.Euler(0,0,0));
		Change (tileValue);
	}

	void Update () {

		textFab.GetComponent<GUIText>().transform.position = Camera.main.WorldToViewportPoint (transform.position);

		if(transform.position != new Vector3(movePosition.x,movePosition.y,0f)) {
			Manager.done = false;
			// we move our cell slowly
			transform.position = Vector3.MoveTowards(transform.position,movePosition, 35 * Time.deltaTime);
		} else {
			Manager.done = true;
			// do a combinaison and increase valur on cell
			if(combine) {
				Change(tileValue * 2); // new value
				combine = false;
				grow = true;
				// destroy our cell after it has combined with other
				Destroy(cTile.textFab);
				Destroy(cTile.gameObject);
				// play a sound when cell combines with another
				GetComponent<AudioSource>().PlayOneShot(FX, 1.0f);
				Manager.done = true;
			}
		}
		// create a scale FX when it spawns
		if(transform.localScale.x != 150 && !grow)
			transform.localScale = Vector3.MoveTowards(transform.localScale,new Vector3(150f,150f,1f), 500 * Time.deltaTime);
		if(grow) { // create a scale FX when cell combines with another
			Manager.done = false;
			transform.localScale = Vector3.MoveTowards(transform.localScale,new Vector3(187.5f,187.5f,1f), 500 * Time.deltaTime);
			if(transform.localScale == new Vector3(187.5f,187.5f,1f))
                grow = false;
		} else
            Manager.done = true;
	}

	void Change (int newValue) {

		tileValue = newValue;
		// after combination we change tile's colour
		GetComponent<SpriteRenderer>().color = Manager.tileColors [Mathf.RoundToInt(Mathf.Log (tileValue, 2) - 1)];
		textFab.GetComponent<GUIText>().text = tileValue.ToString();
		// colour value which is written on our cell
		textFab.GetComponent<GUIText>().color = new Color (0.17f, 0.17f, 0.27f);
	}

	public bool Move (int x, int y) {

		movePosition = Manager.gridToWorld (x, y);

		if(transform.position != (Vector3)movePosition) {
			if(Manager.grid [x, y] != null) {
				combine = true;
				combined = true;
				cTile = Manager.grid[x,y];
				Manager.grid[x,y] = null;
			} 
			// move the cell
			Manager.grid[x,y] = GetComponent<Tile>();
			Manager.grid[Mathf.RoundToInt(Manager.worldToGrid(transform.position.x,transform.position.y).x),Mathf.RoundToInt(Manager.worldToGrid(transform.position.x,transform.position.y).y)] = null;
			return true;

		} else
            return false;
	}
}