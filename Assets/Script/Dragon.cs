using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dragon : MonoBehaviour
{

    public DragonType dragonType;
    public int level;
    public int dragonIndex;
    public static readonly string[] LevelFbxName = {
      "Level1_2",
      "Level1_2",
      "Level3",
      "Level4",
      "Level5",
      "Level6_7",
      "Level6_7",
      "Level8_9",
      "Level8_9",
      "Level10",
      "Level11",
      "Level12_13",
      "Level12_13",
      "Level14",
      "Level15",
    };

    public Dragon() : base() { }

    public Dragon(int _dragonIndex, DragonType _type, int _level) : base()
    {
        dragonIndex = _dragonIndex;
        dragonType = _type;
        level = _level;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}

public enum DragonType
{
    FIRE,
    WATER,
    EARTH,
    METAL,
    WOOD,
    CYBER
}

