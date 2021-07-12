using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//create a custom editor within the MapGenerator component
[CustomEditor (typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI() {
        
        //get reference to the MapGenerator component that we want to change the editor of
        MapGenerator mapGen = (MapGenerator)target;

        //draw the default inspector
        if(DrawDefaultInspector()) {

            //if we have autoUpdate enabled within the MapGenerator component we re-generate the map everytime we change (aka draw) the inspector
            if(mapGen.autoUpdate) {
                mapGen.GenerateMap();
            }
        }

        //create an editor button that calls the GenerateMap function everytime we press it
        if(GUILayout.Button("Generate")) {
            mapGen.GenerateMap();
        }
    }

}
