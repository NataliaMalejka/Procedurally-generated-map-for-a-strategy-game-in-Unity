using TMPro;
using UnityEngine;

// Visual marker used to display a single step of a unit path
public class PathMarker : MonoBehaviour
{
    [SerializeField] private SpriteRenderer image;
    [SerializeField] private TextMeshPro text;

    // Hex cell this marker is currently assigned to
    public HexCell Cell { get; set; }

    // Displays the marker with a given color and optional label
    public void Show(Color color, string label)
    {
        // Set marker color
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

        // Ensure marker GameObject is active
        gameObject.SetActive(true);
    }

    // Hides the marker visually
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // Sets marker world position
    public void SetWorldPosition(Vector3 pos)
    {
        transform.position = pos;
    }
}
