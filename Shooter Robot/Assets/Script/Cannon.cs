using UnityEngine;

public class Cannon : MonoBehaviour
{
    [SerializeField] ParticleSystem impactEffect;
    private AudioSource audioSource;
    [SerializeField] Transform player;
    [SerializeField] GameObject explosion;
    private bool canShoot;
    [SerializeField] ParticleSystem muzzle, fire;
    private ParticleSystem.EmissionModule muzzleEmission, fireEmission;
    private float distanseBetwwenEnemyAndPlayer;
    [SerializeField] int health = 200;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        muzzleEmission = muzzle.emission;
        muzzleEmission.rateOverTime = 0;

        fireEmission = fire.emission;
        fireEmission.rateOverTime = 0;
    }

    private void Update()
    {
        distanseBetwwenEnemyAndPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (player)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(player.position - transform.position), 2 * Time.deltaTime);
        }
        if ((distanseBetwwenEnemyAndPlayer < 10) && !Player.isPlayerDead) canShoot = true;
        else canShoot = false;
        Shoot();
    }

    void Destroy()
    {
        Destroy(Instantiate(explosion, transform.position, Quaternion.identity), 1);
        Destroy(gameObject);
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.name == "Muzzle")
        {
            impactEffect.Play();
            if (health <= 0) Destroy();
            else health -= 4;
        }
    }

    private void Shoot()
    {
        if (canShoot)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
                // StartCoroutine("LightControl");
            }
            muzzleEmission.rateOverTime = 10;
            fireEmission.rateOverTime = 30;
        }
        else
        {
            audioSource.Stop();
            muzzleEmission.rateOverTime = 0;
            fireEmission.rateOverTime = 0;
            //  leftLight.intensity = rightLight.intensity = 0;
            //    StopCoroutine("LightControl");
        }
    }
}
