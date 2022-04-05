using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Will use when shooting for AI is implemented
public class Bullet : MonoBehaviour
{

    [SerializeField]
    private float dmg = 10;
    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.tag == "Player")
        {
            PlayerController player = other.transform.GetComponent<PlayerController>();
            player.RecieveDamage(dmg);
        }
        Destroy(this.gameObject);

    }
}

