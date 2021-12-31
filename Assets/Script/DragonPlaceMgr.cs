using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonPlaceMgr : MonoBehaviour
{
    public static DragonPlaceMgr instance = null;
    int[] HornNum = {2, 0, 0, 2, 2, 2};
    int[] HornArrayIndex = { 1, -1, -1, 2, 2, 0 };
    float[] HornSize = { 0, 0, 1, 1, 1.1f, 1.1f, 1.2f, 1.2f, 1.3f, 1.3f, 1.4f, 1.4f, 1.5f, 1.5f, 1.6f };
    Vector3 defaultHornSize = new Vector3(0.625f, 0.625f, 0.625f);
    // Start is called before the first frame update
    public GameObject dragonPlacePrefab = null;

    public List<Dragon> dragons;
    public Texture2D[] faces;
    public Texture2D[] bodys;
    public Material[] dragonMaterials;
    public GameObject[] horns;
    public Capture capture;
    private void Awake()
    {
        if (DragonPlaceMgr.instance) DragonPlaceMgr.instance = this;
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 15; j++)
            {
                dragons.Add(new Dragon(i * 15 + j, (DragonType)i, j));
                Debug.Log(dragons[i * 15 + j].dragonIndex);
            }
        }
    }


    void Start()
    {
        foreach (Dragon dragon in dragons)
        {
            loadDragonFromResourceToMyDragon(dragon);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void loadDragonFromResourceToMyDragon(Dragon dragon)
    {
        string path = string.Format("Model/{0}/{1}", dragon.dragonType.ToString(), Dragon.LevelFbxName[dragon.level]);
        GameObject dragonPrefab = Resources.Load<GameObject>(path);
        GameObject dragonGO = Instantiate(dragonPrefab);
        GameObject dragonPlaceGO = Instantiate(dragonPlacePrefab);
        dragonGO.transform.parent = dragonPlaceGO.transform;
        dragonPlaceGO.transform.parent = this.gameObject.transform;
        dragonGO.transform.localEulerAngles = new Vector3(0, 60, 0);
        Camera camera = dragonPlaceGO.transform.Find("Camera").GetComponent<Camera>();
        dragonPlaceGO.transform.localPosition = new Vector3(0, dragon.dragonIndex * 5, 0);
        dragonGO.AddComponent<Dragon>();
        var dragonTmp = dragonGO.GetComponent<Dragon>();
        dragonTmp.level = dragon.level;
        dragonTmp.dragonType = dragon.dragonType;
        dragonTmp.dragonIndex = dragon.dragonIndex;
        InitDragonNode(dragonGO);
        Debug.Log(dragonGO.GetComponent<Dragon>().level);
        string pathDirector = string.Format("D:/papagroup/Dragons_SD/Image/{0}", dragon.dragonType.ToString());
        capture.CreatePathBy(pathDirector);
        capture.captureCamera = camera;
        string pathImg = string.Format("{0}/{1}.png", pathDirector, dragon.level.ToString());
        capture.CaptureScreen(pathImg);
    }

    void InitDragonNode(GameObject dragonNode)
    {
        Dragon dragon = dragonNode.GetComponent<Dragon>();
        // Add texture to material
        setTextureToMaterial(this.faces[(int)dragon.dragonType], this.dragonMaterials[0]);
        setTextureToMaterial(this.bodys[(int)dragon.dragonType], this.dragonMaterials[1]);
        // Init Material for dragon
        this.InitMaterial(dragonNode);
        // Add horn
        if (HornNum[(int)dragon.dragonType] > 0 && dragon.level > 1)
        {
            Tuple<GameObject, GameObject> hornCouple = this.InitHorn(dragon);
            GameObject locator1 = dragonNode.transform.Find("Root/Center/Spine1/Spine2/Head/Mouth_Top1/locator1").gameObject;
            GameObject locator2 = dragonNode.transform.Find("Root/Center/Spine1/Spine2/Head/Mouth_Top1/locator2").gameObject;
            hornCouple.Item1.transform.parent = locator1.transform;
            hornCouple.Item2.transform.parent = locator2.transform;
            ResetGO(hornCouple.Item1);
            ResetGO(hornCouple.Item2);
            hornCouple.Item1.transform.localScale = new Vector3(
                this.defaultHornSize.x * this.HornSize[dragon.level],
                this.defaultHornSize.y * this.HornSize[dragon.level],
                this.defaultHornSize.z * this.HornSize[dragon.level]);
            hornCouple.Item2.transform.localScale = new Vector3(
                this.defaultHornSize.x * this.HornSize[dragon.level] * -1,
                this.defaultHornSize.y * this.HornSize[dragon.level],
                this.defaultHornSize.z * this.HornSize[dragon.level]);
        }
    }
    Tuple<GameObject, GameObject> InitHorn(Dragon dragon)
    {
        GameObject hornL = this.InitHornLR(dragon.dragonType);
        GameObject hornR = this.InitHornLR(dragon.dragonType);
        

        return new Tuple<GameObject, GameObject>(hornL, hornR);
    }

    void ResetGO(GameObject target)
    {
        target.transform.localPosition = new Vector3(0, 0, 0);
        target.transform.localEulerAngles = new Vector3(0, 0, 0);
        target.transform.localScale = new Vector3(0, 0, 0);
    }
    GameObject InitHornLR(DragonType type)
    {
        GameObject hornL = Instantiate(this.horns[HornArrayIndex[(int)type]]);
        Material material = new Material(this.dragonMaterials[1]);
        this.addMaterial(hornL, material);
        return hornL;
    }

    //GameObject InitHornR(DragonType type)
    //{
    //    GameObject hornR = Instantiate(this.horns[HornArrayIndex[(int)type]]);
    //    Material material = new Material(this.dragonMaterials[1]);
    //    this.addMaterial(hornR, material);
    //    return hornR;
    //}

    void setTextureToMaterial(Texture2D tex, Material material)
    {
        material.mainTexture = tex;
    }

    void InitMaterial(GameObject dragonNode)
    {
        for (int index = 0; index < dragonNode.transform.childCount; index++)
        {
            GameObject element = dragonNode.transform.GetChild(index).gameObject;
            if (!element.GetComponent<Renderer>()) continue;
            Material material = null;
            if (element.name == "Face")
            {
                material = new Material(this.dragonMaterials[0]);
            }
            else material = new Material(this.dragonMaterials[1]);
            this.addMaterial(element, material);
        }
    }
    //Material instantiateMaterial(Material target)
    //{
    //    Material material = new Material(target);
    //    material.CopyPropertiesFromMaterial(target);
    //    return material;
    //}
    void addMaterial(GameObject target, Material material)
    {
        Renderer meshRender = target.GetComponent<Renderer>();
        meshRender.material = material;
    }
}
    
