using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "new TerrainData",menuName = "UpdateableData/TerrainData")]

public class TerrainData : UpdateableData
{
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
    
    public float uniformScale = 5f;
    public bool useFlatShading;
    public bool useFalloff;
    public float minHeight {
        get {
            return uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(0);
        }
    }

    public float maxHeight {
        get {
            return uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(1);
        }
    }
}
