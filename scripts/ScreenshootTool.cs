// create an orthographic camera and point it down, set size to map in game units (e.g., 2048)
// in project settings, set Maximum LOD Level to 1 for overhead screenshots, copy script to project

using UnityEngine;
using UnityEditor;
using System.IO;

public class ScreenshotTool : EditorWindow
{
  private string[] resolutions = { "2K", "4K", "8K", "16K" };
  private int[] sizes = { 2048, 4096, 8192, 16384 };
  private int selectedResolution = 0;
  private string pathPrefix = "C:/temp/snapshot.png";

  [MenuItem("Tools/Screenshot Tool")]
  public static void ShowWindow()
  {
    GetWindow<ScreenshotTool>("Screenshot Tool");
  }

  private void OnGUI()
  {
    GUILayout.Label("Screenshot Settings", EditorStyles.boldLabel);

    selectedResolution = EditorGUILayout.Popup("Resolution", selectedResolution, resolutions);
    pathPrefix = EditorGUILayout.TextField("Save File", pathPrefix);

    if (GUILayout.Button("Capture Screenshot"))
    {
      Capture();
    }
  }

  private void Capture()
  {
    Debug.Log("Max Texture Size: " + SystemInfo.maxTextureSize);

    int size = sizes[selectedResolution];
    //string filename = Path.Combine(pathPrefix, $"{resolutions[selectedResolution].ToLower()}.png");
    string filename = pathPrefix;

    Camera cam = null;
    foreach (var c in Camera.allCameras)
    {
        if (c.isActiveAndEnabled)
        {
            cam = c;
            break;
        }
    }

    if (cam == null)
    {
        Debug.LogError("No active camera found.");
        return;
    }

    RenderTexture rt = new RenderTexture(size, size, 24, RenderTextureFormat.ARGBHalf); // enables 8k/16k textures
    cam.targetTexture = rt;

    Texture2D screenshot = new Texture2D(size, size, TextureFormat.RGB24, false);
    cam.Render();

    RenderTexture.active = rt;
    screenshot.ReadPixels(new Rect(0, 0, size, size), 0, 0);
    screenshot.Apply();

    cam.targetTexture = null;
    RenderTexture.active = null;
    DestroyImmediate(rt);

    byte[] bytes = screenshot.EncodeToPNG();
    File.WriteAllBytes(filename, bytes);

    Debug.Log($"Screenshot saved to: {filename}");
  }
}

