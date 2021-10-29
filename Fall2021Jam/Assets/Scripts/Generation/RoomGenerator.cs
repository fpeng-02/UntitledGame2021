using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    /*Class: Node
     * node class that contains the information for one room. 
    */
    public class Node
    {
        //local vars
        private Vector2 pos;       //location of the lower left corner of this room, in global space
        private Node parent;                //parent of the node
        private List<Node> children;        //children of the node
        private Room repRoom;               //Room gameobject the node represents

        //constructor 
        public Node(Vector2 pos, Node parent, Room repRoom)
        {
            this.pos = pos;
            this.parent = parent;
            this.repRoom = repRoom;
            children = new List<Node>();
        }

        //Getters
        public Vector2 GetPos() { return this.pos; }
        public List<DoorCoord> GetDoorCoords() { return repRoom.GetDoorCoords(); }
        public List<Vector2> GetFill() { return repRoom.GetFill(); }
        public Room GetRoom() { return this.repRoom; }

        //Add a child node to the child list
        public void AddChild(Node child)
        {
            children.Add(child);
        }

        //choose a random (kinda?) door from the availible non closed doors
    }

    [SerializeField] private float roomSize;
    [SerializeField] private List<Room> rooms;
    [SerializeField] private Room startRoom;
    [SerializeField] private int maxRoom;

    private List<Vector3> debug; // stores anchor of each generated room, connecting them will give a sort of "path" that gen followed


    private List<Vector2> occupiedCoord = new List<Vector2>();

    private Node startNode;     //start node/root node
    private Node currNode;      //current/working node
    private DoorCoord currDoor; //current/working door
    private int roomCount;      //overall room count

    private Vector2 nextDoorSquare = new Vector2(0, 0); //offset from origin


    void Start()
    {
        debug = new List<Vector3>();
        GenerateNodes();
    }

    public void GenerateNodes()
    {
        //create first start room node
        startNode = new Node(new Vector2(0, 0), null, startRoom);
        currNode = startNode;
        GenerateRoom(startNode.GetRoom().gameObject, startNode.GetPos());
        InitNode(currNode);
        
        while (roomCount < maxRoom)
        {
            currDoor = ChooseDoor(currNode);
            if (currDoor == null)
            {
                Debug.Log("TODO: No more open doors in the current room!");
            }
            nextDoorSquare = currDoor.NextCoord() + currNode.GetPos();
            debug.Add(new Vector3(nextDoorSquare.x, nextDoorSquare.y, -5));
            Node newNode = CheckRoom();
            GenerateRoom(newNode.GetRoom().gameObject, newNode.GetPos());
            InitNode(newNode);
            currNode.AddChild(newNode);
            currNode = newNode;
            //TODO BRANCH STOPPING MEKANISM.
        }
    }

    public Node CheckRoom()
    {
        Vector2 testPosition;
        int randRoomInd = (int)Random.Range(0, rooms.Count);
        //check all rooms starting from a random room

        // For every room, check every door alignment for whether or not we can place it somewhere.
        bool doorValid;
        bool firstValidDoor;
        Vector2 testAnchor = new Vector2(0, 0);
        List<Room> validNewRooms = new List<Room>();
        List<List<Vector2>> validNewRoomCoords = new List<List<Vector2>>();  // list will be parallel to validNewRooms
        List<Vector2> t = null;

        foreach (Room room in rooms) {
            firstValidDoor = true;  // used to make sure a list is initialized properly
            foreach (DoorCoord door in room.GetDoorCoords()) {
                if (door.GetDir() != -currDoor.GetDir()) continue;  // only look for doors that are aligned with the current one used in generation
                doorValid = true;  // assume the current door is valid; if we find that placement fails, this will become false
                testAnchor = nextDoorSquare - door.GetCoord();
                foreach (Vector2 fillTest in room.GetFill()) {  // test the situation: if we placed a room down using this door for alignment, will it overlap?
                    if (occupiedCoord.Contains(testAnchor + fillTest)) {
                        doorValid = false;
                        break;
                    }
                }
                if (doorValid) {
                    if (firstValidDoor) {
                        validNewRooms.Add(room);  // one valid door validates the room! also, this will only be hit once.
                        validNewRoomCoords.Add(new List<Vector2>());
                        firstValidDoor = false;
                        t = validNewRoomCoords[validNewRoomCoords.Count - 1];
                    }
                    t.Add(testAnchor);  // t will be initialized since the if condition is always hit before
                }
            }
        }
        if (validNewRooms.Count == 0) {
            Debug.Log("No valid rooms found!");
            return null;
        }
        else {
            int chosenRoomIndex = Random.Range(0, validNewRooms.Count);
            int chosenAnchorIndex = Random.Range(0, validNewRoomCoords[chosenRoomIndex].Count);
            return new Node(validNewRoomCoords[chosenRoomIndex][chosenAnchorIndex], currNode, validNewRooms[chosenRoomIndex]);
        }

        /*
        for (int i = 0; i < rooms.Count; i++)
        {
            Room randRoom = rooms[(randRoomInd + i) % (int)rooms.Count];
            List<DoorCoord> randRoomDoors = randRoom.GetDoorCoords();
            int randDoorOffset = (int)Random.Range(0, randRoomDoors.Count);
            //for every room check if there are doors that match the direction of our current door
            for (int j = 0; j < randRoomDoors.Count; j++)
            {
                //If there is an opposite corresponding door on the room to the current door, 
                //check if the current room fits if the two doors are attached.
                if (randRoomDoors[(j + randDoorOffset) % randRoomDoors.Count].GetDir() == -currDoor.GetDir())
                {
                    //Moves the offset to the bottom left of the Room
                    testPosition = nextDoorSquare - randRoomDoors[j].GetCoord();

                    bool isValid = true;
                    List<Vector2> randRoomFill = randRoom.GetFill();
                    //check if every space in that the room would occupy is full 
                    for (int k = 0; k < randRoomFill.Count; k++)
                    {
                        //if the offsetted piece of room is already taken, the room is invalid and the loop ends.
                        if (occupiedCoord.Contains(testPosition + randRoomFill[k]))
                        {
                            isValid = false;
                            break;
                        }
                    }
                    //If all spots of the room are valid, then return a new node with 
                    if (isValid)
                    {
                        return new Node(testPosition, currNode, randRoom);
                    }

                }
            }
        }
        Debug.Log("TODO: No valid rooms found!");
        return null;*/
    }

    //Chose an open door 
    public DoorCoord ChooseDoor(Node currNode)
    {
        int counter = 0;
        List<DoorCoord> doors = currNode.GetDoorCoords();
        int numInd = doors.Count;

        DoorCoord randDoor;
        //start at a random door in the door array
        int randInd = (int)Random.Range(0, numInd);
        //check consequtively increasing doors, starting at the random starting point, to find a non filled door
        while (counter < numInd)
        {
            randDoor = doors[(randInd + counter) % ((int)numInd)];
            //if door is not filled && the spot that the door leads to is not filled then return it
            if (!randDoor.getFilled() && !occupiedCoord.Contains(randDoor.NextCoord() + currNode.GetPos()))
            {
                return randDoor;
            }
            counter += 1;
        }
        return null;
    }

    //Updates room Count and adds the spaces of the newNode to the occupiedCoord list.
    public void InitNode(Node newNode)
    {
        //NOTE: USER DEFINED CLASSES ARE PASSED BY VALUE NOT REFERENCE
        newNode.GetFill().ForEach(fillSquare => occupiedCoord.Add(fillSquare + newNode.GetPos()));
        roomCount++;
    }

    //Convert The Nodes Into Stage GameObjects
    public void GenerateStage()
    {

    }


    public void GenerateRoom(GameObject room, Vector2 coords)
    {
        Instantiate(room, coords * roomSize, room.transform.rotation);
    }

    public void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        for (int i = 0; i < debug.Count - 1; i++)
            Gizmos.DrawLine(debug[i] * roomSize, debug[i+1] * roomSize);
    }
}
