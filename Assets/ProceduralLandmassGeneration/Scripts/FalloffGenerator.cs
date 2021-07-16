using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FalloffGenerator
{   
    //generates fall off map with gradient black in the middle white on the edges
    public static float[,] GenerateFalloffMap(int size) {
        float[,] map = new float[size, size];

        for(int x = 0; x < size; x++) {
            for(int y = 0; y < size; y ++) {
                float i = (x/(float)size * 2) - 1;
                float j = (y/(float)size * 2) - 1;

                float value = Mathf.Max(Mathf.Abs(i), Mathf.Abs(j));
                map[x,y] = Evaluate(value);
            }
        }

        return map;
    }

    //function that evalues the equation f(x) = (x^a)/((x^a) + (b-b*x)^a) so we can manipulate our falloff map 
    private static float Evaluate(float value) {
        float a = 3;
        float b = 2.2f;

        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow((b - b * value), a));
    }
}
