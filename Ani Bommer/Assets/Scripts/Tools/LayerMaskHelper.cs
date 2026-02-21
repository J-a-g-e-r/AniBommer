using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerMaskHelper : MonoBehaviour
{
    public static bool ObjIsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        if ((layerMask.value & (1 << obj.layer)) > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static LayerMask CreateLayerMask(params int[] layers)
    {
        LayerMask layerMask = 0;
        foreach (int layer in layers)
        {
            layerMask |= (1 << layer);
        }
        return layerMask;
    }
}
