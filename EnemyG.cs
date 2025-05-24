using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

//Script for The Deranged boss that includes a Simon Says memory puzzle in which there are no repeating consecutive colors
public class EnemyG : NetworkBehaviour
{
    public EnemyInfo info;

    public List<Transform> colorPoints;

    public int[] order;
    public GameObject[] players;

    public Vector3 startPos;

    public GameObject chaser;
    public ColorChaser[] chasers;

    public bool canSpawn;
    public bool canColorSwap;
    public bool bossJustStarted;
    private bool firstStart;
    private bool pauseSpawn;

    public int maxChasers;
    public float spawnTime;
    public float colorTime;
    public float moveTime;
    public float chaserSpeed;
    public float chaserAtkSpeed;
    private int maxColors;
    private float delay;

    private Coroutine colorSwapCurrent;
    private Coroutine spawnTimerCurrent;
    private Coroutine colorTimerCurrent;

    void Start()
    {
        info = transform.GetComponent<EnemyInfo>();
        firstStart = true;
        delay = 0.5f;
    }

    void Update()
    {
        if (!IsServer)
            return;

        if (info.aggro.Value)
        {
            players = GameObject.FindGameObjectsWithTag("Player");

            Phases();

            if (firstStart)
            {
                firstStart = false;
                maxColors = info.data.difficulty.Value + 4;
                order = new int[maxColors];
                startPos = transform.position;
                spawnTimerCurrent = StartCoroutine(SpawnTimer(2));
            }

            if (bossJustStarted)
            {                
                bossJustStarted = false;
                delay = 0.5f;
                StartCoroutine(ColorTimer());
            }

            if (canSpawn && !pauseSpawn)
            {
                canSpawn = false;
                SpawnChasers();
                spawnTimerCurrent = StartCoroutine(SpawnTimer(spawnTime));
            }

            if (canColorSwap)
            {
                canColorSwap = false;
                colorSwapCurrent = StartCoroutine(ColorSwap());
            }
        }
        else if (info.hasBeenAggro)
        { 
            if (colorSwapCurrent != null)
                StopCoroutine(colorSwapCurrent);
            if (spawnTimerCurrent != null)
                StopCoroutine(spawnTimerCurrent);
            if (colorTimerCurrent != null)
                StopCoroutine(colorTimerCurrent);
            canColorSwap = false;
            canSpawn = false;
            firstStart = true;
            bossJustStarted = true;
            transform.position = startPos;
            pauseSpawn = false;
        }
    }

    private void SpawnChasers()
    {
        if (GetChaserNum() < maxChasers )
        {            
            SpawnChaser();           
        }
    }

    private int GetChaserNum()//Amount of chasers active
    {
        GameObject[] chasers = GameObject.FindGameObjectsWithTag("ColorChaser");

        int count = 0;

        if (chasers.Length == 0)
            return count;

        for (int i = 0; i < chasers.Length; i++)
        {
            if (chasers[i].GetComponent<ColorChaser>().netActive.Value)
                count++;
        }
        return count;
    }

    private void SpawnChaser()
    {
        for (int i = 0; i < colorPoints.Count; i++)
        {
            ColorChaser cc = GetIdleChaser();

            cc.transform.position = colorPoints[i].position;
            cc.GetComponent<BulletSpawn>().canAttack = true;
            cc.netActive.Value = true;
            cc.damageN.Value = info.damage;
            cc.rangeN.Value = info.range2;
            cc.speedN.Value = chaserSpeed;
            cc.attackSpeed = chaserAtkSpeed;
            cc.hp.Value = 2 * info.TotalPlayersSum();
            cc.bossId.Value = 1;
        }
    }

    private ColorChaser GetIdleChaser()
    {
        ColorChaser temp = null;

        for (int i = 0; i < chasers.Length; i++)
        {
            if (!chasers[i].netActive.Value)
            {
                temp = chasers[i];
                break;
            }               
        }
        return temp;
    }

