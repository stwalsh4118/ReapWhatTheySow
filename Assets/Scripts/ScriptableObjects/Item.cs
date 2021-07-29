using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "new Item",menuName = "Items/Item")]
public class Item : ScriptableObject
{


    public string itemName;
    public string flavorText;
    public int itemTier;
    public bool equipment;
    public string prefabPath;
    public Sprite icon;
    public equipmentType typeOfEquipment;
}

public enum equipmentType {
    ALL,
    AXE,
    PICK,
    NONE
}
