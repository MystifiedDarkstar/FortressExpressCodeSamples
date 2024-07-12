using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AISpawnController : MonoBehaviour
{
    [SerializeField] private GameObject m_UIControllerREF;
    [SerializeField] private GameObject m_EnemyPrefab;

    [SerializeField] private List<GameObject> m_CurrentSpawnPool;
    [SerializeField] private List<GameObject> m_FurtherSpawnPool;
    [SerializeField] private GameObject m_BrutePrefab;

    [SerializeField] private ObjectPool m_ObjectPool;

    [SerializeField] private List<GameObject> m_SpawnPointsTOP;
    [SerializeField] private List<GameObject> m_SpawnPointsBottom;

    [SerializeField] private GameObject m_ScrollBackground;
    [SerializeField] private GameObject m_ScrollTracks;

    private int m_spawnCount = 5; // Total Amount To Spawn
    private int m_countToSpawn = 5; // Current Amount Left To Spawn
    private int m_countToKill = 5; // Current Amount Left To Kill
    private int m_waveCount = 1; // Current Wave Number

    private float m_spawnDelay = 2.0f; // Time between Enemies Spawning

    private float m_waveDelayTimer = 5; // Current Time Between Waves
    private float m_maxWaveDelayTimer = 5; // Max Time Between Waves

    private bool m_isWaveBreak = true;
    private bool m_isGameOver = false;
    private bool m_IsStart = true;

    private float m_HealthModifier = 1.0f;
    private float m_MoveSpeedModifier = 1.0f;
    private float m_ScrollSpeed = 1.0f;

    public UnityEvent E_StartWaveEvent;

    private GameStats m_GameStats;


    public void AddSpawnTop(GameObject point) 
    {
        m_SpawnPointsTOP.Add(point);
    }
    public void AddSpawnBottom(GameObject point) 
    {
        m_SpawnPointsBottom.Add(point);
    }

    private void Awake()
    {
        m_GameStats = GameObject.FindObjectOfType<GameStats>();

        E_StartWaveEvent.AddListener(delegate { m_UIControllerREF.GetComponent<UIController>().UpdateWaveNumber(m_waveCount); });
        E_StartWaveEvent.AddListener(StartWave);

        m_ScrollBackground.GetComponent<ScrollingBackground>().SetScrollSpeed(m_ScrollSpeed);
        m_ScrollTracks.GetComponent<ScrollingBackground>().SetScrollSpeed(m_ScrollSpeed);
    }
    private void Update()
    {
        // if no enemies left to kill we can end the current wave.
        if (m_countToKill <= 0)
        {
            EndWave();
        }
        // if wave break is active start reducing the wave delay timer
        if (m_isWaveBreak && gameObject.activeInHierarchy)
        {
            m_waveDelayTimer -= Time.deltaTime;
            m_UIControllerREF.GetComponent<UIController>().UpdateNotificationUI("New Wave in " + (int)m_waveDelayTimer + " seconds.");

            if (m_IsStart)
                m_GameStats.StartLevelTimer();

            m_IsStart = false;
        }
        // if wave timer is up call start wave
        if (m_waveDelayTimer < 0 && !m_isGameOver)
        {
            m_UIControllerREF.GetComponent<UIController>().UpdateNotificationUI("");
            E_StartWaveEvent.Invoke();
        }
    }
    public void EndGame()
    {
        m_isGameOver = true;
        m_GameStats.StopLevelTimer();
    }

    private void StartWave()
    {
        // reset wave downtime delay 
        m_isWaveBreak = false;
        m_waveDelayTimer = m_maxWaveDelayTimer;

        if (m_waveCount % 4 == 0)
            StartCoroutine(SpawnBrutes());
        else
            StartCoroutine(SpawnEnemy());
    }

    private void EndWave()
    {
        m_isWaveBreak = true; // start counting down the delay timer between waves of 10 seconds

        m_GameStats.AddWaveSurvived();
        m_waveCount++; // increase the wave count number by 1
        
        if (m_waveCount % 5 == 0) 
            UpdateFiveRounds();

        if (m_waveCount % 5 == 0)
            AddNewEnemyToPool();

        // Set the number of enemys to spawn for the wave, the number left to kill, and the number left to spawn.
        m_spawnCount = Mathf.RoundToInt(m_spawnCount * 1.15f); // set the number of enemies to spawn to 15% more than previous wave. so 4 enemies becomes 5. roudn this to integer.
        m_countToSpawn = m_spawnCount; // set the amount left to spawn
        m_countToKill = m_spawnCount; // set the amount left for player to kill
    }

    private void AddNewEnemyToPool()
    {
        if (m_FurtherSpawnPool.Count > 0)
        {
            m_CurrentSpawnPool.Add(m_FurtherSpawnPool[0]);
            m_FurtherSpawnPool.RemoveAt(0);
        }
    }

    private void UpdateFiveRounds()
    {
        if (m_HealthModifier != 2.0f)
        {
            m_HealthModifier += 0.1f;
        }
        if (m_MoveSpeedModifier != 2.0f)
        {
            m_MoveSpeedModifier += 0.1f;
        }
        if (m_ScrollSpeed != 4.0f)
        {
            m_ScrollSpeed += 0.2f;
            m_ScrollBackground.GetComponent<ScrollingBackground>().SetScrollSpeed(m_ScrollSpeed);
            m_ScrollTracks.GetComponent<ScrollingBackground>().SetScrollSpeed(m_ScrollSpeed);
        }
        
    }

    IEnumerator SpawnEnemy()
    {
        // iterate code m_spawnCount times.
        for (int i = 0; i < m_spawnCount; i++)
        {

            if (!m_isGameOver)
            {
                GameObject Enemy = m_ObjectPool.GetObjectFromPool(SelectEnemyPrefab());
                Enemy.transform.position = FindSpawnLocation();
                Enemy.GetComponent<EnemyBase>().InitaliseEnemy(m_MoveSpeedModifier, m_HealthModifier);

                m_countToSpawn--;
            }

            yield return new WaitForSeconds(m_spawnDelay); //wait to cycle next iteration for specififed amount of time
        }
    }

    IEnumerator SpawnBrutes()
    {
        // iterate code m_spawnCount times.
        for (int i = 0; i < m_spawnCount; i++)
        {
            if (!m_isGameOver)
            {
                GameObject Enemy = m_ObjectPool.GetObjectFromPool(m_BrutePrefab);
                Enemy.transform.position = FindSpawnLocation();
                Enemy.GetComponent<EnemyBase>().InitaliseEnemy(m_MoveSpeedModifier, m_HealthModifier);

                m_countToSpawn--;
            }
            yield return new WaitForSeconds(m_spawnDelay); //wait to cycle next iteration for specififed amount of time
        }
    }

    private GameObject SelectEnemyPrefab()
    {
        int randInd = Random.Range(0, m_CurrentSpawnPool.Count);
        return m_CurrentSpawnPool[randInd];
    }

    private Vector3 FindSpawnLocation()
    {
        Vector3 SpawnPosition = new Vector3(); // create variable instance to store vector locally to return

        int SpawnPointIndex = Random.Range(0, m_SpawnPointsTOP.Count);

        int TopBottomIndex = Random.Range(1, 3);

        if (TopBottomIndex == 1) // its spawning at the top
        { 
            SpawnPosition = m_SpawnPointsTOP[SpawnPointIndex].transform.position;
        }
        else if (TopBottomIndex == 2) 
        {
            SpawnPosition = m_SpawnPointsBottom[SpawnPointIndex].transform.position;
        }
        else
        {
            Debug.LogError("Invalid TopBottomIndex. Value:" + TopBottomIndex);
        }
        return SpawnPosition;
    }

    public void MinusCountToKill()
    {
        // enemy died reduce count to kill by 1 everytime this is called by an enemy death event.
        m_countToKill--;
        m_GameStats.AddEnemyKilled();
    }
}
