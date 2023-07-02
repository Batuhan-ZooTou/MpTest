using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicObject : MonoBehaviour, IObjectGrabable, IInteractable
{
    [SerializeField] private Rigidbody objectRigidbody;
    [SerializeField] private Transform objectGrabPointTransform;
    [SerializeField] private float moveSpeed;
    public bool snapped;
    public float HalfSize;
    [Header("PhysicObjectGrounded")]
    [SerializeField] private bool Grounded = true;
    [SerializeField] private float GroundedDistance = 0.001f;
    [SerializeField] private Vector3 halfExtent;
    [SerializeField] private LayerMask GroundLayers;
    //Interface
    #region
    [field: SerializeField] public bool canInteract { get; set; }
    public void Drop()
    {
        objectGrabPointTransform = null;
        objectRigidbody.useGravity = true;
    }
    public void Grab(Transform position)
    {
        this.objectGrabPointTransform = position;
        objectRigidbody.useGravity = false;
    }
    public void Throw(Transform playerCameraTransform, float throwForce)
    {
        objectRigidbody.AddForce(playerCameraTransform.forward * throwForce);
    }
    public bool IsGrounded()
    {
        return Grounded;
    }
    public void OnInteract()
    {
        if (canInteract)
        {
            //do something
        }
    }
    public void Highlight()
    {
        if (canInteract)
        {
            transform.GetComponent<MeshRenderer>().material.SetFloat("_Scale", 1.03f);
        }
    }
    public void DeHighlight()
    {
        transform.GetComponent<MeshRenderer>().material.SetFloat("_Scale", 0f);
    }
    #endregion
    void Start()
    {
        halfExtent = new Vector3(HalfSize, 0.01f, HalfSize);
        GroundedDistance = HalfSize;
    }
    void Update()
    {
        GroundedCheck();
    }
    private void FixedUpdate()
    {
        //moves object to grabpoint
        if (objectGrabPointTransform != null)
        {
            Vector3 DirectionToPoint = objectGrabPointTransform.position - transform.position;
            float DistanceToPoint = DirectionToPoint.magnitude;
            objectRigidbody.velocity = DirectionToPoint.normalized * moveSpeed * DistanceToPoint * Time.fixedDeltaTime;
        }
    }
    private void GroundedCheck()
    {
        //if its held 
        if (objectGrabPointTransform != null)
        {
            Grounded = false;
            return;
        }
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        RaycastHit[] groundHits = Physics.BoxCastAll(spherePosition, halfExtent, Vector3.down, Quaternion.Euler(Vector3.zero), GroundedDistance, GroundLayers, QueryTriggerInteraction.Ignore);
        //there is no collider
        if (groundHits.Length == 0)
        {
            Grounded = false;
        }
        //only 1 collider
        else if (groundHits.Length == 1)
        {
            if (groundHits[0].collider.TryGetComponent(out IObjectGrabable physicObject))
            {
                if (!physicObject.IsGrounded())
                {
                    Grounded = false;
                    return;
                }
                //self
                if (groundHits[0].collider.gameObject == this.gameObject)
                {
                    Grounded = false;
                    return;
                }
                Grounded = true;
            }
            else
            {
                Grounded = true;
            }
        }
        else
        {
            foreach (var hit in groundHits)
            {
                if (hit.collider.TryGetComponent(out IObjectGrabable physicObject))
                {
                    if (hit.collider.gameObject == this.gameObject)
                    {
                        continue;
                    }
                    if (physicObject.IsGrounded())
                    {
                        Grounded = true;
                    }
                    else
                    {
                        Grounded = false;
                    }
                }
                else
                {
                    Grounded = true;
                    return;
                }
            }
        }
    }
    //Gizmos
    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawRay(transform.position, Vector3.down * GroundedDistance);
        Gizmos.DrawCube(new Vector3(transform.position.x, transform.position.y - GroundedDistance, transform.position.z), halfExtent * 2);
    }
}
