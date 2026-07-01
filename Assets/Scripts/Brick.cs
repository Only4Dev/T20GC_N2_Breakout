using UnityEngine;

public class Brick : MonoBehaviour
{
    public void BrickBreak()
    {
        Destroy(this.gameObject);
    }
}
