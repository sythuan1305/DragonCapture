using System;
using UnityEngine;

public class CaptureScript
{
    public int ImageWidth = 1920;
    public int ImageHeight = 1080;

    public string SaveFolder = "Captures";

    public string ModelName;

    public Element[] Element;
}

[Serializable]
public class Element
{
    public string NameElement;
    public string NameMaterial;
    public string NameProperty;
    public Color32[] Colors;
}