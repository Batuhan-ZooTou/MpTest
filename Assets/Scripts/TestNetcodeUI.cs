using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class TestNetcodeUI : MonoBehaviour
{
    public void Host()
    {
        NetworkManager.Singleton.StartHost();
        gameObject.SetActive(false);
    }
    public void Client()
    {
        NetworkManager.Singleton.StartClient();
        gameObject.SetActive(false);
    }
}
