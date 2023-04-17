using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Friend : MonoBehaviour
{
    public float stickDistance = 10.0f;
    public float corridorHalfSize = 5.0f;
    public float disappearStartDistance = -1.0f;
    public float disappearEndDistance = -4.0f;
    public Vector3 stickPosition = new Vector3(0.0f, -3.0f, 10.0f);
    public Quaternion stickRotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);

    public MeshRenderer videoRenderer;

    // Start is called before the first frame update
    void Start()
    {
        basePosition = transform.position;
        baseRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 playerToFriend = basePosition - Camera.main.transform.position;
        float linearDistance = Vector3.Dot(playerToFriend, baseRotation * Vector3.forward);
        float horizontalDistance = Vector3.Dot(playerToFriend, baseRotation * Vector3.right);

        //Debug.Log(linearDistance);

        if (linearDistance < stickDistance || Mathf.Abs(horizontalDistance) > corridorHalfSize)
        {
            transform.SetParent(null);
            transform.position = basePosition;
            transform.rotation = baseRotation;
        }
        else
        {
            transform.SetParent(Camera.main.transform);
            transform.localRotation = stickRotation;
            transform.localPosition = stickPosition;

            float alpha = 1.0f - Mathf.Clamp01((linearDistance - disappearStartDistance) / (disappearEndDistance - disappearStartDistance));
            //Debug.Log(alpha);
            videoRenderer.material.SetFloat("_Alpha", alpha);
            if (alpha == 0.0f)
            {
                gameObject.SetActive(false);
            }
        }
    }

    Vector3 basePosition;
    Quaternion baseRotation;
}
