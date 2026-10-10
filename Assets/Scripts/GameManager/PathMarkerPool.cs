using System.Collections.Generic;
using UnityEngine;

// Object pool for PathMarker 
public class PathMarkerPool : MonoBehaviour
{
    public static PathMarkerPool Instance;

    [SerializeField] private PathMarker prefab;
    // Initial pool size
    [SerializeField] private int initialSize = 32;

    // Stack of currently unused markers
    private readonly Stack<PathMarker> free = new();
    // List of all created markers 
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

        // Create initial pool of markers
        for (int i = 0; i < initialSize; i++)
            Create();
    }

    // Creates a new PathMarker instance and adds it to the pool
    private PathMarker Create()
    {
        var m = Instantiate(prefab, transform);
        m.Hide();
        all.Add(m);
        free.Push(m);
        return m;
    }

    // Retrieves a marker from the pool
    // Creates a new one if the pool is empty
    public PathMarker Get()
    {
        if (free.Count == 0)
            Create();

        var m = free.Pop();
        m.gameObject.SetActive(true);
        return m;
    }

    // Returns a marker back to the pool
    public void Release(PathMarker marker)
    {
        if (marker == null)
            return;

        marker.Hide();
        free.Push(marker);
    }
}
    
