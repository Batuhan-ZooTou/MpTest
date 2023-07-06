using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class PlayerCamera : MonoBehaviour
{
    public static PlayerCamera Instance { get; private set; }
    public CinemachineVirtualCamera cinemachineVirtual;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetFollowTarger(Transform target)
    {
        cinemachineVirtual.Follow = target;
    }
}
