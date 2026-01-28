using System.Collections.Generic;
using UnityEngine;

public class PathMarkerPool : MonoBehaviour
{
    public static PathMarkerPool Instance;

    [SerializeField] private PathMarker prefab;
    [SerializeField] private int initialSize = 32;

    private readonly Stack<PathMarker> free = new();
    private readonly List<PathMarker> all = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        for (int i = 0; i < initialSize; i++)
            Create();
    }

    private PathMarker Create()
    {
        var m = Instantiate(prefab, transform);
        m.Hide();
        all.Add(m);
        free.Push(m);
        return m;
    }

    public PathMarker Get()
    {
        if (free.Count == 0)
            Create();

        var m = free.Pop();
        m.gameObject.SetActive(true);
        return m;
    }

    public void Release(PathMarker marker)
    {
        if (marker == null)
            return;

        marker.Hide();
        free.Push(marker);
    }
}
    
