using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

//Main handler for generating and managing raids
public class HandleRaid : NetworkBehaviour
{
    public BossData bossData;
    public TaskManager tasks;
    public FinalBoss finalBoss;

    public GameObject disconnectScreen;

    public ChoiceButton[] buttons;
    public List<Sprite> bossIcons;
    public List<GameObject> allRooms;
    public List<GameObject> roomSpawnsStandard;
    public List<GameObject> roomSpawnsCustom;
    public List<GameObject> gatesSpawn;
    public List<GameObject> stars;
    public List<Transform> spawnPoints;

    public List<int> bossIds = new List<int>(); //current bosses   
    public List<int> idPool = new List<int>(); //total bosses
    public List<int> finalBossIds = new List<int>(); //final boss layout

    public GameObject standardObjects;
    public GameObject customObjects;
    public GameObject spawnRoom;
    public GameObject starsPivot;
    public GameObject choiceButtonsPivot;
    public GameObject bottomWall;
    public GameObject starCount;

    public TextMeshProUGUI starCountText;
    public TextMeshProUGUI choiceButtonText;

    public int choice1;
    public int choice2;
    public int choice3;//rnd
    public int currentChoice;
    public int customRaidLength;

    public bool firstStart;
    public bool firstStart2;
    public bool canStart;
    public bool hasConnected;

    public bool needNewChoices;
    public bool needFinalRoom;
    public bool needMoveSpawn;

    public NetworkVariable<int> currentRoom = new NetworkVariable<int>();
    public NetworkVariable<int> customRaidLengthNet = new NetworkVariable<int>();
    public NetworkVariable<bool> choiceMade = new NetworkVariable<bool>();
    public NetworkVariable<bool> needNextRoom = new NetworkVariable<bool>();

    public NetworkVariable<bool> playerDied = new NetworkVariable<bool>();

    public NetworkVariable<bool> raidComplete = new NetworkVariable<bool>();

    public int taskRewardTokens;//shape tokens earned through tasks this raid

    // Start is called before the first frame update
    void Start()
    {
        firstStart = true;
        firstStart2 = true;
        needNewChoices = true;
        bossData = GameObject.Find("BossData").GetComponent<BossData>();
        tasks = GameObject.Find("TaskManager").GetComponent<TaskManager>();
        SetSpawnPoints();
    }

