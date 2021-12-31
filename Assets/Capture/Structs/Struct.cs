using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ColorInElement
{
    [Tooltip("The Child of the model to be modified. If it cannot be found, the main model will be used.")]
    public GameObject element;
    [Tooltip("Material name to be modified. If it cannot be found, the first material will be used")]
    public string material;
    [Tooltip("Property to be modified. If it not exist, the main color will be used")]
    public string property;
    [Tooltip("List color will be used")]
    public List<Color> colors;
}
