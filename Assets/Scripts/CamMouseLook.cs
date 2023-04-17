using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamMouseLook : MonoBehaviour {

    Vector2 mouseLook;
    Vector2 smoothV;
    public float sensitivity = 3;
    public float smoothing = 3;
    public bool busy = false;

    GameObject character;

	// Use this for initialization
	void Start () {
        character = this.transform.parent.gameObject;
	}
	
	// Update is called once per frame
	void Update () {

        if (!busy)
        {
            float factor = 50.0f;
            var md = new Vector2(Input.GetAxisRaw("Look X"), Input.GetAxisRaw("Look Y"));
            var moused = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            float time = Time.deltaTime;
            if (moused.magnitude > 0.0f)
            {
                factor = 1.0f;
                time = 1.0f;
                md = moused;
            }

            //md = Vector2.Scale(md, new Vector2(sensitivity * smoothing, sensitivity * smoothing) * Time.deltaTime);
            md = md * sensitivity * factor * time;
            if (smoothing > 0.0f)
            {
                md = md * smoothing;
                smoothV.x = Mathf.Lerp(smoothV.x, md.x, 1f / smoothing);
                smoothV.y = Mathf.Lerp(smoothV.y, md.y, 1f / smoothing);
            }
            else
            {
                smoothV = md;
            }
            
            mouseLook += smoothV;
            mouseLook.y = Mathf.Clamp(mouseLook.y, -90, 90);

            transform.localRotation = Quaternion.AngleAxis(-mouseLook.y, Vector3.right);
            character.transform.localRotation = Quaternion.AngleAxis(mouseLook.x, character.transform.up);
        } 
	}
}
