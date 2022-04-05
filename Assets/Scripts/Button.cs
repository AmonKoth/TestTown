using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{

    [SerializeField]
    private Light _light;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            _light.gameObject.SetActive(true);
        }
        Invoke("TurnOffTheLight", 4);
    }

    private void TurnOffTheLight()
    {
        _light.gameObject.SetActive(false);
    }


    private void Start()
    {
        TurnOffTheLight();
    }
}
