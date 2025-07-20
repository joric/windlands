// create an orthographic camera and point it down, set size to map in game units (e.g., 2048)
// in project settings, set Maximum LOD Level to 1 for overhead screenshots, copy script to project

using UnityEngine;
using System.Collections;
using System.IO;

public class AdjustableScreenshot : MonoBehaviour
{
    [Header("Screenshot resolution")]
    public int width = 4096;
    public int height = 4096;

    [Header("Output file")]
    public string filePath = @"D:/Projects/github/windlands/4k.png";

    void Start()
    {
        StartCoroutine(CaptureScreenshot());
    }

    IEnumerator CaptureScreenshot()
    {
        yield return new WaitForEndOfFrame();

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
            yield break;
        }

        //RenderTexture rt = new RenderTexture(width, height, 24);
        RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGBHalf); // enables 8k/16k textures
        cam.targetTexture = rt;

        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
        cam.Render();

        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenShot.Apply();

        cam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        byte[] bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);

        Debug.Log($"Screenshot saved to {filePath}");

        Debug.Log("Max Texture Size: " + SystemInfo.maxTextureSize);
    }
}

