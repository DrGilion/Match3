using UnityEngine;
using System.Collections;

public class Item : MonoBehaviour {

	public const int points = 5;
	public int row { get; set; }
	public int column { get; set; }
	public string type{ get; set;}

	private bool shaking = false;

	// How long the object should shake for.
	private float shakeDuration = 0.0f;

	public float shakingTime = 0.5f;
	public float shakeAmount = 0.005f;
	public float decreaseFactor = 1.0f;

	public GameObject explosionGO;

	Vector3 originalPos;

	Color originalColor;
	Color touchColor = new Color(0.2f,0.2f,0.2f,1);

	void Awake(){
		type = this.GetComponent<Item>().name;
		//explosionGO = GameObject.FindGameObjectWithTag ("Explosion");
	}

	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	void Update () {
		
	}

	void OnDestroy(){
		
	}

	private IEnumerator shakeAnimation(){
		originalPos = this.transform.position;
		shakeDuration = shakingTime;
		shaking = true;
		while(shaking){
			this.transform.position = originalPos + Random.insideUnitSphere * shakeAmount;

			shakeDuration -= Time.deltaTime * decreaseFactor;
			if(shakeDuration <= 0.0f){
				shaking = false;
				this.transform.position = originalPos;
			}
			yield return null;
		}
	}

	public void shake(){
		//Debug.Log("Ich schüttele mich: " + type + " (Column: " + column + ", Row: " + row + ")");
		StartCoroutine (shakeAnimation ());

	}

	public int explode(){
		//Debug.Log("Explosion: " + type + " (Column: " + column + ", Row: " + row + ")");
		explosionGO = Instantiate(explosionGO);
		explosionGO.GetComponent<ParticleSystem>().Play();
		explosionGO.transform.position = this.transform.position;
		Destroy(explosionGO, explosionGO.GetComponent<ParticleSystem> ().duration);
		Destroy(this.gameObject, explosionGO.GetComponent<ParticleSystem> ().duration);
		return points;

	}

	public void select(){
		Renderer r = GetComponent<Renderer> ();
		originalColor = r.material.color;
		r.material.color += touchColor;
	}

	public void deselect(){
		 GetComponent<Renderer> ().material.color = originalColor;
	}

	public bool isNeighbor(Item other){
		return (this.column == other.column || this.row == other.row)
			&& Mathf.Abs(this.column - other.column) <= 1
			&& Mathf.Abs(this.row -other.row) <= 1;
	}

	public override string ToString ()
	{
		return string.Format ("[Item: row={0}, column={1}, type={2}]", row, column, type);
	}
}
