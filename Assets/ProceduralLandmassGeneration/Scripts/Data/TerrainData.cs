using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TerrainData : UpdateableData
{
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
    
    public float uniformScale = 5f;
    public bool useFlatShading;
    public bool useFalloff;
}
