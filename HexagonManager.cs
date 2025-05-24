using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Create a random field of hexagons that can be filled in by player colors
public class HexagonManager : MonoBehaviour
{
    public GameObject[] hexPoints;
    public GameObject[] awakePoints;
    public bool needPoints;
    public bool needReset;
    public int maxPoints;
    public int totalAwakePoints;

    // Start is called before the first frame update
    void Start()
    {
        hexPoints = GameObject.FindGameObjectsWithTag("HexPoint");
    }

    // Update is called once per frame
    void Update()
    {
        SetTotalAwakePoints();
        SetNeedPoints();
        FillAwakePointsArray();
        SetMap();

        if (needReset)
        {
            ResetMap();
            needReset = false;
        }
    }

    private void SetNeedPoints()
    {
        if (totalAwakePoints >= maxPoints)
            needPoints = false;
    }

    private void SetTotalAwakePoints()
    {
        int count = 0;

        for (int i = 0; i < hexPoints.Length; i++)
        {
            if (hexPoints[i].GetComponent<HexPoint>().awake == true)
                count++;
        }
        totalAwakePoints = count;
    }

    private void FillAwakePointsArray()
    {
        awakePoints = new GameObject[totalAwakePoints];

        for (int i = 0; i < awakePoints.Length; i++)
        {
            for (int j = 0; j < hexPoints.Length; j++)
            {
                bool pointExists = false;

                for (int k = 0; k < awakePoints.Length; k++) 
                {
                    if (hexPoints[j] == awakePoints[k])
                        pointExists = true;
                }

                if (hexPoints[j].GetComponent<HexPoint>().awake && !pointExists)
                {
                    awakePoints[i] = hexPoints[j];
                    break;
                }
            }
        }
    }

    private void SetMap()
    {
        if (needPoints)
        {
            int randomHex = Random.Range(0, awakePoints.Length);
            
            for (int i = 0; i < awakePoints.Length;i++)
            {
                if (i == randomHex)
                {
                    int randomAdj = Random.Range(0, awakePoints[i].GetComponent<HexPoint>().nonAwakeAdjPoints.Length);

                    for (int j = 0; j < awakePoints[i].GetComponent<HexPoint>().nonAwakeAdjPoints.Length; j++)
                    {
                        if (j == randomAdj)
                        {
                            awakePoints[i].GetComponent<HexPoint>().nonAwakeAdjPoints[j].GetComponent<HexPoint>().awake = true;
                        }
                    }
                }
            }
        }
    }

    private void ResetMap()
    {
        for (int i = 0; i < hexPoints.Length;i++)
        {
            if (!hexPoints[i].GetComponent<HexPoint>().centerPoint)
                hexPoints[i].GetComponent<HexPoint>().awake = false;
        }

        needPoints = true;
    }
}
