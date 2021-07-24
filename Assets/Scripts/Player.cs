using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public Item itemInHand;
    public float interactRange = 8f;
    public TerrainDeformer terrainDeformer;
    public InfiniteTerrain infiniteTerrain;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            Debug.Break();
        } else if(Input.GetMouseButtonDown(0)) {

            itemInHand = Inventory.instance.Items[Inventory.instance.activeHotbarSlot - 1];
            Click();
        } else if(Input.GetMouseButtonDown(1)) {
            Ray ray;
            RaycastHit hit;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if(Physics.Raycast(ray, out hit)) {
                TerrainCoordHolder tch = hit.transform.gameObject.GetComponent<TerrainCoordHolder>();
                MeshFilter meshFilter = hit.transform.gameObject.GetComponent<MeshFilter>();
                MeshCollider meshCollider = hit.transform.gameObject.GetComponent<MeshCollider>();
                if(tch != null) {
                    Debug.Log("deforming mesh");
                    List<int> vertextIndices = terrainDeformer.GetListOfVertexIndicesAroundShapeAtPoint(hit.point, TerrainDeformer.DeformationShape.CIRCLE, 1);
                    Mesh mesh = infiniteTerrain.terrainChunkDict[tch.terrainCoord].lodMeshes[0].mesh;
                    mesh = terrainDeformer.DeformMesh(vertextIndices, mesh, 2);
                    infiniteTerrain.terrainChunkDict[tch.terrainCoord].lodMeshes[0].mesh = mesh;
                    infiniteTerrain.terrainChunkDict[tch.terrainCoord].lodMeshes[0].mesh.RecalculateNormals();
                    infiniteTerrain.terrainChunkDict[tch.terrainCoord].UpdateCollisionMeshAfterDeformation();
                }

            }
        }

    }

    private void OnTriggerEnter(Collider other) {
        
        //if we collided with a dropped item
        if(other.gameObject.GetComponent<DroppedItem>()) {
            Debug.Log("on trigger enter");
            if(Inventory.instance.AddItem(other.gameObject.GetComponent<DroppedItem>().item, 1)) {
                Destroy(other.gameObject);
            }
        }
    }

    public void Click() {
        Ray ray;
        RaycastHit hit;

        if(Inventory.instance.inMenu) {return;}
        //get a ray coming out from the mouse (in our case the middle of the screen)
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //if the ray hits something
        if(Physics.Raycast(ray, out hit))
        {   
            Debug.Log(hit.transform.name);
            Interactable interactedWith = hit.transform.GetComponent<Interactable>();
            if(interactedWith != null && hit.distance <= interactRange) {
                Vector3 closerHitPoint = ((hit.distance * .95f) * ray.direction) + transform.position;
                if(itemInHand == null) {
                    interactedWith.Interact(closerHitPoint, 0, equipmentType.ALL);
                } else if(itemInHand.equipment) {
                    interactedWith.Interact(closerHitPoint, itemInHand.itemTier + 1, itemInHand.typeOfEquipment);
                }
                
            }
         }
    }
}
