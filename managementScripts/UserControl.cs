using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserControl : MonoBehaviour
{
    public float panSpeed;
    public float rotateSpeed;
    public float rotateAmount;

    public ActivePlayer player;

    private Transform selfTransform;
    public Quaternion rotation;

    private float panDetect = 15f;
    private float minHeight;
    private float maxHeight;

    private float MIN_X = 0;
    private float MAX_X = 1000;

    private float MIN_Z = 0;
    private float MAX_Z = 1000;

    private Vector3 clampedPosition;

    public GameObject selectedObject;
    public GameObject prevSelectedObject;
    public GameObject tempObject;
    public int selectedObjectType;

    public int counter = 0;

    public Transform forwardDirector, backwardDirector, leftDirector, rightDirector;

    public bool cursorIsBusy;

    public GameObject techTree, mainMenu;
    public float terrheight;

    public ParticleSystem rightClickOnEnvEffect;

    // Start is called before the first frame update
    void Start()
    {
        selfTransform = transform;
        rotation = transform.rotation;
        player = GetComponent<ActivePlayer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!techTree.activeSelf && !mainMenu.activeSelf && !player.subtitles.activeSelf)
        {
            MoveCamera();
            RotateCamera();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                selfTransform.rotation = rotation;
            }
        }
    }

    void MoveCamera()
    {
        terrheight = Terrain.activeTerrain.SampleHeight(selfTransform.position) + Terrain.activeTerrain.transform.position.y;

        if(terrheight > 49)
        {
            minHeight = terrheight += 10f;
            maxHeight = terrheight += 30f;
        }

        float xPos = Input.mousePosition.x;
        float yPos = Input.mousePosition.y;

        if (Input.GetKey(KeyCode.A) || xPos > 0 && xPos < panDetect)
        {
            selfTransform.position = Vector3.MoveTowards(selfTransform.position, leftDirector.position, panSpeed);
        }
        else if (Input.GetKey(KeyCode.D) || xPos < Screen.width && xPos > Screen.width - panDetect)
        {
            selfTransform.position = Vector3.MoveTowards(selfTransform.position, rightDirector.position, panSpeed);
        }
        if (Input.GetKey(KeyCode.W) || yPos < Screen.height && yPos > Screen.height - panDetect)
        {
            selfTransform.position = Vector3.MoveTowards(selfTransform.position, forwardDirector.position, panSpeed);
        }
        else if (Input.GetKey(KeyCode.S) || yPos > 0 && yPos < panDetect)
        {
            selfTransform.position = Vector3.MoveTowards(selfTransform.position, backwardDirector.position, panSpeed);
        }

        selfTransform.Translate(selfTransform.forward * Input.GetAxis("Mouse ScrollWheel") * 20, Space.World);

        clampedPosition.x = Mathf.Clamp(selfTransform.position.x, MIN_X, MAX_X);
        clampedPosition.y = Mathf.Clamp(selfTransform.position.y, minHeight, maxHeight);
        clampedPosition.z = Mathf.Clamp(selfTransform.position.z, MIN_Z, MAX_Z);

        selfTransform.position = clampedPosition;
    }

    private void RotateCamera()
    {
        Vector3 origin = selfTransform.eulerAngles;
        Vector3 destination = origin;

        if (Input.GetKey(KeyCode.E))
        {
            destination.y += Input.GetAxis("Mouse X") * rotateAmount;
        }

        if (destination != origin)
        {
            selfTransform.eulerAngles = Vector3.MoveTowards(origin, destination, Time.deltaTime * rotateSpeed);
        }
    }
}
