using UnityEngine;

[CreateAssetMenu(fileName = "NumberFont", menuName = "Breakout/Number Font")]
public class NumberFont : ScriptableObject
{
    [SerializeField] private Sprite[] digits = new Sprite[10];

    public Sprite GetDigit(int digit)
    {
        return digits[Mathf.Clamp(digit, 0, 9)];
    }

    public float GetWidth(int digit)
    {
        Sprite sprite = GetDigit(digit);

        if (sprite == null)
            return 0f;

        return sprite.rect.width / sprite.pixelsPerUnit;
    }
}