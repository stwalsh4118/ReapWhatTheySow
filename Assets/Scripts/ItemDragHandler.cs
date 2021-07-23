using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{   

    public RectTransform rectTransform;
    public CanvasGroup canvasGroup;
    [SerializeField]
    private Canvas canvas;
    public bool canvasScale = false;
    public Image image;
    public Vector2 initialPosition;
    public enum Parent {
        Hotbar,
        Inventory
    }


    public Parent parent;
    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        image = GetComponent<Image>();
        canvas = GameObject.Find("Inventory").GetComponent<Canvas>();
        initialPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData) {
        if(parent == Parent.Hotbar) {
            GameObject.Find("Hotbar").transform.SetAsLastSibling();
        } else {
            GameObject.Find("InventoryMenu").transform.SetAsLastSibling();
        }
        canvasGroup.blocksRaycasts = false;
        image.maskable = false;
        Inventory.instance.mostRecentDragStart = transform.parent.GetComponent<InventoryPosition>().inventoryPosition;
    }

    //when click and drag an item move it so it follows the mouse cursor
    public void OnDrag(PointerEventData eventData)
    {
        if(canvasScale) {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        } else {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor * 1.25f;
        }
    } 

    //when stopping the click and drag put it back in its place (we will do other things later)
    public void OnEndDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition = initialPosition;
        canvasGroup.blocksRaycasts = true;
        image.maskable = true;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
