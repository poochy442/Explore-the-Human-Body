using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;

public class StateManagement : MonoBehaviour
{
    public enum State {
        Spawning,
        Examining
    }

    // Set in engine
    public Canvas Canvas;
    public Text HeaderTextOriginal, InfoTextOriginal;
    public Button CloseButtonOriginal;

    public GameObject BodyOriginal;

    public ARSessionOrigin sessionOrigin;

    // State handling
    public State CurrentState { get; set; }

    // Required for placing objects on a plane
    ARPlaneManager PlaneManager;
    ARRaycastManager RaycastManager;
    static List<ARRaycastHit> Hits = new List<ARRaycastHit>();

    // Local reference to instantiated objects
    private Text headerText, infoText;
    private Button closeButton;
    private GameObject body;

    private Camera camera;

    private void Awake()
    {
        RaycastManager = sessionOrigin.GetComponent<ARRaycastManager>();
        PlaneManager = sessionOrigin.GetComponent<ARPlaneManager>();
        camera = sessionOrigin.GetComponentInChildren<Camera>();
    }

    void Start()
    {
        // Instantiate Header text
        headerText = Instantiate(HeaderTextOriginal, Canvas.transform, false);
        headerText.name = "HeaderText";

        // Set the state to initial state
        CurrentState = State.Spawning;
    }

    void Update()
    {
        // Handle touch
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if(touch.phase == TouchPhase.Began)
            {
                switch (CurrentState)
                {
                    case State.Spawning:
                        if (DoesTouchHitPlane(touch))
                        {
                            PlaceObjectOnPlane(BodyOriginal, touch);
                            ChangeState(State.Examining);
                        }
                        break;
                    case State.Examining:
                        if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                        {
                            GameObject selectedItem = EventSystem.current.currentSelectedGameObject;
                            if (selectedItem.name == "CloseButton")
                            {
                                ChangeState(State.Spawning);
                            }
                        } else
                        {
                            // TODO: Change to Raycast hit body part
                            DoesTouchHitBody(touch);
                            /* System.Random r = new System.Random();
                            string[] bodyParts = new string[] { "Spine", "Elbow", "Head", "Legs" };
                            infoText.text = bodyParts[r.Next(bodyParts.Length)]; */
                        }
                        break;
                }
                
            }
        }
    }

    private void ChangeState(State newState)
    {
        // Change UI depending on state
        switch (newState)
        {
            case State.Spawning:
                headerText.text = "Select Location";
                Destroy(closeButton);
                Destroy(body);
                SetAllPlanesActive(true);
                break;
            case State.Examining:
                headerText.text = "Click the body to learn more";

                closeButton = Instantiate<Button>(CloseButtonOriginal, Canvas.transform, false);
                closeButton.name = "CloseButton";

                infoText = Instantiate<Text>(InfoTextOriginal, Canvas.transform, false);
                infoText.name = "InfoText";

                SetAllPlanesActive(false);
                break;
        }

        // Set new state
        CurrentState = newState;
    }

    // Check if a touch does hit a plane
    private bool DoesTouchHitPlane(Touch touch)
    {
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        RaycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon);

        if (hits.Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Check if a touch hits the body
    private bool DoesTouchHitBody(Touch touch)
    {
        Ray ray = camera.ScreenPointToRay(touch.position);
        RaycastHit hit;
        
        if (Physics.Raycast(ray.origin, ray.direction, out hit))
        {
            infoText.text = hit.collider.gameObject.name;
            return true;
        }
        else
        {
            infoText.text = "No hit";
            return false;
        }
    }

    // Place an object on a hit plane
    private void PlaceObjectOnPlane(GameObject ObjectToPlace, Touch touch)
    {
        if (RaycastManager.Raycast(touch.position, Hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = Hits[0].pose;

            body = Instantiate(ObjectToPlace, hitPose.position, hitPose.rotation) as GameObject;
            body.name = "Body";
        }
    }

    // Hide plane detection
    private void SetAllPlanesActive(bool value)
    {
        foreach (var plane in PlaneManager.trackables)
            plane.gameObject.SetActive(value);
    }

}