    // Update is called once per frame
    void Update()
    {
        if (NetworkManager.LocalClient != null)
        {
            hasConnected = true;
            if (needMoveSpawn)
            {
                needMoveSpawn = false;
                MoveSpawnRoom();
            }
            
            if (IsServer)
            {
                if (firstStart)
                {
                    firstStart = false;
                    needNextRoom.Value = true;
                }

                if (bossData.raidType.Value == 0)
                {
                    currentChoice = GetCurrentButton();
                    StandardRaid();
                }
                else
                {
                    CustomRaid();

                    customRaidLengthNet.Value = customRaidLength;
                }

            }
            else
            {
                if (bossData.raidType.Value == 0)
                {
                    standardObjects.SetActive(true);
                    choiceButtonText.gameObject.SetActive(true);
                    customObjects.SetActive(false);
                    starCountText.gameObject.SetActive(false);

                }
                else if (bossData.raidType.Value == 1)
                {
                    standardObjects.SetActive(false);
                    choiceButtonText.gameObject.SetActive(false);
                    customObjects.SetActive(true);
                    starCountText.gameObject.SetActive(true);
                }              
            }

            if (bossData.raidType.Value == 1)
                starCountText.text = currentRoom.Value + "/" + customRaidLengthNet.Value;
        }

        if (hasConnected)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            if (players.Length == 0)
                disconnectScreen.SetActive(true);
        }
    }

    private void CustomRaid()
    { 
        if (firstStart2 )
        {
            firstStart2 = false;

            standardObjects.SetActive(false);
            choiceButtonText.gameObject.SetActive(false);
            customObjects.SetActive(true);
            starCountText.gameObject.SetActive(true);
           
            if (bossData.customBosses.Count >= 1)
                for (int i = 0; i < bossData.customBosses.Count; i++)
                {
                    idPool.Add(bossData.customBosses[i]);
                }
            else for (int i = 0; i < bossData.customBossesRnd.Count; i++)
                 {
                    idPool.Add(bossData.customBossesRnd[i]);
                 }

            customRaidLength = idPool.Count;
            SpawnRoomsCustom();
        }
    }

    private void SpawnRoomsCustom()
    {
        for (int i = 0; i < customRaidLength; i++)//fill roomspawns
        {
            int rndNum = UnityEngine.Random.Range(0, idPool.Count);
            GameObject newRoom = allRooms[idPool[rndNum]];

            roomSpawnsCustom[i].GetComponent<RoomSpawn>().room = newRoom;
            roomSpawnsCustom[i].GetComponent<RoomSpawn>().active.Value = true;        

            idPool.Remove(idPool[rndNum]);
            newRoom.SetActive(true);
            newRoom.transform.position = roomSpawnsCustom[i].transform.position;
            newRoom.GetComponent<Room>().spawn = roomSpawnsCustom[i].GetComponent<RoomSpawn>();
            newRoom.GetComponent<Room>().canMove.Value = true;
            newRoom.GetComponent<Room>().spawnPos.Value = roomSpawnsCustom[i].transform.position;

            if (i == customRaidLength - 1)//end raid
                newRoom.GetComponent<Room>().finalBoss = true;
        }

        for (int i = 0; i < roomSpawnsCustom.Count - idPool.Count; i++)
        {
            roomSpawnsCustom[i + idPool.Count].gameObject.SetActive(false);
        }
    }

    private void StandardRaid()
    {  
        if (firstStart2)
        {
            idPool = bossData.defaultBosses;
            
            firstStart2 = false;

            standardObjects.SetActive(true);
            choiceButtonText.gameObject.SetActive(true);
            customObjects.SetActive(false);
            starCountText.gameObject.SetActive(false);
            finalBoss = GameObject.Find("FinalMarker").transform.parent.gameObject.GetComponent<FinalBoss>();
        }

        if (currentRoom.Value == 6)
        {
            finalBoss.SetAbilities();
            needNewChoices = false;
            currentRoom.Value++;
            needNextRoom.Value = false;
        }

        if (currentRoom.Value > 0 && currentRoom.Value < 7)
            stars[currentRoom.Value - 1].GetComponent<SetStarSprite>().active.Value = true;
        else if (currentRoom.Value == 7)
            stars[5].GetComponent<SetStarSprite>().active.Value = true;

        if (needNextRoom.Value && currentRoom.Value < 6)
        {
            if (needNewChoices)
            {
                needNewChoices = false;

                choice1 = GetChoice(-1, -1);
                choice2 = GetChoice(choice1, -1);
                choice3 = GetChoice(choice1, choice2);

                buttons[0].iconId.Value = choice1;
                buttons[1].iconId.Value = choice2;
                buttons[2].iconId.Value = choice3;
                buttons[0].active.Value = false;
                buttons[1].active.Value = false;
                buttons[2].active.Value = true;
            }

            if (choiceMade.Value)
            {
                needNextRoom.Value = false;
                needNewChoices = true;
                SetChoiceMadeServerRpc(false);

                if (currentChoice >= 0)//choose selected option
                {
                    allRooms[currentChoice].transform.position = roomSpawnsStandard[currentRoom.Value].transform.position;
                    allRooms[currentChoice].GetComponent<Room>().canMove.Value = true;
                    allRooms[currentChoice].GetComponent<Room>().spawnPos.Value = roomSpawnsStandard[currentRoom.Value].transform.position;
                }                   
                else//choose random option
                {
                    allRooms[choice3].transform.position = roomSpawnsStandard[currentRoom.Value].transform.position;
                    currentChoice = buttons[2].iconId.Value;
                    allRooms[choice3].GetComponent<Room>().canMove.Value = true;
                    allRooms[choice3].GetComponent<Room>().spawnPos.Value = roomSpawnsStandard[currentRoom.Value].transform.position;
                }
                bossIds[currentChoice] = -3;               
                finalBossIds.Add(currentChoice);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetChoiceMadeServerRpc(bool b)
    {
        choiceMade.Value = b;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetRaidCompleteServerRpc(bool b)
    {
        raidComplete.Value = b;
    }

    private int GetChoice(int temp, int temp2)
    {
        int rnd = Random.Range(0, bossIds.Count);

        if (bossIds[rnd] < 0)
        {
            return GetChoice(temp, temp2);
        }
        else if (rnd != temp && rnd != temp2)
        {
            return rnd;
        }
        else return GetChoice(temp, temp2);

        return rnd;
    }

    private int GetCurrentButton()
    {
        int temp = -2;

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].active.Value)
                return buttons[i].iconId.Value;
        }

        return temp;
    }

    private void MoveSpawnRoom()
    {
        if (bossData.raidType.Value == 1 && currentRoom.Value == customRaidLength)
            return;

        if (IsServer)
        {
            spawnRoom.transform.position += new Vector3(0, 80, 0);
            starsPivot.transform.position += new Vector3(0, 80, 0);
            choiceButtonsPivot.transform.position += new Vector3(0, 80, 0);
            starCount.transform.position += new Vector3(0, 80, 0);

            if (currentRoom.Value == 1)
                bottomWall.transform.position += new Vector3(0, 40, 0);
            else bottomWall.transform.position += new Vector3(0, 80, 0);

            MovePlayers();
        }
    }

    private void SetSpawnPoints()
    {
        if (spawnPoints.Count > 0)
            spawnPoints.Clear();

        for (int i = 0; i < spawnRoom.transform.GetChild(2).childCount; i++)
        {
            spawnPoints.Add(spawnRoom.transform.GetChild(2).GetChild(i).transform);
        }
    }

    private void MovePlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        for (int i = 0; i < players.Length; i++)//teleport players
        {
            PlayerInfo infoTemp = players[i].GetComponent<PlayerInfo>();

            if (players[i].transform.position.y < bottomWall.transform.position.y)
            {
                infoTemp.ServerMovePlayerRpc(infoTemp.spawnPoints[infoTemp.playerId.Value].position);
                players[i].transform.position = infoTemp.spawnPoints[infoTemp.playerId.Value].position;
            }
        }
    }
}