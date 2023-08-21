using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MaterialList", menuName = "ScriptableObjects/DataContainers/MaterialList", order = 1)]
public class MaterialList : ScriptableObject
{
    public List<Material> materialList;
}
