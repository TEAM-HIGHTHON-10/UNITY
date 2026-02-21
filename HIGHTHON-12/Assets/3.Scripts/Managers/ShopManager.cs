using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [Header("Miljip Hat Item")]
    [SerializeField] private int hatPriceGoldenEgg = 100;
    [SerializeField] private Button buyHatButton;
    [SerializeField] private GameObject buyAvailableChild;
    [SerializeField] private TMP_Text hatStatusText;

    private bool isHatPurchased;
    private bool isHatEquipped;

    private void Start()
    {
        RefreshShopUI();
    }

    private void Update()
    {
        RefreshPurchaseButtonState();
    }

    public void OnClickBuyOrToggleHat()
    {
        if (GameManager.I == null)
        {
            return;
        }

        if (!isHatPurchased)
        {
            if (!GameManager.I.TryUseGoldenEggs(hatPriceGoldenEgg))
            {
                RefreshShopUI();
                return;
            }

            isHatPurchased = true;
            isHatEquipped = true;
            GameManager.I.SetHatEquipped(true);
            RefreshShopUI();
            return;
        }

        isHatEquipped = !isHatEquipped;
        GameManager.I.SetHatEquipped(isHatEquipped);
        RefreshShopUI();
    }

    private void RefreshShopUI()
    {
        RefreshPurchaseButtonState();
        RefreshStatusText();
    }

    private void RefreshPurchaseButtonState()
    {
        bool canPurchase = GameManager.I != null && GameManager.I.GoldenEggCount >= hatPriceGoldenEgg;
        bool showChildButton = isHatPurchased || canPurchase;

        if (buyAvailableChild != null)
        {
            buyAvailableChild.SetActive(showChildButton);
        }

        if (buyHatButton != null)
        {
            buyHatButton.interactable = isHatPurchased || canPurchase;
        }
    }

    private void RefreshStatusText()
    {
        if (hatStatusText == null)
        {
            return;
        }

        if (!isHatPurchased)
        {
            hatStatusText.text = $"밀집모자 {hatPriceGoldenEgg}G";
            return;
        }

        hatStatusText.text = isHatEquipped ? "장착중" : "해제됨";
    }
}