using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{   

    private const float viewerMoveThresholdForChunkUpdate = 25f;
    private const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    private const float colliderUpdateDistanceThreshold = 5f;
    public static float maxViewDistance = 450;

    public int colliderLODIndex;
    //array to hold user inputted LODs and their respective distance thresholds
    public LODInfo[] detailLevels;
    public Transform viewer;

    public static Vector2 viewerPosition;
    private Vector2 previousViewerPosition;

    public static MapGenerator mapGenerator;
    private int chunkSize;
    private int chunksVisibleInViewDistance;
    public Material mapMaterial;

    //dictionary to save a terrain chunk and the coordinate its in (each chunk is 1 place on the grid, so middle chunk in 0,0 and chunk on the left is -1,0 etc.)
    public Dictionary<Vector2, TerrainChunk> terrainChunkDict = new Dictionary<Vector2, TerrainChunk>();

    //list of chunks we need to set not visible since theyre out of range
    private static List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    public RandomObjectSpawner randomObjectSpawner;

    private void Start() {

        mapGenerator = FindObjectOfType<MapGenerator>();
        //initialize number of chunks we can see with the set max view distance to calculate chunks we need to see obv
        maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
        chunkSize = mapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);

        UpdateVisibleChunks();
    }
    
    private void Update() {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / mapGenerator.terrainData.uniformScale;

        if(viewerPosition != previousViewerPosition) {
            foreach(TerrainChunk chunk in visibleTerrainChunks) {
                chunk.UpdateCollisionMesh();
            }
        }

        //only update the visible chunks if the viewer has moved a decent distance to that we dont have to update the chunks every frame needlessly
        if((previousViewerPosition - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate) {
            UpdateVisibleChunks();
            previousViewerPosition = viewerPosition;
        }
        
    }

    private void UpdateVisibleChunks() {
        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();
        //set all the chunks we saw last update to not visible since there may be some that we shouldn't be seeing anymore
        for(int i = visibleTerrainChunks.Count - 1; i >= 0; i--) {
            alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
            visibleTerrainChunks[i].UpdateTerrainChunk();
        }


        //get the chunk coordinates like described above
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        //loop through all the chunks we should be able to see
        for(int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++) {
            for(int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++) {

                //calculate the chunks coord from where the viewer is currently
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if(!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord)) {
                    //if we've seen this chunk before update it appropriately
                    if(terrainChunkDict.ContainsKey(viewedChunkCoord)) {
                        terrainChunkDict[viewedChunkCoord].UpdateTerrainChunk();

                    //else if we haven't seen this chunk before add it to the dictionary of chunks we haven't seen
                    } else {
                        terrainChunkDict.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, colliderLODIndex, transform, mapMaterial));
                    }
                }

            }
        }
    }

    public Mesh GetTerrainMesh(GameObject Terrain) {
        TerrainChunk tc = terrainChunkDict[Terrain.GetComponent<TerrainCoordHolder>().terrainCoord];
        return tc.lodMeshes[0].mesh;
    }

    //class created to hold the data for our chunks
    public class TerrainChunk {

        public Vector2 coord;

        //holds the mesh for the chunk that will be rendered to the screen, the position of said chunk, and the perimeter bounds of the chunk so we know if its in render distance
        GameObject meshObject;
        Vector2 position;
        Bounds chunkBounds;
        MeshRenderer meshRenderer;
        public MeshFilter meshFilter;
        public MeshCollider meshCollider;
        public TerrainCoordHolder terrainCoordHolder;
        LODInfo[] detailLevels;
        public LODMesh[] lodMeshes;
        MapData mapData;
        int colliderLODIndex;
        bool mapDataRecieved;
        int previousLODIndex = -1;
        bool hasSetCollider = false;

        //initialize chunk data
        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Material material) {

            this.coord = coord;
            this.detailLevels = detailLevels;   
            this.colliderLODIndex = colliderLODIndex;

            //calculate the x,z position of the chunk
            position = coord * size;

            //creates bounding box centered in the middle of the chunk and then the size of the chunk
            chunkBounds = new Bounds(position, Vector2.one * size);
            //calculate the world position of the chunk
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);
            //create the chunk game object
            meshObject = new GameObject("Terrain Chunk");
            terrainCoordHolder = meshObject.AddComponent<TerrainCoordHolder>();
            terrainCoordHolder.terrainCoord = coord;
            GameObject Water = Instantiate(Resources.Load("Prefabs/Water", typeof (GameObject))) as GameObject;
            Water.transform.parent = meshObject.transform;
            GameObject OutOfBoundsPlane = Instantiate(Resources.Load("Prefabs/OutOfBoundsPlane", typeof (GameObject))) as GameObject;
            OutOfBoundsPlane.transform.parent = meshObject.transform;
            //give the chunk game object a mesh renderer and filter and collider
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            //set the material for the chunk
            meshRenderer.material = material;
            //set the position of the chunk in the world
            meshObject.transform.position = positionV3 * mapGenerator.terrainData.uniformScale;
            //set the scale of the chunk
            meshObject.transform.localScale = Vector3.one * mapGenerator.terrainData.uniformScale;
            //set the parent of the chunk so they can all be under one parent
            meshObject.transform.parent = parent;
            //default set it to not visible
            SetVisible(false);

            //create all of the different meshes with the different level of details for this chunk
            lodMeshes = new LODMesh[detailLevels.Length];
            for(int i = 0; i < detailLevels.Length; i++) {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod);
                lodMeshes[i].updateCallback += UpdateTerrainChunk;
                if(i == colliderLODIndex) {
                    lodMeshes[i].updateCallback += UpdateCollisionMesh;
                }
            }

            //generate the map for the chunk which then generates the mesh for the chunk then sets the mesh for the chunk
            mapGenerator.RequestMapData(position, OnMapDataRecieved);
        }

        //once the map data has been generated set the map data to this chunk, designate that the data has been recieved, and generate and set the texture based on the 
        //  map data that was generated
        private void OnMapDataRecieved(MapData mapData) {
            this.mapData = mapData;
            mapDataRecieved = true;

            UpdateTerrainChunk();
        }

        //finds the point on the chunk's perimeter that is closest to the player and if the distance is greater than the max view distance then it will disable it
        public void UpdateTerrainChunk() {
            if(!mapDataRecieved) {return;}

            //returns the smallest sqr distance between the given position and the bounding box
            float viewerDistanceFromNearestEdge = Mathf.Sqrt(chunkBounds.SqrDistance(viewerPosition));
            bool wasVisible = IsVisible();
            bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;

            //if the chunk is visible set the mesh to the correct LOD mesh that corresponds to its distance away from the viewer
            if(visible) {
                int lodIndex = 0;
                //loop through all of the distance thresholds to check which LOD corresponds to its distance from the viewer
                for(int i = 0; i < detailLevels.Length - 1; i++) {
                    if(viewerDistanceFromNearestEdge > detailLevels[i].visibleDistanceThreshold) {
                        lodIndex = i+1;
                    } else {
                        break;
                    }
                }

                //if the chunk is in a different LOD threshold than on the previous frame, change the mesh to match which LOD it should be
                if(lodIndex != previousLODIndex) {

                    //grab the mesh from the array of different LOD meshes
                    LODMesh lodMesh = lodMeshes[lodIndex];

                    //if the mesh for the LOD we're in has been created then set the chunks mesh to the LOD mesh
                    if(lodMesh.hasMesh) {
                        previousLODIndex = lodIndex;
                        meshFilter.mesh = lodMesh.mesh;

                        RandomObjectSpawner.instance.SpawnObjectsPoisson(lodMesh.mesh.vertices, lodMesh.mesh.normals, position, meshObject);

                    //if the mesh hasnt been created yet (and the mesh hasnt even been requested yet) request that the mesh be created
                    //  (if it has been requested but not created then we will wait for a later frame to update the LOD)
                    } else if(!lodMesh.hasRequestedMesh) {
                        lodMesh.RequestMesh(mapData);
                    }
                }

                
            }
            if(wasVisible != visible) {
                if(visible) {
                    //add this chunk to the visible last update list since it was indeed visible so that we can set it invisible on the next frame
                    visibleTerrainChunks.Add(this);
                } else {
                    visibleTerrainChunks.Remove(this);
                }
                SetVisible(visible);
            }
        }

        public void UpdateCollisionMesh() {
            if(hasSetCollider) {return;}

            float sqrDistanceFromViewerToEdge = chunkBounds.SqrDistance(viewerPosition);

            if(sqrDistanceFromViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDistanceThreshold) {
                if(!lodMeshes[colliderLODIndex].hasRequestedMesh) {
                    lodMeshes[colliderLODIndex].RequestMesh(mapData);
                }
            }

            if(sqrDistanceFromViewerToEdge < colliderUpdateDistanceThreshold * colliderUpdateDistanceThreshold) {
                if(lodMeshes[colliderLODIndex].hasMesh) {
                   meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
                   hasSetCollider = true;
                }
            }
        } 

        public void UpdateCollisionMeshAfterDeformation() {
            meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
        }

        public void SetVisible(bool visible) {
            meshObject.SetActive(visible);
        }

        public bool IsVisible() {
            return meshObject.activeSelf;
        }
    }

    //class used to hold the meshes of differed LODs
    public class LODMesh {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        private int lod;
        public event System.Action updateCallback;

        public LODMesh(int lod) {
            this.lod = lod;
        }

        //once the mesh data had been generated create the mesh from it
        private void OnMeshDataRecieved(MeshData meshData) {
            mesh = meshData.CreateMesh();
            hasMesh = true;
            updateCallback();
        }

        //request that mesh data be created from the given map data with the correct LOD
        public void RequestMesh(MapData mapData) {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataRecieved);
        }
    }

    //struct to hold data relating to the Levels of Detail and their influence based on the distance from the viewer
    [System.Serializable]
    public struct LODInfo {
        [Range(0,ProceduralMeshGenerator.numSupportedLODs-1)]
        public int lod;
        public float visibleDistanceThreshold;

        public float sqrVisibleDistanceThreshold {
            get {
                return visibleDistanceThreshold * visibleDistanceThreshold;
            }
        }
    }
}
