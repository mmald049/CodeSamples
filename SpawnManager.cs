using UnityEngine;
using System.Collections;

//Spawn squares with random x positions at the top of the screen
public class SpawnManager : MonoBehaviour
{
    public GameObject obstacle;

    public GameObject[] allObstacles;

    public int obstaclesToInstantiate;

    public float obstacleSpeed;
    public float maxObstacles;

    public float yCeiling;
    public float yFloor;

    public float spawnTime;

    public bool canSpawn;

    public Color[] colors;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        allObstacles = new GameObject[obstaclesToInstantiate];

        InstantiateObstacles();

        canSpawn = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (canSpawn)
            SpawnObstacles();
    }

    void InstantiateObstacles()
    {
        for (int i = 0; i < obstaclesToInstantiate; i++)
        {
            allObstacles[i] = Instantiate(obstacle, new Vector3(0, yCeiling, 0), Quaternion.identity);
        }
    }

    void SpawnObstacles()
    {
        if (CheckActiveObstacles() < maxObstacles)
        {
            for (int i = 0; i < allObstacles.Length; i++)
            {
                Obstacle ob = allObstacles[i].GetComponent<Obstacle>();

                if (!ob.canMove)
                {
                    canSpawn = false;
                    StartCoroutine(SpawnTimer());
                    ob.canMove = true;
                    int xPos = Random.Range(-6, 7) * 2;
                    ob.transform.position = new Vector2(xPos, yCeiling);

                    int rndColor = Random.Range(0, 4);
                    ob.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().color = colors[rndColor];

                    break;
                }
            }
        }
    }

    int CheckActiveObstacles()
    {
        int temp = 0;

        for (int i = 0; i < allObstacles.Length; i++)
        {
            if (allObstacles[i].GetComponent<Obstacle>().canMove)
                temp++;
        }

        return temp;
    }

    IEnumerator SpawnTimer()
    {
        yield return new WaitForSeconds(spawnTime);
        canSpawn = true;
    }
}
