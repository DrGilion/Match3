using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public enum GameState{
	Init,
	Ready,
	Swapping,
	Animating
}

public struct MatchingInfo{
	public string type;
	public List<Vector2> matchedItems;
	public int lineLength;
}

public class GameManager : MonoBehaviour {

	public Item[,] grid { get; set; }
	public const int size = 10;
	public int points { get; set; }
	public TextMesh pointstext = new TextMesh();

	public List<Item> items;
	public const int matchingSize = 3;



	private GameState state = GameState.Init;
	private Item selectedItem = null;
	private HashSet<Item> toRemove = new HashSet<Item>();


	void Awake(){
		points = 0;
		buildGUI();
		grid = null;
	}

	// Use this for initialization
	void Start () {
		do {
			removeItems();
			initializeGrid();
			verticalMatches().ForEach (info => info.matchedItems.ForEach (item => toRemove.Add (grid [(int)item.x, (int)item.y])));
			horizontalMatches().ForEach (info => info.matchedItems.ForEach (item => toRemove.Add (grid [(int)item.x, (int)item.y])));
		} while(toRemove.Count > 0);

		state = GameState.Ready;
	}

	// Update is called once per frame
	void Update () {
		
		if(state == GameState.Ready && Input.GetMouseButtonDown (0)){
			RaycastHit hit;
			bool b = Physics.Raycast ((Camera.main.ScreenPointToRay (Input.mousePosition)), out hit);
			if(b){
				Item hitGO = hit.collider.gameObject.GetComponent<Item>();
				if(selectedItem == null){
					selectedItem = hitGO;
					selectedItem.select();
				}else{
					if(selectedItem == hitGO){
						hitGO.shake();

					}else{
						StartCoroutine ( swapItems (selectedItem, hitGO));
					}
					selectedItem.deselect();
					selectedItem = null;
				}
			}
		}
	}

	public void initializeGrid(){
		if(grid == null) grid = new Item[size,size];
		for( int col = 0 ; col < size ; col++){
			for( int row = 0 ; row < size ; row++){
				if (grid [col, row] == null) {
					Item tmp = (Item)Instantiate (items [(int)Mathf.Floor (Random.Range (0.0f, items.Count))], new Vector3 (col, row, 0), Quaternion.identity);
					tmp.column = col;
					tmp.row = row;
					float scaling = 0.75f;
					tmp.transform.localScale = new Vector3 (scaling, scaling, scaling);
					grid [col, row] = tmp;
				}
			}
		}
	}

	public void buildGUI(){
		//pointstext = Instantiate (pointstext);
		pointstext.text = "Points: " + points;
	}

	public void removeItems(){
		
		List<Item> tmp = new List<Item>(toRemove);
		for(int i = tmp.Count-1 ; i >= 0 ; i--){
			grid [tmp [i].column, tmp [i].row] = null;
			Destroy (tmp[i].gameObject);
		}
		toRemove.Clear();

	}
		

	public bool isMatchingLocal(int x, int y){
		
		return false;
	}

	public List<MatchingInfo> verticalMatches(){
		List<MatchingInfo> result = new List<MatchingInfo>();
		for (int col = 0; col < size; col++) {
			List<MatchingInfo> matches = new List<MatchingInfo>();
			for (int row = 0; row < size; row++) {
				MatchingInfo match = new MatchingInfo();
				int startindex = row;
				int endindex = row;
				string matchType = grid [col, row].type;

				for(int tmprow = row+1 ; tmprow < size ; tmprow++){
					if(grid[col,tmprow].type == matchType){
						endindex = tmprow;
					}else{
						row = tmprow - 1;
						break;
					}
				}

				int len = 1 + endindex - startindex;
				if(len >= matchingSize){
					match.lineLength = len;
					match.type = matchType;
					List<Vector2> retval = new List<Vector2>();
					for(int start = startindex ; start <= endindex ; start++){
						retval.Add(new Vector2(col,start));
					}
					match.matchedItems = retval;
					matches.Add (match);

				}

			}
			result.AddRange (matches);
		}

		return result;
	}

	public List<MatchingInfo> horizontalMatches(){
		List<MatchingInfo> result = new List<MatchingInfo>();
		for (int row = 0; row < size; row++) {
			List<MatchingInfo> matches = new List<MatchingInfo>();
			for (int col = 0; col < size; col++) {
				MatchingInfo match = new MatchingInfo();
				int startindex = col;
				int endindex = col;
				string matchType = grid [col, row].type;
				for(int tmpcol = col+1 ; tmpcol < size ; tmpcol++){
					if(grid[tmpcol,row].type == matchType){
						endindex = tmpcol;
					}else{
						col = tmpcol - 1;
						break;
					}
				}

				int len = 1 + endindex - startindex;
				if(len >= matchingSize){
					match.lineLength = len;
					match.type = matchType;
					List<Vector2> retval = new List<Vector2>();
					for(int start = startindex ; start <= endindex ; start++){
						retval.Add(new Vector2(start,row));
					}
					match.matchedItems = retval;
					matches.Add (match);

				}

			}
			result.AddRange (matches);
		}

		return result;
	}

	public List<Item> globalMatchedItems(){
		List<Item> result = new List<Item> ();
		verticalMatches().ForEach (info => info.matchedItems.ForEach (item => {
			if(!result.Contains(grid [(int)item.x, (int)item.y])) result.Add (grid [(int)item.x, (int)item.y]);
		}));
		horizontalMatches().ForEach (info => info.matchedItems.ForEach (item => {
			if(!result.Contains(grid [(int)item.x, (int)item.y])) result.Add (grid [(int)item.x, (int)item.y]);
		}));
		return result;
	}

	public IEnumerator swapItems(Item a,Item b){
		if(a.isNeighbor(b)){
			int tmprow = a.row;
			int tmpcol = a.column;
			Item tmpitem = a;

			grid [a.column, a.row] = b;
			a.row = b.row;
			a.column = b.column;

			grid [b.column, b.row] = tmpitem;
			b.row = tmprow;
			b.column = tmpcol;


			StartCoroutine (ItemSwap(a,b,0.5f));
			yield return new WaitForSeconds(0.75f);
			if(verticalMatches ().Count == 0 && horizontalMatches ().Count == 0){
				grid [b.column, b.row] = a;
				b.row = a.row;
				b.column = a.column;

				grid [a.column, a.row] = tmpitem;
				a.row = tmprow;
				a.column = tmpcol;


				StartCoroutine (ItemSwap(b,a,0.2f));
			}else{
				var matches = globalMatchedItems ();
				for(int i = matches.Count-1 ; i >= 0 ; i--){
					grid [matches[i].column, matches[i].row] = null;
					points += matches[i].explode();
					initializeGrid ();
					pointstext.text = "Points: " + points;
				}
			}

		}else{
			a.shake();
			b.shake();
		}
	}

	public IEnumerator ItemSwap(Item a ,Item b, float time){
		Vector3 tmpa = a.transform.position;
		Vector3 tmpb = b.transform.position;
		float i = 0;
		while (i < 1) {
			i += Time.deltaTime / time;

			a.transform.position = Vector3.Slerp (a.transform.position, tmpb, i);
			b.transform.position = Vector3.Slerp (b.transform.position, tmpa, i);

			yield return null;
		}
	}
}




