using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PickUpScript : MonoBehaviour
{
    public GameObject player;
    public Transform holdPosition;
    public GameObject door;
    public activePanel activePanelReferance;

    //if you copy from below this point, you are legally required to like the video
    public float throwForce = 500f; //force at which the object is thrown at
    public float pickUpRange = 8f; //how far the player can pickup the object from
    [SerializeField]
    private float doorOpenRange = 12f; //how far away the player needs to be in order to open or close the door
    private float rotationSensitivity = 1f; //how fast/slow the object is rotated in relation to mouse movement
    private GameObject heldObj; //object which we pick up
    private Rigidbody heldObjRb; //rigidbody of object we pick up
    private bool canDrop = true; //this is needed so we don't throw/drop object when rotating the object
    private int LayerNumber; //layer index

    private Vector3 defaultHeldObjectScale = Vector3.zero;

    //Reference to script which includes mouse movement of player (looking around)
    //we want to disable the player looking around when rotating the object
    //example below 
    //MouseLookScript mouseLookScript;
    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(0f, 0f, 1f, 0.5f);
        }

        LayerNumber = LayerMask.NameToLayer("holdLayer"); //if your holdLayer is named differently make sure to change this ""

        //mouseLookScript = player.GetComponent<MouseLookScript>();
       
    }

    void Update()
    {
    }

    public void toggleDoorState()
    {
        if (PauseMenu.isGamePause) return;

        //Debug.Log("toggling...");
        if (heldObj == null) //if currently not holding anything
        {
            Debug.Log("NULL PASS");
            //perform raycast to check if player is looking at object within interaction range
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, doorOpenRange))
            {
                Debug.Log("RAY PASS");
                //make sure pickup tag is attached
                if (hit.transform.gameObject.tag == "Door")
                {
                    Debug.Log("DOOR FOUND!!");

                    doorMovement targetDoor = hit.transform.GetComponent<doorMovement>();

                    // If the script exists on that door, run its Interact method!
                    if (targetDoor != null)
                    {
                        targetDoor.ToggleDoor();
                    }
                }
            }
        }
    }

    public void runPickUpObject()
    {
        //Debug.Log("running pick up method");

        if (heldObj == null) //if currently not holding anything
        {
            //perform raycast to check if player is looking at object within pickuprange
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, pickUpRange))
            {
                //make sure pickup tag is attached
                if (hit.transform.gameObject.tag == "canPickUp")
                {
                    //pass in object hit into the PickUpObject function

                    defaultHeldObjectScale = hit.transform.localScale;
                    PickUpObject(hit.transform.gameObject);
                }
            }
        }
        else
        {
            if (canDrop == true)
            {
                StopClipping(); //prevents object from clipping through walls
                //DropObject();
            }
        }
    }

    public void runThrowObject()
    {
        if (PauseMenu.isGamePause) return;

        if (heldObj != null) //if player is holding object
        {
            MoveObject(); //keep object position at holdPos
            RotateObject();
            if (canDrop == true)
            {
                StopClipping();
                ThrowObject();
            }
        }
    }

    // New helper method to safely handle collision bypassing for either standard Colliders OR CharacterControllers
    private void SetIgnoreCollisionWithPlayer(GameObject target, bool ignore)
    {
        if (player == null || target == null) return;

        Collider targetCollider = target.GetComponent<Collider>();
        if (targetCollider == null) return;

        Collider playerCollider = player.GetComponent<Collider>();

        if (playerCollider == null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null)
            {
                Physics.IgnoreCollision(targetCollider, cc, ignore);
                return;
            }
        }

        if (playerCollider != null)
        {
            Physics.IgnoreCollision(targetCollider, playerCollider, ignore);
        }
    }

    public class HotbarItem
    {
        public GameObject heldObject; // the real pickup object
        public Sprite icon;
    }

    public List<GameObject> hotbarSlots;
   
    public Sprite emptySlotSprite;

    public HotbarItem[] hotbarItems = new HotbarItem[5];

    void PickUpObject(GameObject pickUpObj)
    {
        if (PauseMenu.isGamePause) return;

        Debug.Log("Running method and found " + pickUpObj.name);

        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            GameObject slot = hotbarSlots[i];
            Image slotImage = slot.GetComponentInChildren<Image>(true);
            if (slotImage == null) continue;

            if (slotImage.sprite == emptySlotSprite)
            {
                slot.SetActive(true);

                Sprite[] allSprites = Resources.LoadAll<Sprite>("Items/sprites/" + pickUpObj.name);
                Sprite itemSprite = System.Array.Find(allSprites, s => s.name == pickUpObj.name + "_0");

                if (itemSprite != null)
                {
                    slotImage.sprite = itemSprite;
                    pickUpObj.SetActive(false);
                    Debug.Log("Added " + pickUpObj.name + " to hotbar.");

                    hotbarItems[i] = new HotbarItem
                    {
                        heldObject = pickUpObj,
                        icon = itemSprite
                    };

                    break;
                }
                else
                {
                    Debug.LogWarning("No sprite found for: " + pickUpObj.name);
                }
            }
        }
    }

    public void DropSelectedSlot()
    {
        if (PauseMenu.isGamePause) return;

        int index = activePanelReferance.SelectedIndex;
        HotbarItem item = hotbarItems[index];

        if (item == null || item.heldObject == null)
        {
            Debug.Log("Empty slot");
            return;
        }

        DropFromHotbar(index);
    }

    public void DropFromHotbar(int index)
    {
        HotbarItem item = hotbarItems[index];
        if (item == null || item.heldObject == null) return;

        // Reactivate at a position in front of the player
        item.heldObject.transform.position = holdPosition.position;
        item.heldObject.SetActive(true);

        // Clear the slot visually and in data
        Image slotImage = hotbarSlots[index].GetComponentInChildren<Image>(true);
        slotImage.sprite = emptySlotSprite;
        hotbarItems[index] = null;

        hotbarSlots[index].SetActive(false);
    }

    void MoveObject()
    {
        //keep object position the same as the holdPosition position
        heldObj.transform.position = holdPosition.transform.position;
    }

    void RotateObject()
    {
        if (Input.GetKey(KeyCode.R))//hold R key to rotate, change this to whatever key you want
        {
            canDrop = false; //make sure throwing can't occur during rotating

            //disable player being able to look around
            //mouseLookScript.verticalSensitivity = 0f;
            //mouseLookScript.lateralSensitivity = 0f;

            float XaxisRotation = Input.GetAxis("Mouse X") * rotationSensitivity;
            float YaxisRotation = Input.GetAxis("Mouse Y") * rotationSensitivity;
            //rotate the object depending on mouse X-Y Axis
            heldObj.transform.Rotate(Vector3.down, XaxisRotation);
            heldObj.transform.Rotate(Vector3.right, YaxisRotation);
        }
        else
        {
            //re-enable player being able to look around
            //mouseLookScript.verticalSensitivity = originalvalue;
            //mouseLookScript.lateralSensitivity = originalvalue;
            canDrop = true;
        }
    }

    void ThrowObject()
    {
        //same as drop function, but add force to object before undefining it
        SetIgnoreCollisionWithPlayer(heldObj, false);
        heldObj.layer = 0;
        heldObjRb.isKinematic = false;
        heldObj.transform.parent = null;
        heldObjRb.AddForce(transform.forward * throwForce);
        heldObj = null;
    }

    void StopClipping() //function only called when dropping/throwing
    {
        var clipRange = Vector3.Distance(heldObj.transform.position, transform.position); //distance from holdPos to the camera
        //have to use RaycastAll as object blocks raycast in center screen
        //RaycastAll returns array of all colliders hit within the cliprange
        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, transform.TransformDirection(Vector3.forward), clipRange);
        //if the array length is greater than 1, meaning it has hit more than just the object we are carrying
        if (hits.Length > 1)
        {
            //change object position to camera position 
            heldObj.transform.position = transform.position + new Vector3(0f, -0.5f, 0f); //offset slightly downward to stop object dropping above player 
            //if your player is small, change the -0.5f to a smaller number (in magnitude) ie: -0.1f
        }
    }
}