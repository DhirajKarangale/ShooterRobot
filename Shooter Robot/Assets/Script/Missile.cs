using UnityEngine;

public class Missile : MonoBehaviour
{
    [SerializeField] GameObject explosion;

    void LaunchMissile(Vector3 targetPosition)
    {
        Invoke("SetActive",1);

        LeanTween.move(gameObject, targetPosition, 1.5f).setEase(LeanTweenType.easeInBack).setOnComplete(Explode);
    }

    private void Explode()
    {
        Destroy(Instantiate(explosion, transform.position, Quaternion.identity), 1);
        Destroy(gameObject);
    }

    private void SetActive()
    {
        GetComponent<Collider>().enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if(other.tag == "Box") other.GetComponent<Rigidbody>().AddExplosionForce(2010, transform.position, 11);
            else if(other.tag == "Enemy") other.SendMessage("Destroy");
            Explode();
        }
    }
}
