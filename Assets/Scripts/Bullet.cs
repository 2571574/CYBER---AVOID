using UnityEngine;

public class Bullet : MonoBehaviour
{
    public bool isDodged = false;

    private void OnEnable()
    {
        isDodged = false;
    }
}