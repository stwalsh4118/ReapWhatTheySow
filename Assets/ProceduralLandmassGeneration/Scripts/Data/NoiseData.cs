using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "new NoiseData",menuName = "UpdateableData/NoiseData")]

public class NoiseData : UpdateableData
{
    public float noiseScale;
    public int octaves;

    //range attribute clamps the persistance value between 0 and 1 and creates a slider in the editor
    [Range(0,1)]
    public float persistance;
    public float lacunarity;
    public int seed;
    public Vector2 offset;
    public Noise.NormalizeMode normalizeMode;


    #if UNITY_EDITOR
    protected override void OnValidate() {
        
        if(lacunarity < 1) {
            lacunarity = 1;
        }
        if(octaves < 0) {
            octaves = 0;
        }
        
        base.OnValidate();
    }

    #endif
}
