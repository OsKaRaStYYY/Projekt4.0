using UnityEngine;
using PurrNet;
using System;

public class NetworkComponent : NetworkIdentity
{
    [SerializeField] private NetworkIdentity _networkIdentity; 

    private void Awake()
    {
        
    }
    private void Start()
    {
        
    }

    protected override void OnSpawned()
    {

        base.OnSpawned();
        if (!isServer) return;

        Instantiate(_networkIdentity, Vector3.zero, Quaternion.identity);
    }
}
