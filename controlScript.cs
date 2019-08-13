using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class controlScript : MonoBehaviour
{
    public SpriteRenderer startRef;
    public SpriteRenderer goalRef;

    public pathfinding mapData;
    public Camera mainCamera;
    public SpriteRenderer playerSprite;

    public bool canInteract = true;
    public float moveSpeed = 3f;

    public bool canMove = true;
    public Vector3 targetPoint;
    public int currentMovePoint = 0;
    private List<Vector3> moveDirections = new List<Vector3>();
    public float maxInteractDistance = 0.32f;

    public bool canBuild = true;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        setCamera();
    }

    // Update is called once per frame
    void Update()
    {
        if( canMove && Input.GetMouseButtonDown(0) ) // && !EventSystem.current.IsPointerOverGameObject() )
        {
            Vector2 mousePosition = Input.mousePosition;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            OnSingleTap(worldPosition);
        }

        if( canMove && Input.touchCount > 0 )
        {
            Touch currentInput = Input.GetTouch(0);
            Vector2 screenPosition = currentInput.position;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            if( currentInput.tapCount == 1 )
            {
                OnSingleTap(worldPosition);
            }
            //else if( currentInput.tapCount == 2 )
            //{
            //    OnDoubleTap(worldPosition);
            //}
        }
        else if( canMove && currentMovePoint < moveDirections.Count ) // currently have directions to move
        {
            movePlayerUsingDirections();
        }
    }

    void OnSingleTap(Vector3 goalPosition)
    {
        setupMove(goalPosition);
    }

    void OnDoubleTap(Vector3 position)
    {
        // move the character
        // check if within range to interact
        // interact with a grid or item
    }

    void setupMove(Vector3 goalPosition)
    {
        // call to get moveDirections
        // moveDirections = 
        moveDirections.Clear();


        Vector3 startrefposition = transform.position;
        startrefposition.z = -0.5f;
        startRef.transform.position = startrefposition;

        Vector3 goalrefposition = goalPosition;
        goalrefposition.z = -0.5f;
        goalRef.transform.position = goalrefposition;

        moveDirections = mapData.getMoveDirections( transform.position,goalPosition );
        moveDirections.Reverse();
        if( moveDirections.Count > 0 )
        {
            currentMovePoint = 0;
            targetPoint = moveDirections[currentMovePoint];
        }
        if( moveDirections.Count > 0 )
        {
            moveDirections.RemoveAt(0); // delete first location to get rid of jitter
        }
    }

    void movePlayerUsingDirections()
    {
        move();
        setCamera();
    }

    void setCamera()
    {
        Vector3 tempCameraPosition = mainCamera.transform.position;
        Vector3 tempSpritePosition = transform.position;
        tempSpritePosition.z = tempCameraPosition.z;
        mainCamera.transform.position = tempSpritePosition;
    }

    void move()
    {
        // move towards the target
        targetPoint.z = transform.position.z;
        transform.position = Vector3.MoveTowards(transform.position, targetPoint, moveSpeed * Time.deltaTime);

        if (transform.position == targetPoint)
        {
            currentMovePoint++;
            if( currentMovePoint < moveDirections.Count)
            {
                targetPoint = moveDirections[currentMovePoint];
            }
        }
    }
}
