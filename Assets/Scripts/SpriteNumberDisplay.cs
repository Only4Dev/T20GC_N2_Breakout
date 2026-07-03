using UnityEngine;

public class SpriteNumberDisplay : MonoBehaviour
{
    [SerializeField] private NumberFont font;
    [SerializeField] private SpriteRenderer[] digitSlots; // assign manually in Inspector, left-to-right or however you've arranged them
    [SerializeField] private bool rightAlign = true;

    private int lastRenderedValue = -1;

    public void SetValue(int value)
    {
        if (value == lastRenderedValue)
            return;

        lastRenderedValue = value;

        string text = Mathf.Max(0, value).ToString();

        if (text.Length > digitSlots.Length)
            text = text.Substring(text.Length - digitSlots.Length); // keep the least-significant digits if it overflows

        int emptySlots = digitSlots.Length - text.Length;

        for (int i = 0; i < digitSlots.Length; i++)
        {
            int textIndex = rightAlign ? i - emptySlots : i;

            if (textIndex >= 0 && textIndex < text.Length)
            {
                int digit = text[textIndex] - '0';
                digitSlots[i].sprite = font.GetDigit(digit);
                digitSlots[i].enabled = true;
            }
            else
            {
                digitSlots[i].enabled = false; // no digit here yet, hide it
            }
        }
    }
}