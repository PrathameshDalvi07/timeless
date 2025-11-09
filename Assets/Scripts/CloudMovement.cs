using UnityEngine;

public class CloudMovement : MonoBehaviour
{
    [SerializeField] private float speed = 1f;

    private void Update()
    {
        // Move the cloud horizontally
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }
}
