using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteGrid : MonoBehaviour
{
    Transform player;
    float gridSize = 10f;
    float minHeight = 10f;
    void Start()
    {
        player = GameObject.Find("FPSPlayer").transform;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(Mathf.RoundToInt(player.position.x / gridSize) * gridSize, minHeight, Mathf.RoundToInt(player.position.z / gridSize) * gridSize);
    }
}
