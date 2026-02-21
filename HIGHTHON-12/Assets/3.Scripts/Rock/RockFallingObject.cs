using UnityEngine;

public class RockFallingObject : MonoBehaviour
{
    private RockDodgeGameManager gameManager;
    private Transform characterTransform;
    private float despawnY;
    private bool hasScored;
    private bool hasTriggeredGameOver;

    public void Initialize(RockDodgeGameManager manager, Transform character, float destroyY)
    {
        gameManager = manager;
        characterTransform = character;
        despawnY = destroyY;
    }

    private void Update()
    {
        if (gameManager == null)
        {
            return;
        }

        if (!hasScored && characterTransform != null && transform.position.y < characterTransform.position.y)
        {
            hasScored = true;
            gameManager.OnRockPassedCharacter();
        }

        if (transform.position.y <= despawnY)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHitCharacter(other.transform);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryHitCharacter(collision.transform);
    }

    private void TryHitCharacter(Transform target)
    {
        if (hasTriggeredGameOver || gameManager == null || characterTransform == null || target == null)
        {
            return;
        }

        if (target == characterTransform || target.IsChildOf(characterTransform) || characterTransform.IsChildOf(target))
        {
            hasTriggeredGameOver = true;
            gameManager.OnRockHitCharacter();
        }
    }
}