    private int[] GetArray()//fill the array with random numbers
    {
        int[] x = new int[maxColors];//create temp array

        for (int i = 0; i < x.Length; i++)
        {
            if (i == 0)//set first color
            {
                x[i] = Random.Range(0, colorPoints.Count);
            }
            else if (i != 0)//set rest of colors
            {
                int z = x[i - 1];

                x[i] = GetNum(z);//pass previous color 0 1 2 or 3
            }
        }      
        return x;
    }

    private int GetNum(int num)//get a random number
    {
        int temp = Random.Range(0, 2);

        if (num == 1 || num == 2)
        {
            if (temp == 0)
                return num - 1;
            else return num + 1;
        }
        else if (num == 3)
        {
            if (temp == 0)
                return 0;
            else return 2;
        }
        else if (num == 0)
        {
            if (temp == 0)
                return 1;
            else return 3;
        }

        return 0;
    }

    private void Phases()
    {
        info.bodyDamage = info.damage;
        float spawnM = 1;
        float colorM = 1;
        float moveM = 1;
        float atkSpdM = 1;

        if (info.data.difficulty.Value == 1)
        {
            spawnM = 1.5f;
            colorM = 1.5f;
            moveM = 1.5f;
            atkSpdM = 0.75f;
        }
        if (info.data.difficulty.Value == 2)
        {
            spawnM = 2f;
            colorM = 2f;
            moveM = 2f;
            atkSpdM = 0.5f;
        }

        if (info.netCurrentHp.Value <= info.netTotalHp.Value)
        {
            spawnTime = 11f / spawnM;
            colorTime = 4.5f / colorM;
            moveTime = 2.1f / moveM;
            chaserAtkSpeed = 2f * atkSpdM;
        }

        if (info.netCurrentHp.Value <= info.netTotalHp.Value * 2f / 3f)
        {
            spawnTime = 10f / spawnM;
            colorTime = 3f / colorM;
            moveTime = 1.8f / moveM;
            chaserAtkSpeed = 1.75f * atkSpdM;
        }

        if (info.netCurrentHp.Value <= info.netTotalHp.Value * 1f / 3f)
        {
            spawnTime = 9f / spawnM;
            colorTime = 1.5f / colorM;
            moveTime = 1.5f / moveM;
            chaserAtkSpeed = 1.5f * atkSpdM;
        }
    }

    IEnumerator ColorTimer()
    {
        yield return new WaitForSeconds(colorTime);
        canColorSwap = true;
    }

    IEnumerator SpawnTimer(float time)//spawn goons
    {
        yield return new WaitForSeconds(time);
        canSpawn = true;    
    }

    IEnumerator ColorSwap()
    {
        order = GetArray();

        for (int i = 0; i < order.Length; i++)//boss teleports
        {
            transform.position = colorPoints[order[i]].transform.position;

            yield return new WaitForSeconds(delay * 3);

            transform.position = startPos;

            yield return new WaitForSeconds(delay);
        }

        transform.position = startPos;

        yield return new WaitForSeconds(delay * 2);

        //pauseSpawn = true;

        for (int k = 0; k < order.Length; k++)//waves start
        {
            for (int i = 0; i < colorPoints.Count; i++)
            {
                if (order[k] != colorPoints[i].transform.GetComponent<ColorPoint>().num)
                {
                    SetColorActive(i, true);
                }
            }
            yield return new WaitForSeconds(delay * 3);

            for (int i = 0; i < colorPoints.Count; i++)
            {               
                if (order[k] != colorPoints[i].transform.GetComponent<ColorPoint>().num)
                {
                    SetColorActive(i, false);
                }
            }
            yield return new WaitForSeconds(moveTime);
        }
        //pauseSpawn = false;
        colorTimerCurrent = StartCoroutine(ColorTimer());
    }

    private void SetColorActive(int i, bool b)
    {
        if (b)
            colorPoints[i].transform.GetComponent<ColorPoint>().netActive.Value = true;
        else colorPoints[i].transform.GetComponent<ColorPoint>().netActive.Value = false;
    }
}
