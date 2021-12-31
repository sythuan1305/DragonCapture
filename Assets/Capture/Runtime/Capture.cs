using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Capture : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField]
    [Tooltip("The camera will be used. If null, the main camera will be used")]
    public Camera captureCamera = null;

    [Header("Import")]
    [SerializeField]
    [Tooltip("The json to import")]
    public TextAsset importJson = null;

    [Header("Image size")]
    [SerializeField]
    public int width = 1920;
    [SerializeField]
    public int height = 1080;

    [Header("Path")]
    [SerializeField]
    [Tooltip("The folder used to save images. If it doesn't exist, it will be created")]
    public string saveFolder = "Captures";
    [SerializeField]
    public bool useAbsolutePath = false;
    [SerializeField]
    public string path = null;
    [SerializeField]
    public string savePath = null;

    [Header("Model")]
    [SerializeField]
    [Tooltip("The main model")]
    public GameObject mainModel;
    [SerializeField]
    public List<ColorInElement> colorsPerElement;

    [Header("Tool")]
    [SerializeField]
    [Tooltip("Enable limit capture")]
    public bool limitCapture = false;
    [SerializeField]
    public int limit = 10;
    private int counter = 0;

    private List<List<Material>> backupMaterials;

    void Start()
    {
        Init();
    }

    public void Init()
    {
        if (captureCamera == null)
            captureCamera = GetComponent<Camera>();
        if (captureCamera == null)
            captureCamera = Camera.main;

        if (useAbsolutePath)
            savePath = path + '/' + saveFolder;
        else savePath = Application.dataPath + '/' + saveFolder;
    }

    public void CreatePath()
    {
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);
    }

    public void CreatePathBy(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    public void SetPath()
    {
        if (useAbsolutePath)
            savePath = path + '/' + saveFolder;
        else savePath = Application.dataPath + '/' + saveFolder;
    }

    private void LateUpdate()
    {
        if (Input.GetKeyUp(KeyCode.F9))
            CaptureScreen(savePath + '/' + (new DateTime().ToString("yyyyMMdd_HHmmss")) + ".png");
        if (Input.GetKeyUp(KeyCode.F10))
            ActiveScriptCapture();
    }

    public void CaptureScreen()
    {
        CreatePath();
        string path = savePath + "/" + (new DateTime().ToString("yyyyMMdd_HHmmss")) + ".png";
        CaptureScreen(path);
    }

    public void CaptureScreen(string path)
    {
        RenderTexture current = RenderTexture.active;
        RenderTexture currentCamera = captureCamera.targetTexture;

        RenderTexture texture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);

        captureCamera.targetTexture = texture;
        RenderTexture.active = texture;

        captureCamera.Render();

        Texture2D image = new Texture2D(captureCamera.targetTexture.width, captureCamera.targetTexture.height, TextureFormat.ARGB32, false);
        image.ReadPixels(new Rect(0, 0, captureCamera.targetTexture.width, captureCamera.targetTexture.height), 0, 0);
        image.Apply();

        RenderTexture.active = current;
        captureCamera.targetTexture = currentCamera;

        byte[] bytes = image.EncodeToPNG();
        if (Application.isPlaying) Destroy(image);
        else DestroyImmediate(image);

        File.WriteAllBytes(path, bytes);
    }

    public void ActiveScriptCapture()
    {
        CreatePath();
        FixedColor();
        BackupMaterial();
        counter = 0;
        List<int> index = new List<int>(colorsPerElement.Count);
        for (int i = 0; i < colorsPerElement.Count; ++i)
            index.Add(0);
        while (index[0] < colorsPerElement[0].colors.Count)
        {
            if (limitCapture && counter >= limit) break;
            string path = savePath + '/' + SetupColor(index) + ".png";
            CaptureScreen(path);
            ++index[index.Count - 1];
            for (int i = index.Count - 1; i > 0; --i)
            {
                if (index[i] >= colorsPerElement[i].colors.Count)
                {
                    index[i] = 0;
                    ++index[i - 1];
                }
                else break;
            }
            ++counter;
        }
        RevertMaterial();
    }

    public void FixedColor()
    {
        foreach (ColorInElement item in colorsPerElement)
        {
            for (int i = 0; i < item.colors.Count; ++i)
            {
                Color newColor = new Color(item.colors[i].r, item.colors[i].g, item.colors[i].b);
                item.colors[i] = newColor;
            }
        }
    }

    private void BackupMaterial()
    {
        backupMaterials = new List<List<Material>>();
        foreach (ColorInElement item in colorsPerElement)
        {
            Renderer elementRenderer = item.element.GetComponent<Renderer>();
            Material[] materials = elementRenderer.sharedMaterials;
            List<Material> tmp = new List<Material>(materials);
            backupMaterials.Add(tmp);
        }
    }

    private void RevertMaterial()
    {
        if (backupMaterials == null) return;
        for (int i = 0; i < colorsPerElement.Count; ++i)
        {
            Renderer elementRenderer = colorsPerElement[i].element.GetComponent<Renderer>();
            elementRenderer.materials = backupMaterials[i].ToArray();
        }
    }

    public string SetupColor(List<int> index)
    {
        string nameString = mainModel.name;
        for (int i = 0; i < index.Count; ++i)
        {
            Renderer elementRenderer = colorsPerElement[i].element.GetComponent<Renderer>();
            Material[] materials = elementRenderer.sharedMaterials;
            Material elementMaterial = null;
            for (int j = 0; j < materials.Length; ++j)
                if (materials[j].name.Equals(colorsPerElement[i].material))
                {
                    elementMaterial = elementRenderer.materials[j];
                    break;
                }
            if (elementMaterial == null) elementMaterial = elementRenderer.material;
            if (elementMaterial.HasProperty(colorsPerElement[i].property))
                elementMaterial.SetColor(colorsPerElement[i].property, colorsPerElement[i].colors[index[i]]);
            else
            {
                Debug.LogWarning(string.Format("The material {0} don't have properties {1}!", colorsPerElement[i].material, colorsPerElement[i].property));
                elementMaterial.color = colorsPerElement[i].colors[index[i]];
            }
            nameString += "_" + ColorUtility.ToHtmlStringRGB(colorsPerElement[i].colors[index[i]]);
        }
        Debug.Log(nameString);
        return nameString;
    }
}
