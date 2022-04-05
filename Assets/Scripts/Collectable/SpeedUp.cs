using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedUp : CollectibleBase
{

    [SerializeField]
    private float _powerUpTime = 5.0f;

    private void OnTriggerEnter(Collider other)
    {
        Collect(other.gameObject);
    }

    private void ResetStatus()
    {
        _player.SwitchPartcileLock();
        Destroy(gameObject);
    }

    override protected void Collect(GameObject connect)
    {
        _player = connect.transform.GetComponent<PlayerController>();
        if (_player.GetParticleLock())
        {
            _player.SwitchPartcileLock();
            //Little cheating?
            this.transform.position = new Vector3(-50000, -50000, -5000);

            Invoke("ResetStatus", _powerUpTime);
        }
    }

}
