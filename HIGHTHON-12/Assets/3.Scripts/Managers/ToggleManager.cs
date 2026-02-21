using UnityEngine;

public class ToggleManager : MonoBehaviour
{
    [Header("Game")]
    [SerializeField] private Animator gameAnimator;
    [SerializeField] private string gameIdleState = "IdleGame";
    [SerializeField] private string gameOpenState = "OpenGame";
    [SerializeField] private string gameCloseState = "CloseGame";
    [SerializeField] private bool isGameOpen;

    [Header("Shop")]
    [SerializeField] private Animator shopAnimator;
    [SerializeField] private string shopIdleState = "IdleShop";
    [SerializeField] private string shopOpenState = "OpenShop";
    [SerializeField] private string shopCloseState = "CloseShop";
    [SerializeField] private bool isShopOpen;

    private void Start()
    {
        PlayState(gameAnimator, gameIdleState);
        PlayState(shopAnimator, shopIdleState);

        isGameOpen = false;
        isShopOpen = false;
    }

    public void OnClickToggleGame()
    {
        if (isGameOpen)
        {
            PlayState(gameAnimator, gameCloseState);
            isGameOpen = false;
            return;
        }

        PlayState(gameAnimator, gameOpenState);
        isGameOpen = true;
    }

    public void OnClickToggleShop()
    {
        if (isShopOpen)
        {
            PlayState(shopAnimator, shopCloseState);
            isShopOpen = false;
            return;
        }

        PlayState(shopAnimator, shopOpenState);
        isShopOpen = true;
    }

    private static void PlayState(Animator targetAnimator, string stateName)
    {
        if (targetAnimator == null || string.IsNullOrWhiteSpace(stateName))
        {
            return;
        }

        targetAnimator.Play(stateName, 0, 0f);
    }
}