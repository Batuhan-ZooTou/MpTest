using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObjectGrabable
{
    public void Grab(Transform position);
    public void Drop();
    public void Throw(Transform dir,float force);
    public bool IsGrounded();

}

public interface IInteractable
{
    public string TextUI { get; set; }
    public int TextSizeUI { get; set; }
    public bool canInteract { get;  set; }
    public void Highlight();
    public void DeHighlight();
    public void OnInteract();
}
