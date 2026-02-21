using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum CharacterGrade
    {
        Newbie = 0,
        Junior = 1,
        Mid = 2,
        Senior = 3
    }

    public static GameManager I;

    [Header("Currency")]
    [SerializeField] private int eggCount;
    [SerializeField] private int goldenEggCount;

    [Header("Level")]
    [SerializeField] private CharacterGrade currentGrade = CharacterGrade.Newbie;
    [SerializeField] private int currentGradeExp;
    [SerializeField] private int expPerLevelUp = 10;
    [SerializeField] private int eggsPerLevelUp = 10;

    [Header("Grade Sprites (Newbie -> Senior)")]
    [SerializeField] private Sprite[] gradeSprites = new Sprite[4];
    [SerializeField] private Sprite[] hatEquippedGradeSprites = new Sprite[4];
    [SerializeField] private Image characterImage;

    [Header("Equipment")]
    [SerializeField] private bool isHatEquipped;

    [Header("Level Up UI")]
    [SerializeField] private Button levelUpButton;
    [SerializeField] private Button feedButton;
    [SerializeField] private GameObject levelUpAvailableChild;

    [Header("Currency UI (TMP)")]
    [SerializeField] private TMP_Text eggCountText;
    [SerializeField] private TMP_Text goldenEggCountText;

    [Header("Grade UI (TMP)")]
    [SerializeField] private TMP_Text gradeText;
    [SerializeField] private TMP_Text expProgressText;
    [SerializeField] private Image nextGradeProgressFillImage;

    public int EggCount => eggCount;
    public int GoldenEggCount => goldenEggCount;
    public CharacterGrade CurrentGrade => currentGrade;
    public int CurrentGradeExp => currentGradeExp;

    private void Awake()
    {
        if (I != null)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        RefreshCharacterAndGradeUI();
        RefreshLevelUpState();
        RefreshCurrencyTexts();
        RefreshExpProgressText();
    }


    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            AddEggs(5);
        }

        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            AddGoldenEggs(5);
        }
    }

    public void OnClickAddEggs()
    {
        AddEggs(5);
    }

    public void OnClickAddGoldenEggs()
    {
        AddGoldenEggs(5);
    }

    public void AddEggs(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        eggCount += amount;
        RefreshCurrencyTexts();
        RefreshLevelUpState();
    }

    public bool TryUseEggs(int amount)
    {
        if (amount <= 0 || eggCount < amount)
        {
            return false;
        }

        eggCount -= amount;
        RefreshCurrencyTexts();
        RefreshLevelUpState();
        return true;
    }

    public void AddGoldenEggs(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        goldenEggCount += amount;
        RefreshCurrencyTexts();
    }

    public bool TryUseGoldenEggs(int amount)
    {
        if (amount <= 0 || goldenEggCount < amount)
        {
            return false;
        }

        goldenEggCount -= amount;
        RefreshCurrencyTexts();
        return true;
    }

    private void RefreshCurrencyTexts()
    {
        if (eggCountText != null)
        {
            eggCountText.text = eggCount.ToString();
        }

        if (goldenEggCountText != null)
        {
            goldenEggCountText.text = goldenEggCount.ToString();
        }
    }

    public bool CanLevelUp()
    {
        return !IsMaxLevelReached() && eggCount >= eggsPerLevelUp;
    }

    public void OnClickLevelUp()
    {
        TryLevelUp();
    }

    // 밥 주기 버튼에서도 레벨업 로직을 동일하게 사용
    public void OnClickFeed()
    {
        TryLevelUp();
    }

    // Inspector에서 이름이 다를 수 있어 별도 별칭 제공
    public void OnClickFeedButton()
    {
        TryLevelUp();
    }

    public bool TryLevelUp()
    {
        if (IsMaxLevelReached())
        {
            RefreshLevelUpState();
            return false;
        }

        if (!TryUseEggs(eggsPerLevelUp))
        {
            return false;
        }

        AddExperience(expPerLevelUp);
        return true;
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentGradeExp += amount;
        TryPromoteGrade();
        RefreshGradeProgressUI();
        RefreshExpProgressText();
    }

    private void TryPromoteGrade()
    {
        while (currentGrade != CharacterGrade.Senior && currentGradeExp >= GetRequiredExpForNextGrade(currentGrade))
        {
            int requiredExp = GetRequiredExpForNextGrade(currentGrade);
            currentGradeExp -= requiredExp;
            currentGrade += 1;
            RefreshCharacterAndGradeUI();
        }
    }

    private int GetRequiredExpForNextGrade(CharacterGrade grade)
    {
        switch (grade)
        {
            case CharacterGrade.Newbie:
                return 2500;
            case CharacterGrade.Junior:
                return 5000;
            case CharacterGrade.Mid:
                return 7500;
            default:
                return int.MaxValue;
        }
    }


    public void SetHatEquipped(bool equipped)
    {
        isHatEquipped = equipped;
        RefreshCharacterAndGradeUI();
    }

    private void RefreshCharacterAndGradeUI()
    {
        int spriteIndex = (int)currentGrade;
        if (characterImage != null)
        {
            Sprite selectedSprite = null;

            if (isHatEquipped && hatEquippedGradeSprites != null && spriteIndex >= 0 && spriteIndex < hatEquippedGradeSprites.Length)
            {
                selectedSprite = hatEquippedGradeSprites[spriteIndex];
            }

            if (selectedSprite == null && gradeSprites != null && spriteIndex >= 0 && spriteIndex < gradeSprites.Length)
            {
                selectedSprite = gradeSprites[spriteIndex];
            }

            if (selectedSprite != null)
            {
                characterImage.sprite = selectedSprite;
            }
        }

        if (gradeText != null)
        {
            gradeText.text = currentGrade.ToString();
        }

        RefreshGradeProgressUI();
        RefreshExpProgressText();
    }

    private void RefreshGradeProgressUI()
    {
        if (nextGradeProgressFillImage == null)
        {
            return;
        }

        if (currentGrade == CharacterGrade.Senior)
        {
            nextGradeProgressFillImage.fillAmount = 1f;
            return;
        }

        int requiredExp = GetRequiredExpForNextGrade(currentGrade);
        float progress = requiredExp <= 0 ? 0f : (float)currentGradeExp / requiredExp;
        nextGradeProgressFillImage.fillAmount = Mathf.Clamp01(progress);
    }

    private void RefreshExpProgressText()
    {
        if (expProgressText == null)
        {
            return;
        }

        int maxExp = currentGrade == CharacterGrade.Senior
            ? GetRequiredExpForNextGrade(CharacterGrade.Mid)
            : GetRequiredExpForNextGrade(currentGrade);

        int shownCurrentExp = currentGrade == CharacterGrade.Senior
            ? maxExp
            : currentGradeExp;

        expProgressText.text = $"{shownCurrentExp}/{maxExp}";
    }

    private void RefreshLevelUpState()
    {
        bool canLevelUp = CanLevelUp();
        bool canFeed = !IsMaxLevelReached() && canLevelUp;

        if (levelUpAvailableChild != null)
        {
            levelUpAvailableChild.SetActive(canLevelUp);
        }

        if (levelUpButton != null)
        {
            levelUpButton.interactable = canLevelUp;
        }

        if (feedButton != null)
        {
            feedButton.interactable = canFeed;
        }
    }

    private bool IsMaxLevelReached()
    {
        return currentGrade == CharacterGrade.Senior;
    }
}