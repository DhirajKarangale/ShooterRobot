using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] GameObject explosion;

    private void Update()
    {
        if (player)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(player.position - transform.position), 2 * Time.deltaTime);
        }
    }

    void Damage()
    {
        Destroy(Instantiate(explosion, transform.position, Quaternion.identity), 1);
        Destroy(gameObject);
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.name == "Muzzle")
        {
            Damage();
        }
    }
}
