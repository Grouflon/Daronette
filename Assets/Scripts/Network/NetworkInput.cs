using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkInput : NetworkBehaviour
{
    public NetworkVariable<float> xAxis = new NetworkVariable<float>();
    public NetworkVariable<float> yAxis = new NetworkVariable<float>();
    public NetworkVariable<bool> button1 = new NetworkVariable<bool>();
    public NetworkVariable<bool> button2 = new NetworkVariable<bool>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
