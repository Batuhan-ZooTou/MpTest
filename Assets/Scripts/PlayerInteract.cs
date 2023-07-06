using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using TMPro;
using Unity.Netcode;

public class PlayerInteract : NetworkBehaviour
{
    public static PlayerInteract Instance { get; private set; }

    [SerializeField] private Transform cam;
    [SerializeField] public Transform objectGrabPointTransform;
    [SerializeField] private Collider player;
    [SerializeField] public PhysicObject grabbedObject;
    [SerializeField] private IInteractable Interactable;

    [Header("ObjectGrabVariables")]
    [SerializeField] private float objectGrabPointAdjustSpeed;
    [SerializeField] private float interactDistance;
    [SerializeField] private float holdDistance;
    [SerializeField] private float throwForce;

    [Header("LayerMasks")]
    [SerializeField] private LayerMask interactables;
    [SerializeField] private LayerMask grabables;
    [SerializeField] private LayerMask solidLayerMask;
    [Header("CursorUI")]
    [SerializeField] GameObject CursorUI;
    [SerializeField] GameObject InteractUI;
    [SerializeField] TextMeshProUGUI InteractTextUI;

    private Vector3 defaultGrabPoint;
    Ray ray;
    RaycastHit hit;


    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        cam = PlayerCamera.Instance.transform;
    }
    //Inputs
    #region 
    public void OnScrollWheel(InputAction.CallbackContext context)
    {
        if (grabbedObject!=null)
        {
            float mouseScrollY = -context.ReadValue<Vector2>().y;
            if (mouseScrollY > 0f && Vector3.Distance(objectGrabPointTransform.position, cam.position) <= interactDistance) // forward
            {
                objectGrabPointTransform.position += cam.forward * Time.deltaTime * objectGrabPointAdjustSpeed * 0.25f;
                if (Vector3.Distance(objectGrabPointTransform.position, cam.position) > interactDistance)
                {
                    objectGrabPointTransform.position = cam.position + cam.forward * interactDistance;
                }
            }
            else if (mouseScrollY < 0f && Vector3.Distance(objectGrabPointTransform.position, cam.position) >= holdDistance) // backwards
            {
                objectGrabPointTransform.position -= cam.forward * Time.deltaTime * objectGrabPointAdjustSpeed * 0.25f;
                if (Vector3.Distance(objectGrabPointTransform.position, cam.position) < holdDistance)
                {
                    objectGrabPointTransform.position = cam.position + cam.forward * holdDistance;
                }
            }
        }
    }
    public void OnLMB(InputAction.CallbackContext context)
    {
        if (!IsOwner)
        {
            return;
        }
        if (context.started)
        {
            //dropping with lmb
            if (grabbedObject != null)
            {
                Physics.IgnoreCollision(player, grabbedObject.GetComponent<Collider>(), false);
                grabbedObject.DropServerRpc();
                grabbedObject = null;
            }
            else if (Interactable != null)
            {
                //if hits grabable object
                if (Physics.Raycast(ray, out hit, interactDistance, grabables))
                {
                    if (hit.transform.TryGetComponent(out grabbedObject))
                    {
                        Physics.IgnoreCollision(player, grabbedObject.GetComponent<Collider>(), true);
                        objectGrabPointTransform.position = defaultGrabPoint;
                        grabbedObject.GrabServerRpc(NetworkObject);

                    }
                }
            }
        }
    }

    public void OnRMB(InputAction.CallbackContext context)
    {
        if (!IsOwner)
        {
            return;
        }
        if (context.started)
        {
            //throwing with rmb
            if (grabbedObject != null)
            {
                grabbedObject.Throw(cam, throwForce);
                Physics.IgnoreCollision(player, grabbedObject.GetComponent<Collider>(), false);
                grabbedObject.DropServerRpc();
                grabbedObject = null;
            }
        }
    }
    public void Interact(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            //if hits interacable
            Interactable?.OnInteract();
        }
    }
    #endregion
    void Update()
    {
        AdjuctGrabPoint();
        CheckForInteractables();
        //UpdateCursor();
        defaultGrabPoint = cam.position + cam.forward * holdDistance;
    }
    public void AdjuctGrabPoint()
    {
        if (grabbedObject != null)
        {
            //adjusting the point while near walls
            if (Physics.Raycast(cam.position, cam.forward, out RaycastHit raycastHit, Mathf.Clamp(Vector3.Distance(objectGrabPointTransform.position, cam.position),holdDistance,interactDistance) + grabbedObject.HalfSize, solidLayerMask))
            {
                grabbedObject.snapped = true;
                objectGrabPointTransform.position = raycastHit.point + (cam.forward * -grabbedObject.HalfSize);
            }
            else
            {
                grabbedObject.snapped = false;
                if (Vector3.Distance(objectGrabPointTransform.position, cam.position) < holdDistance)
                {
                    objectGrabPointTransform.position = cam.position + cam.forward * holdDistance;
                }
            }
        }
    }
    private void UpdateCursor()
    {
        if (Interactable!=null)
        {
            if (Interactable.canInteract)
            {
                CursorUI.SetActive(false);
                InteractUI.SetActive(true);
                InteractTextUI.text = Interactable.TextUI;
                InteractTextUI.fontSize= Interactable.TextSizeUI;
            }
            else
            {
                CursorUI.SetActive(true);
                InteractUI.SetActive(false);
            }
        }
        else
        {
            CursorUI.SetActive(true);
            InteractUI.SetActive(false);
        }
    }
    void CheckForInteractables()
    {
        //if player holding object
        if (grabbedObject != null)
        {
            //for possiblle erros
            IInteractable pastInteractable = Interactable;
            Interactable = grabbedObject.GetComponent<IInteractable>();
            if (pastInteractable != Interactable)
            {
                pastInteractable.DeHighlight();
            }
            Interactable.Highlight();
        }
        //if player holding nothing
        else
        {
            //Raycasting
            ray = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out hit, interactDistance, interactables))
            {
                IInteractable pastInteractable;
                //if there is already interactable
                if (Interactable!=null)
                {
                    //check if 2 object is not highlighted at the same time
                    pastInteractable = Interactable;
                    hit.transform.TryGetComponent(out Interactable);
                    //if its not same interacable
                    if (pastInteractable != Interactable)
                    {
                        pastInteractable.DeHighlight();
                        Interactable.Highlight();
                    }
                    //if its same interactable
                    else if (pastInteractable == Interactable)
                    {
                        //Interactable.Highlight();
                    }
                    return;
                }
                //if there were no interactable before
                hit.transform.TryGetComponent(out Interactable);
                Interactable.Highlight();
                //There is Interactable object
            }
            //if there is no interactable
            else if (Interactable != null)
            {
                Interactable.DeHighlight();
                Interactable = null;
            }
        }
    }
    private void OnDrawGizmos()
    {
        // Draw a yellow cube at the transform position
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(objectGrabPointTransform.position, 0.05f);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(cam.transform.position, cam.transform.forward);
    }
}
