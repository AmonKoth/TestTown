using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleBase : MonoBehaviour
{
    protected PlayerController _player;

    private void OnTriggerEnter(Collider other)
    {
        Collect(other.gameObject);
    }

    private void Update()
    {
        Rotator();
    }
    //To create a simple animations
    private void Rotator()
    {
        transform.Rotate(new Vector3(15.0f, 30.0f, 45.0f) * Time.deltaTime);
    }

    virtual protected void Collect(GameObject connect)
    {
        _player = connect.transform.GetComponent<PlayerController>();
        if (_player)
        {
            UIComponent _playerUI = _player.GetComponent<UIComponent>();
            _playerUI.AddPoints();
            Destroy(gameObject);

        }
    }
}
