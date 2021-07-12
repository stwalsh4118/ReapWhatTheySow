using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    public List<Transform> onBoard;
    public enum Direction {
        Left,
        Right,
        Up,
        Down
    }
    public float conveyorSpeed = 1f;

    public Direction dir;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {   
        //if there is nothing on the conveyor belt do nothing
        if(onBoard.Count == 0) {return;}

        //if the object that was on the conveyor belt was destroyed for some reason, remove it from the onboard list
        if(onBoard[0] == null) {
            onBoard.RemoveAt(0);
            return;
        }

        //move the first object on the conveyor belt according to the direction of the conveyor belt
        switch(dir) {
            case Direction.Left:
                onBoard[0].position = new Vector3(onBoard[0].position.x - conveyorSpeed * Time.deltaTime, onBoard[0].position.y, onBoard[0].position.z);
                break;
            case Direction.Right:
                onBoard[0].position = new Vector3(onBoard[0].position.x + conveyorSpeed * Time.deltaTime, onBoard[0].position.y, onBoard[0].position.z);
                break;
            case Direction.Up:
                onBoard[0].position = new Vector3(onBoard[0].position.x, onBoard[0].position.y, onBoard[0].position.z + conveyorSpeed * Time.deltaTime);
                break;
            case Direction.Down:
                onBoard[0].position = new Vector3(onBoard[0].position.x, onBoard[0].position.y, onBoard[0].position.z - conveyorSpeed * Time.deltaTime);
                break;
            default:
                Debug.Log("No Direction Set For Conveyor");
                break;
        }
    }

    //when something gets on the conveyor belt add it to the list
    private void OnTriggerEnter(Collider other) {
        if(other.tag == "Player") {return;} 
        Debug.Log(other.transform.name + "has gotten on the conveyor");
        onBoard.Add(other.transform);
    }

    //when something leaves the conveyor belt remove it from the list
    private void OnTriggerExit(Collider other) {
        if(other.tag == "Player") {return;}
        Debug.Log(other.transform.name + "has gotten off the conveyor");
        onBoard.Remove(other.transform);
    }
}
