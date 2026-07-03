using UnityEngine;

public class FPSLimit : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        QualitySettings.vSyncCount = 0; // let targetFrameRate control it instead
        Application.targetFrameRate = 120;
    }

}
