using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>Compact on-device AR and prefab-spawner diagnostics for P11.</summary>
public class ARStatusOverlay : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager imageManager;
    [SerializeField] private bool showDiagnosticHud = true;

    private ARImagePrefabSpawner spawner;
    private string status = "SEARCHING FOR MARKER";
    private int trackedImageCount;

    private void Reset()
    {
        imageManager = GetComponent<ARTrackedImageManager>();
    }

    private void OnEnable()
    {
        if (imageManager == null)
            imageManager = GetComponent<ARTrackedImageManager>();
        spawner = GetComponent<ARImagePrefabSpawner>();
        if (imageManager != null)
            imageManager.trackablesChanged.AddListener(OnImagesChanged);
        else
            Debug.LogWarning("[AR Kelompok 4] ARTrackedImageManager belum terpasang pada XR Origin.");
    }

    private void OnDisable()
    {
        if (imageManager != null)
            imageManager.trackablesChanged.RemoveListener(OnImagesChanged);
    }

    private void OnImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> changes)
    {
        trackedImageCount = imageManager.trackables.count;
        foreach (ARTrackedImage image in changes.added)
            status = "MARKER DETECTED";
        foreach (ARTrackedImage image in changes.updated)
            status = image.trackingState == TrackingState.Tracking
                ? "TRACKING ACTIVE" : "TRACKING LIMITED";
        if (changes.removed.Count > 0)
            status = "SEARCHING FOR MARKER";
    }

    private void OnGUI()
    {
        if (!showDiagnosticHud)
            return;

        if (spawner == null)
            spawner = GetComponent<ARImagePrefabSpawner>();

        GUI.depth = -10;
        float width = Mathf.Min(Screen.width - 32f, 500f);
        Rect panel = new Rect(16f, 16f, width, 116f);
        GUI.Box(panel, "");
        GUI.Label(new Rect(30f, 27f, width - 24f, 22f), "PINK BOTANICAL AR  |  KELOMPOK 4");
        GUI.Label(new Rect(30f, 51f, width - 24f, 22f), status + "   |   TRACKABLES: " + trackedImageCount);
        GUI.Label(new Rect(30f, 76f, width - 24f, 32f),
            spawner != null ? "OBJECT: " + spawner.DiagnosticStatus : "OBJECT: SPAWNER NOT FOUND");
    }
}
