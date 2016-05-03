using UnityEngine;
using System.Collections;

public class CameraContolsScript : MonoBehaviour {

	Camera cam;
	public float maxDisplacement;
	public float scrollSpeed;
	public float maxZoom;
	public float minZoom;
	public float zoomSpeed;

	// Use this for initialization
	void Start () {
		cam = gameObject.GetComponent<Camera> ();
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKey(KeyCode.UpArrow)) {
			cam.transform.Translate (new Vector3 (0f, scrollSpeed, 0f));
			if (cam.transform.position.y > maxDisplacement) {
				cam.transform.Translate (new Vector3 (0f, maxDisplacement - cam.transform.position.y , 0f));
			}
		}
		if(Input.GetKey(KeyCode.DownArrow)) {
			cam.transform.Translate (new Vector3 (0f, -scrollSpeed, 0f));
			if (cam.transform.position.y < -maxDisplacement) {
				cam.transform.Translate (new Vector3 (0f,-maxDisplacement - cam.transform.position.y, 0f));
			}
		
		}
		if(Input.GetKey(KeyCode.LeftArrow)) {
			cam.transform.Translate (new Vector3 (-scrollSpeed, 0f, 0f));
			if (cam.transform.position.x < -maxDisplacement) {
				cam.transform.Translate (new Vector3 (- maxDisplacement - cam.transform.position.x, 0f, 0f));
			}
				
		}
		if(Input.GetKey(KeyCode.RightArrow)) {
			cam.transform.Translate (new Vector3 (scrollSpeed, 0f, 0f));
			if (cam.transform.position.x > maxDisplacement) {
				cam.transform.Translate (new Vector3 (maxDisplacement - cam.transform.position.x, 0f, 0f));
			}
						
		}
		if (Input.GetKey(KeyCode.A)) {
			cam.orthographicSize -= zoomSpeed;
			if (cam.orthographicSize < minZoom) {
				cam.orthographicSize = minZoom;
			}
		}
		if (Input.GetKey (KeyCode.S)) {
			cam.orthographicSize += zoomSpeed;
			if (cam.orthographicSize > maxZoom) {
				cam.orthographicSize = maxZoom;
			}
		}
	}
}
