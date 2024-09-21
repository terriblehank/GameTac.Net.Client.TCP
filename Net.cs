using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameTac.Net.Client;

public class Net : MonoBehaviour
{
    private void Awake()
    {
        Dashboard.Instance.LoadProtocols();
    }

    // Start is called before the first frame update
    void Start()
    {
        Dashboard.Instance.Connect("localhost", 8080);
    }

    // Update is called once per frame
    void Update()
    {
        Dashboard.Instance.Update();
    }
}
