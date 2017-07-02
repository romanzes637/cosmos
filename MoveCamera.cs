using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
	public float speed = 0.1F;
	public Vector3 lastPosition;
	Camera cam;

	// Use this for initialization
	void Start ()
	{
		cam = GetComponent<Camera> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		#if UNITY_EDITOR
		if (Input.GetMouseButtonDown (0) && cam.pixelRect.Contains (Input.mousePosition)) {
			lastPosition = Input.mousePosition;
		} else if (Input.GetMouseButton (0) && cam.pixelRect.Contains (Input.mousePosition)) {
			Vector3 delta = Input.mousePosition - lastPosition;
			transform.Translate (-delta.x * speed, -delta.y * speed, 0);
			lastPosition = Input.mousePosition;
		}
		if (Input.GetAxis ("Mouse ScrollWheel") > 0 && cam.pixelRect.Contains (Input.mousePosition)) {
			cam.orthographicSize = Mathf.Clamp(--cam.orthographicSize, 1, 50);
		}
		if (Input.GetAxis ("Mouse ScrollWheel") < 0 && cam.pixelRect.Contains (Input.mousePosition)) {
			cam.orthographicSize = Mathf.Clamp(++cam.orthographicSize, 1, 50);
		}
		#endif
		if (Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Moved) {
			Vector2 touchDeltaPosition = Input.GetTouch (0).deltaPosition;
			transform.Translate (-touchDeltaPosition.x * speed * Time.deltaTime, -touchDeltaPosition.y * speed * Time.deltaTime, 0);
		} 
	}
}
