using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    [Header("Towers")]
    public GameObject[] PossibleTowerPositions;
    public GameObject TowerCircle;
    public GameObject Tower;
    public LayerMask ClickTargetLayer;
    public AudioClip SplashSound;

    [Header("Scoring")]
    public int StartCredits;
    public int NexusStrength;
    public int TowerCosts;
    public int KillPlayerReward;
    public int WaveReward;
    public int PlayerHitsNexus;

    [Header("UI")]
    public Text CreditText;
    public Image CreditBar;
    public Text LevelText;
    public Image LevelBar;
    public Text NexusHealthText;

    [Header("Enemy Waves")]
    public GameObject Enemy;
    public Transform SpawnPoint;
    public int EnemiesPerWave = 7;
    public float InitialSpawnDelay = 5f;
    public float CycleTime = 30f;
    public int CurrentEnemies;
    public int WaveLevel;

    [Header("Game Over Display")]
    public Transform Nexus;
    public GameObject NexusExplosion;
    public AudioClip ExplosionSound;
    public GameObject GameOverDisplay;


    public enum Incident { TowerPlaced, PlayerReachedNexus, PlayerKilled, NewWave }

    private readonly List<GameObject> usableTiles = new List<GameObject>();
    private bool towerTilesShown;
    private AudioSource audioSource;
    private float currentTime;
    private bool timerRunning;
    private float nexusHealth;
    private int remainingCredits;

    void Start()
    {
        remainingCredits = StartCredits;
        nexusHealth = NexusStrength;
        audioSource = gameObject.AddComponent<AudioSource>();
        currentTime = CycleTime;

        GameOverDisplay.SetActive(false);

        // Paint green circles
        foreach (GameObject possibleTowerPositions in PossibleTowerPositions)
        {
            GameObject circle = Instantiate(TowerCircle,
                possibleTowerPositions.transform.position + new Vector3(0, 0.3f, 0),
                Quaternion.identity);
            usableTiles.Add(circle);
        }

        StartCoroutine(SpawnWaves());
    }


    void Update()
    {
        TowerPlacement();
        UiUpdate();
        SpawnTimerUpdate();
    }


    private void UiUpdate()
    {
        CreditText.text = "Credits " + remainingCredits;
        LevelText.text = "Level " + WaveLevel;
        NexusHealthText.text = "Nexus " + nexusHealth;

        CreditBar.fillAmount = remainingCredits / (float)TowerCosts;
        LevelBar.fillAmount = currentTime / CycleTime;
    }

    private void SpawnTimerUpdate()
    {
        if (timerRunning && currentTime > 0)
            currentTime -= Time.deltaTime;
    }

    private void TowerPlacement()
    {
        ShowTowerTiles(remainingCredits > TowerCosts);

        if (remainingCredits < TowerCosts) return;
        if (!Input.GetButtonDown("Fire1")) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000, ClickTargetLayer))
        {
            GameObject tile = hit.collider.gameObject;
            if (usableTiles.Contains(tile))
                PlaceTower(tile);
        }
    }
    
    private void PlaceTower(GameObject tile)
    {
        usableTiles.Remove(tile);
        Instantiate(Tower, tile.transform.position, Quaternion.identity);
        tile.GetComponent<LandingPoint>().Splash();
        audioSource.clip = SplashSound;
        audioSource.Play();

        for(int g = usableTiles.Count - 1; g >= 0; g--)
        {
            GameObject go = usableTiles[g];
            if (Vector3.Distance(go.transform.position, tile.transform.position) < 5)
            {
                go.SetActive(false);
                usableTiles.RemoveAt(g);
            }
        }

        ReportIncident(Incident.TowerPlaced);
    }

    private void ShowTowerTiles(bool show)
    {
        if(show && !towerTilesShown)
        {
            foreach (GameObject tile in usableTiles)
                tile.SetActive(true);
            towerTilesShown = true;
        }

        if(!show && towerTilesShown)
        {
            foreach (GameObject tile in usableTiles)
            {
                tile.SetActive(false);
                towerTilesShown = false;
            }
        }
    }
    
    private IEnumerator SpawnWaves()
    {
        yield return new WaitForSeconds(InitialSpawnDelay);
        timerRunning = true;

        while (timerRunning)
        {
            for (int e = 0; e < EnemiesPerWave; e++)
            {
                Instantiate(Enemy, SpawnPoint.position, SpawnPoint.rotation);
                CurrentEnemies++;
                float deltaSpawnTime = UnityEngine.Random.Range(2f, 5f);
                yield return new WaitForSeconds(deltaSpawnTime);

            }

            yield return new WaitForSeconds(currentTime);
            currentTime = CycleTime;
            WaveLevel++;

            ReportIncident(Incident.NewWave);
            remainingCredits += WaveLevel * 10;

        }
    }

    public void ReportIncident(Incident incident)
    {
        switch (incident)
        {
            case Incident.TowerPlaced:
                remainingCredits -= TowerCosts;
                break;
            case Incident.PlayerReachedNexus:
                nexusHealth -= PlayerHitsNexus;
                if (nexusHealth < 0)
                {
                    GameOver();
                }
                break;
            case Incident.PlayerKilled:
                remainingCredits += KillPlayerReward;
                break;
            case Incident.NewWave:
                remainingCredits += WaveLevel * 10;
                break;
            default:
                break;
        }
    }

    private void GameOver()
    {
        Instantiate(NexusExplosion, Nexus.position, Quaternion.identity);
        audioSource.PlayOneShot(ExplosionSound);
        Destroy(Nexus.gameObject);
        Invoke("ShowGameOverScreen", 3f);
    }

    private void ShowGameOverScreen()
    {
        GameOverDisplay.SetActive(true);
        Time.timeScale = 0;
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene(1);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
