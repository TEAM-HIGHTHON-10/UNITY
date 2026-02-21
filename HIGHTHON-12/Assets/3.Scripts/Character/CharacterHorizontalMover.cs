using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterHorizontalMover : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Clamp Area (Optional)")]
    [SerializeField] private Transform moveArea;
    [SerializeField] private Vector2 fallbackAreaSize = new Vector2(8f, 1f);

    private void Update()
    {
        float inputX = ReadHorizontalInput();
        if (Mathf.Abs(inputX) <= 0.001f)
        {
            return;
        }

        Vector3 nextPos = transform.position + Vector3.right * (inputX * moveSpeed * Time.deltaTime);

        if (TryGetHorizontalBounds(out float minX, out float maxX))
        {
            nextPos.x = Mathf.Clamp(nextPos.x, minX, maxX);
        }

        transform.position = nextPos;
    }

    private static float ReadHorizontalInput()
    {
        if (Keyboard.current == null)
        {
            return 0f;
        }

        float inputX = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            inputX -= 1f;
        }

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            inputX += 1f;
        }

        return Mathf.Clamp(inputX, -1f, 1f);
    }

    private bool TryGetHorizontalBounds(out float minX, out float maxX)
    {
        minX = 0f;
        maxX = 0f;

        if (moveArea == null)
        {
            return false;
        }

        Bounds bounds = new Bounds(moveArea.position, new Vector3(fallbackAreaSize.x, fallbackAreaSize.y, 0f));

        Collider2D areaCollider = moveArea.GetComponent<Collider2D>();
        if (areaCollider != null)
        {
            bounds = areaCollider.bounds;
        }
        else
        {
            Renderer areaRenderer = moveArea.GetComponent<Renderer>();
            if (areaRenderer != null)
            {
                bounds = areaRenderer.bounds;
            }
        }

        minX = bounds.min.x;
        maxX = bounds.max.x;
        return true;
    }
}