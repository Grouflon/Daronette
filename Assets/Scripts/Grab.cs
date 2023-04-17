using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grab : MonoBehaviour
{
    public float grabDistance = 10.0f;
    public float grabbedSize = 2.0f;
    public Vector3 grabOffset = new Vector3(0.0f, 0.0f, 3.0f);
    public float rotationSpeed = 5.0f;
    public float lerpTime = 0.3f;

    public RectTransform grabPrompt;

    public void DoGrab()
    {
        if (m_grabCandidate == null)
            return;

        GetComponentInChildren<CamMouseLook>().busy = true;
        GetComponent<FPSPlayerController>().canMove = false;

        m_grabbedObject = m_grabCandidate;
        m_grabCandidate = null;
        m_grabbedTime = Time.timeSinceLevelLoad;
        m_grabbedObjectBasePosition = m_grabbedObject.position;
        m_grabbedObjectBaseRotation = m_grabbedObject.rotation;
        m_grabbedObjectBaseScale = m_grabbedObject.localScale;

        m_grabbedObject.SetParent(Camera.main.transform);

        m_positionError = m_grabbedObject.localPosition - grabOffset;
        m_rotationError = m_grabbedObject.localRotation;
        m_scaleError = 0.0f;

        m_manualRotation = Quaternion.identity;

        MeshRenderer renderer = m_grabbedObject.GetComponentInChildren<MeshRenderer>();
        if (renderer != null)
        {
            Bounds bounds = renderer.bounds;
            float maxDimension = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z) / m_grabbedObject.localScale.x;
            if (maxDimension > 0.0f)
            {
                m_targetScale = grabbedSize / maxDimension;
                m_scaleError = m_grabbedObject.localScale.x - m_targetScale;
            }
        }
    }

    public void Drop()
    {
        if (m_grabbedObject == null)
            return;

        GetComponentInChildren<CamMouseLook>().busy = false;
        GetComponent<FPSPlayerController>().canMove = true;

        m_grabbedObject.SetParent(null);
        m_grabbedObject.position = m_grabbedObjectBasePosition;
        m_grabbedObject.rotation = m_grabbedObjectBaseRotation;
        m_grabbedObject.localScale = m_grabbedObjectBaseScale;

        m_grabbedObject = null;
    }
    

    public Transform GetGrabbedObject()
    {
        return m_grabbedObject;
    }

    public Transform GetGrabbedCandidate()
    {
        return m_grabCandidate;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (grabPrompt != null)
        {
            grabPrompt.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool grabDown = Input.GetButtonDown("Grab");
        float factor = 50.0f;
        float time = Time.deltaTime;
        var input = new Vector2(Input.GetAxisRaw("Look X"), Input.GetAxisRaw("Look Y"));
        var mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        if (mouseInput.magnitude > 0.0f)
        {
            input = mouseInput;
            factor = 1.0f;
            time = 1.0f;
        }
        input *= factor;

        m_grabCandidate = null;
        if (grabPrompt != null)
        {
            grabPrompt.gameObject.SetActive(false);
        }

        if (m_grabbedObject == null)
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit, grabDistance, LayerMask.GetMask("Items"), QueryTriggerInteraction.Collide))
            {
                m_grabCandidate = hit.collider.gameObject.transform;
                if (grabPrompt != null)
                {
                    grabPrompt.gameObject.SetActive(true);
                }
                //Debug.Log(m_grabCandidate);
            }
            else
            {
                //Debug.Log("Rien");
            }
        }
        else
        {
            m_manualRotation = Quaternion.Euler(0.0f, input.x * rotationSpeed * time, 0.0f) * m_manualRotation;
            m_manualRotation = Quaternion.Euler(input.y * rotationSpeed * time, 0.0f, 0.0f) * m_manualRotation;

            float t = Mathf.Clamp01((Time.timeSinceLevelLoad - m_grabbedTime) / lerpTime);
            t = Ease.QuadOut(t);
            Vector3 positionError = Vector3.Lerp(m_positionError, Vector3.zero, t);
            Quaternion rotationError = Quaternion.Lerp(m_rotationError, Quaternion.identity, t);
            float scaleError = Mathf.Lerp(m_scaleError, 0.0f, t);

            m_grabbedObject.localPosition = grabOffset + positionError;
            m_grabbedObject.localRotation = rotationError * m_manualRotation;
            float scale = m_targetScale + scaleError;
            m_grabbedObject.localScale = new Vector3(scale, scale, scale);
        }

        if (grabDown) 
        {
            if (m_grabbedObject != null)
            {
                Drop();
            }
            else if (m_grabCandidate != null)
            {
                DoGrab();
            }
        }
    }

    Transform m_grabCandidate = null;
    Transform m_grabbedObject = null;
    Vector3 m_grabbedObjectBasePosition;
    Quaternion m_grabbedObjectBaseRotation;
    Vector3 m_grabbedObjectBaseScale;

    Vector3 m_positionError;
    Quaternion m_rotationError;
    float m_scaleError;
    float m_targetScale;

    Quaternion m_manualRotation;

    float m_grabbedTime = 0.0f;
}
