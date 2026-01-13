using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PathMarker : MonoBehaviour
{
    [SerializeField] private SpriteRenderer image;
    [SerializeField] private TextMeshPro text;

    public HexCell Cell { get; set; }

    public void Show(Color color, string label)
    {
        image.color = color;
        image.enabled = true;

        if (string.IsNullOrEmpty(label))
        {
            text.enabled = false;
        }
        else
        {
            text.text = label;
            text.enabled = true;
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetWorldPosition(Vector3 pos)
    {
        transform.position = pos;
    }
}
