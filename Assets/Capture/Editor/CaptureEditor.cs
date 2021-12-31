using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Capture))]
public class CaptureEditor : Editor
{
    MonoScript _script;

    private void OnEnable()
    {
        if (target == null) return;
        _script = MonoScript.FromMonoBehaviour((Capture)target);

        Capture capture = (Capture)target;
        capture.SetPath();
    }

    public override void OnInspectorGUI()
    {
        if (target == null) return;

        Capture capture = (Capture)target;
        serializedObject.Update();

        GUI.enabled = false;
        _script = EditorGUILayout.ObjectField("Script", _script, typeof(MonoScript), false) as MonoScript;
        GUI.enabled = true;

        //DrawDefaultInspector();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("captureCamera"));

        EditorGUILayout.PropertyField(serializedObject.FindProperty("importJson"));
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Import JSON"))
        {
            if (capture.importJson)
            {
                CaptureScript script = JsonUtility.FromJson<CaptureScript>(capture.importJson.text);
                if (script.ImageHeight >= 0) capture.height = script.ImageHeight;
                if (script.ImageWidth >= 0) capture.width = script.ImageWidth;
                if (!string.IsNullOrWhiteSpace(script.SaveFolder)) capture.saveFolder = script.SaveFolder;
                else
                {
                    Debug.LogError("Please locate the save folder!");
                }
                GameObject model = GameObject.Find(script.ModelName);
                if (model)
                {
                    capture.mainModel = model;
                    capture.colorsPerElement.Clear();
                    for (int i = 0; i < script.Element.Length; ++i)
                    {
                        Element element = script.Element[i];
                        GameObject elementObject;
                        if (string.IsNullOrWhiteSpace(element.NameElement) || element.NameElement.Equals(model.name))
                            elementObject = model;
                        else
                            elementObject = model.transform.Find(element.NameElement).gameObject;
                        if (!elementObject)
                        {
                            Debug.LogWarning(string.Format("The model has no element named {0}, and will be ignored", element.NameElement));
                        }
                        ColorInElement colorInElement = new ColorInElement();
                        colorInElement.element = elementObject;

                        colorInElement.material = element.NameMaterial;
                        colorInElement.property = element.NameProperty;
                        colorInElement.colors = new List<Color>();
                        for (int j = 0; j < element.Colors.Length; ++j)
                            colorInElement.colors.Add((Color)element.Colors[j]);
                        capture.colorsPerElement.Add(colorInElement);
                    }
                }
                else
                {
                    Debug.LogError(string.Format("The model named {0} could not be found.", script.ModelName));
                    Debug.LogError("Please attach manually. Or re-import the json!");
                    Debug.LogError("Elements cannot be imported without a model!");
                }
                capture.FixedColor();
            }
            else
                Debug.Log("Please attach importJson");
        }
        if (GUILayout.Button("Export JSON"))
        {
            CaptureScript script = new CaptureScript();
            script.ImageWidth = capture.width;
            script.ImageHeight = capture.height;
            script.ModelName = capture.mainModel.name;
            script.SaveFolder = capture.saveFolder;
            script.Element = new Element[capture.colorsPerElement.Count];
            for (int i = 0; i < capture.colorsPerElement.Count; ++i)
            {
                ColorInElement colorInElement = capture.colorsPerElement[i];
                Element element = new Element();
                element.NameElement = colorInElement.element.name;
                element.Colors = colorInElement.colors.ConvertAll(new System.Converter<Color, Color32>(ColorToColor32)).ToArray();
                element.NameMaterial = colorInElement.material;
                element.NameProperty = colorInElement.property;
                script.Element[i] = element;
            }
            string content = JsonUtility.ToJson(script, true);
            string path = capture.savePath + string.Format("/script_{0}.json", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            Debug.Log(string.Format("Save script to: {0}", path));
            File.WriteAllText(path, content);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            capture.FixedColor();
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("width"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("height"));

        EditorGUILayout.PropertyField(serializedObject.FindProperty("mainModel"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("colorsPerElement"));

        foreach (ColorInElement item in capture.colorsPerElement)
        {
            if (item.element)
            {
                Renderer renderer = item.element.GetComponent<Renderer>();
                if (renderer)
                {
                    Material material = null;
                    for (int i = 0; i < renderer.sharedMaterials.Length; ++i)
                    {
                        //string name = renderer.sharedMaterials[i].name;
                        if (renderer.sharedMaterials[i].name.Equals(item.material))
                        {
                            material = renderer.sharedMaterials[i];
                            break;
                        }
                    }
                    if (material != null) { if (!material.HasProperty(item.property)) EditorGUILayout.HelpBox(string.Format("The Material {0} has no Material named {1}.", item.material, item.property), MessageType.Warning); }
                    else EditorGUILayout.HelpBox(string.Format("The GameObject {0} has no Material named {1}. The fisrt Material will be used.", item.element.name, item.material), MessageType.Warning);
                }
            }
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("saveFolder"));

        EditorGUILayout.PropertyField(serializedObject.FindProperty("useAbsolutePath"));
        if (capture.useAbsolutePath)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("path"));
            while (capture.path.EndsWith("/"))
            {
                capture.path.Remove(capture.path.Length - 1);
            }
        }
        capture.SetPath();

        GUI.enabled = false;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("savePath"));
        GUI.enabled = true;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("limitCapture"));
        if (capture.limitCapture)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("limit"));

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Capture One"))
        {
            capture.CaptureScreen();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }
        if (GUILayout.Button("Capture All"))
        {
            capture.ActiveScriptCapture();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }
        GUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }

    public static Color32 ColorToColor32(Color color)
    {
        return (Color32)color;
    }
}
