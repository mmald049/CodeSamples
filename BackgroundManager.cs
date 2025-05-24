using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BackgroundManager : MonoBehaviour
{
    public List<Transform> shapesUp;
    public List<Transform> shapesDown;

    public GameObject[] points;

    public GameObject pointsPivot;
    public GameObject playerCam;

    public Transform blocks;

    public float speed;

    public float ceiling;
    public float floor;
    public float eastWall;
    public float westWall;

    public float ceilingOffset;
    public float chunkMin;

    // Start is called before the first frame update
    void Start()
    {
        ceilingOffset = 50;
        chunkMin = 100;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameObject.Find("PlayerData").GetComponent<OptionsData>().disableBg)
            speed = 0;
        else speed = 2;

        ceiling = playerCam.transform.position.y + 50;
        floor = playerCam.transform.position.y - 50;
        pointsPivot.transform.position = new Vector3(playerCam.transform.position.x, playerCam.transform.position.y, 0);
        eastWall = playerCam.transform.position.x + 50;
        westWall = playerCam.transform.position.x - 50;

        MoveShapesUp();
    }

    private void MoveShapesUp()
    {
        for (int i = 0; i < shapesUp.Count; i++)
        {
            shapesUp[i].transform.position += new Vector3(0, 1, 0) * speed * Time.deltaTime; //move chunk up

            if (ceiling - shapesUp[i].transform.position.y > 100)//moving north
            {
                shapesUp[i].transform.position += new Vector3(0, 100, 0);
            }

            else if (shapesUp[i].transform.position.y - floor > chunkMin)//moving south
            {
                shapesUp[i].transform.position += new Vector3(0, -100, 0);

                if (shapesUp[i].GetComponent<BlockChunk>().columnNum < 4)
                {
                    shapesUp[i].transform.position += new Vector3(-20, 0, 0);
                    shapesUp[i].GetComponent<BlockChunk>().columnNum += 1;
                }
                else
                {
                    shapesUp[i].transform.position += new Vector3(80, 0, 0);
                    shapesUp[i].GetComponent<BlockChunk>().columnNum = 0;
                }
            }
            
            else if (eastWall - shapesUp[i].transform.position.x > chunkMin)//moving east
            {                               
                for (int j = 0; j < shapesUp.Count; j++)
                {
                    if (shapesUp[j] != shapesUp[i])
                        if (shapesUp[j].position.y == shapesUp[i].position.y)
                            ShiftColumnNum(shapesUp[j], 1);
                }
                ShiftColumnNum(shapesUp[i], 1);
                shapesUp[i].transform.position += new Vector3(100, 0, 0);
            }

            else if (shapesUp[i].transform.position.x - westWall > chunkMin)//moving west
            {                             
                for (int j = 0; j < shapesUp.Count; j++)
                {
                    if (shapesUp[j] != shapesUp[i])
                        if (shapesUp[j].position.y == shapesUp[i].position.y)
                            ShiftColumnNum(shapesUp[j], -1);
                }
                ShiftColumnNum(shapesUp[i], -1);
                shapesUp[i].transform.position += new Vector3(-100, 0, 0);
            }            
        }
    }

    private void ShiftColumnNum(Transform chunk, int temp)
    {
        if (temp == 1)//east
        {
            if (chunk.GetComponent<BlockChunk>().columnNum < 4)               
                chunk.GetComponent<BlockChunk>().columnNum += 1;
            else
                chunk.GetComponent<BlockChunk>().columnNum = 0;

        }
        if (temp == -1)//west
        {
            if (chunk.GetComponent<BlockChunk>().columnNum > 0)
                chunk.GetComponent<BlockChunk>().columnNum -= 1;
            else
                chunk.GetComponent<BlockChunk>().columnNum = 4;
        }
    }

    private Transform GetLocalPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        for (int i = 0;i < players.Length;i++)
        {
            if (players[i].GetComponent<NetworkObject>().IsLocalPlayer)
                return players[i].transform;
        }

        return null;
    }
}
