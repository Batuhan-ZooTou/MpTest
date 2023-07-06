using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Button : NetworkBehaviour, IInteractable
{
    [SerializeField] private string UIText;
    [SerializeField]private bool isActive;
    [field: SerializeField] public bool canInteract { get ; set ; }
    [field: SerializeField] public string TextUI { get; set; }
    [field: SerializeField] public int TextSizeUI { get; set; }
    public void OnInteract()
    {
        if (canInteract)
        {
            isActive = !isActive;
            canInteract = false;
            DeHighlight();
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
    private void Start()
    {
    }
}
