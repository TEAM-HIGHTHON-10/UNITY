using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RockDodgeGameManager : MonoBehaviour
{
    [Header("Entry")]
    [SerializeField] private int entryEggCost = 10;
    [SerializeField] private Button enterGameButton;
    [SerializeField] private GameObject enterGameAvailableChild;

    [Header("Panels / Background")]
    [SerializeField] private GameObject uiBg;
    [SerializeField] private GameObject inGameBg;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Texts")]
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text gameOverScoreText;
    [SerializeField] private TMP_Text gameOverRewardGoldenEggText;

    [Header("Character Sprite (Grade / Hat / Dead)")]
    [SerializeField] private SpriteRenderer inGameCharacterSpriteRenderer;
    [SerializeField] private Sprite[] aliveGradeSprites = new Sprite[4];
    [SerializeField] private Sprite[] aliveHatGradeSprites = new Sprite[4];
    [SerializeField] private Sprite[] deadGradeSprites = new Sprite[4];
    [SerializeField] private Sprite[] deadHatGradeSprites = new Sprite[4];

    [Header("Spawning")]
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private Transform rockSpawnerParent;
    [SerializeField] private Transform spawnArea;
    [SerializeField] private Vector2 fallbackSpawnSize = new Vector2(8f, 1f);
    [SerializeField] private float spawnInterval = 0.6f;
    [SerializeField] private Transform characterTransform;
    [SerializeField] private float rockDespawnY = -10f;

    private bool isPlaying;
    private bool isGameOver;
    private int score;
    private Coroutine countdownRoutine;
    private Coroutine spawnRoutine;

    private void Start()
    {
        SetActiveSafe(uiBg, true);
        SetActiveSafe(inGameBg, false);
        SetActiveSafe(gameOverPanel, false);

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        RefreshScoreTexts();
        RefreshRewardText();
        RefreshEnterButtonState();
    }

    private void Update()
    {
        RefreshEnterButtonState();
    }

    public void OnClickEnterRockGame()
    {
        if (isPlaying || GameManager.I == null)
        {
            return;
        }

        if (!GameManager.I.TryUseEggs(entryEggCost))
        {
            RefreshEnterButtonState();
            return;
        }

        StartGameSession();
    }

    public void OnRockPassedCharacter()
    {
        if (!isPlaying || isGameOver)
        {
            return;
        }

        score += 1;
        RefreshScoreTexts();
    }

    public void OnRockHitCharacter()
    {
        if (!isPlaying || isGameOver)
        {
            return;
        }

        isGameOver = true;

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        SetActiveSafe(gameOverPanel, true);
        ApplyCharacterSprite(true);
        RefreshScoreTexts();
        RefreshRewardText();
    }

    public void OnClickConfirmGameOver()
    {
        int rewardGoldenEgg = GetRewardGoldenEggAmount();
        if (GameManager.I != null && rewardGoldenEgg > 0)
        {
            GameManager.I.AddGoldenEggs(rewardGoldenEgg);
        }

        EndGameSession();
    }

    private void StartGameSession()
    {
        isPlaying = true;
        isGameOver = false;
        score = 0;

        ClearSpawnedRocks();
        RefreshScoreTexts();
        RefreshRewardText();
        ApplyCharacterSprite(false);

        SetActiveSafe(uiBg, false);
        SetActiveSafe(inGameBg, true);
        SetActiveSafe(gameOverPanel, false);

        if (countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
        }

        countdownRoutine = StartCoroutine(CoCountdownAndStart());
    }

    private void EndGameSession()
    {
        isPlaying = false;
        isGameOver = false;
        score = 0;

        if (countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
            countdownRoutine = null;
        }

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        ClearSpawnedRocks();

        SetActiveSafe(uiBg, true);
        SetActiveSafe(inGameBg, false);
        SetActiveSafe(gameOverPanel, false);

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        RefreshScoreTexts();
        RefreshRewardText();
        RefreshEnterButtonState();
    }

    private IEnumerator CoCountdownAndStart()
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
        }

        for (int remain = 3; remain >= 1; remain--)
        {
            if (countdownText != null)
            {
                countdownText.text = remain.ToString();
            }

            yield return new WaitForSeconds(1f);
        }

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        spawnRoutine = StartCoroutine(CoSpawnRocks());
        countdownRoutine = null;
    }

    private IEnumerator CoSpawnRocks()
    {
        while (isPlaying && !isGameOver)
        {
            SpawnRock();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnRock()
    {
        if (rockPrefab == null)
        {
            return;
        }

        Vector3 spawnPos = GetRandomSpawnPosition();
        Transform parent = rockSpawnerParent != null ? rockSpawnerParent : transform;
        GameObject spawned = Instantiate(rockPrefab, spawnPos, Quaternion.identity, parent);

        RockFallingObject rock = spawned.GetComponent<RockFallingObject>();
        if (rock == null)
        {
            rock = spawned.AddComponent<RockFallingObject>();
        }

        rock.Initialize(this, characterTransform, rockDespawnY);
    }

    private Vector3 GetRandomSpawnPosition()
    {
        if (spawnArea == null)
        {
            return transform.position;
        }

        Bounds bounds = new Bounds(spawnArea.position, new Vector3(fallbackSpawnSize.x, fallbackSpawnSize.y, 0f));

        Collider2D areaCollider = spawnArea.GetComponent<Collider2D>();
        if (areaCollider != null)
        {
            bounds = areaCollider.bounds;
        }
        else
        {
            Renderer areaRenderer = spawnArea.GetComponent<Renderer>();
            if (areaRenderer != null)
            {
                bounds = areaRenderer.bounds;
            }
        }

        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float spawnY = bounds.max.y;
        return new Vector3(randomX, spawnY, spawnArea.position.z);
    }

    private void ClearSpawnedRocks()
    {
        if (rockSpawnerParent == null)
        {
            return;
        }

        for (int i = rockSpawnerParent.childCount - 1; i >= 0; i--)
        {
            Destroy(rockSpawnerParent.GetChild(i).gameObject);
        }
    }

    private void RefreshEnterButtonState()
    {
        bool canEnter = !isPlaying && GameManager.I != null && GameManager.I.EggCount >= entryEggCost;

        if (enterGameAvailableChild != null)
        {
            enterGameAvailableChild.SetActive(canEnter);
        }

        if (enterGameButton != null)
        {
            enterGameButton.interactable = canEnter;
        }
    }

    private void RefreshScoreTexts()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }

        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = score.ToString();
        }
    }

    private int GetRewardGoldenEggAmount()
    {
        return score / 2;
    }

    private void RefreshRewardText()
    {
        if (gameOverRewardGoldenEggText != null)
        {
            gameOverRewardGoldenEggText.text = GetRewardGoldenEggAmount().ToString();
        }
    }

    private void ApplyCharacterSprite(bool isDead)
    {
        if (inGameCharacterSpriteRenderer == null || GameManager.I == null)
        {
            return;
        }

        int gradeIndex = (int)GameManager.I.CurrentGrade;
        bool isHatEquipped = GameManager.I.IsHatEquipped;

        Sprite selectedSprite = null;
        Sprite[] preferredSprites = isDead
            ? (isHatEquipped ? deadHatGradeSprites : deadGradeSprites)
            : (isHatEquipped ? aliveHatGradeSprites : aliveGradeSprites);

        if (preferredSprites != null && gradeIndex >= 0 && gradeIndex < preferredSprites.Length)
        {
            selectedSprite = preferredSprites[gradeIndex];
        }

        if (selectedSprite == null)
        {
            Sprite[] fallbackSprites = isHatEquipped ? aliveHatGradeSprites : aliveGradeSprites;
            if (fallbackSprites != null && gradeIndex >= 0 && gradeIndex < fallbackSprites.Length)
            {
                selectedSprite = fallbackSprites[gradeIndex];
            }
        }

        if (selectedSprite != null)
        {
            inGameCharacterSpriteRenderer.sprite = selectedSprite;
        }
    }

    private static void SetActiveSafe(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }
}