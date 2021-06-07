using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Animator animator;
    private AudioSource audioSource;
    [SerializeField] GameObject explosion;
    [SerializeField] float speed = 1f;
    private bool canMove = true,canShoot;
    [SerializeField] ParticleSystem rightMuzzle, leftMuzzle, rightFire, leftFire;
    private ParticleSystem.EmissionModule rightMuzzleEmission, leftMuzzleEmission, rightFireEmission, leftFireEmission;
    [SerializeField] int health = 100;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        animator.Play("Walk");
        audioSource = GetComponent<AudioSource>();

        rightMuzzleEmission = rightMuzzle.emission;
        rightMuzzleEmission.rateOverTime = 0;

        leftMuzzleEmission = leftMuzzle.emission;
        leftMuzzleEmission.rateOverTime = 0;

        rightFireEmission = rightFire.emission;
        rightFireEmission.rateOverTime = 0;

        leftFireEmission = leftFire.emission;
        leftFireEmission.rateOverTime = 0;
    }

    private void Update()
    {
        Move();
        CheckToShoot();
    }

    private void Move()
    {
        if (canMove)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
            if(!isGrounded() || CheckFront())
            {
                animator.Play("Idle");
                canMove = false;
                LeanTween.rotateAroundLocal(gameObject, Vector3.up, 180, 0.5f).setOnComplete(CompletedMove);
            }
        }
    }

    bool isGrounded()
    {
        return Physics.Raycast(transform.position + transform.forward * 0.4f + transform.up * 0.1f, Vector3.down, 0.1f);
    }

    bool CheckFront()
    {
        return Physics.Raycast(transform.position + transform.forward * 0.4f + transform.up * 0.5f, Vector3.forward, 0.1f);
    }

    private void CompletedMove()
    {
        animator.Play("Walk");
        canMove = true;
    }

    private void CheckToShoot()
    {
        if (canShoot)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
            rightMuzzleEmission.rateOverTime = leftMuzzleEmission.rateOverTime = 10;
            rightFireEmission.rateOverTime = leftFireEmission.rateOverTime = 30;
        }
        else
        {
            audioSource.Stop();
            rightMuzzleEmission.rateOverTime = leftMuzzleEmission.rateOverTime = 0;
            rightFireEmission.rateOverTime = leftFireEmission.rateOverTime = 0;
          //  leftLight.intensity = rightLight.intensity = 0;
        }
    }

    void Destroy()
    {
        Destroy(Instantiate(explosion, transform.position, Quaternion.identity),1);
        Destroy(gameObject);
    }

    private void OnParticleCollision(GameObject other)
    {
        if(other.name == "Muzzle")
        {
            if (health <= 0) Destroy();
            else health -= 4;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((other.gameObject.name == "Robot") && !Player.isPlayerDead)
        {
            canShoot = true;
            canMove = false;
            animator.Play("Idle");
        }
        else
        {
            canShoot = false;
            canMove = true;
            animator.Play("Walk");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "Robot")
        {
            canShoot = false;
            canMove = true;
            animator.Play("Walk");
        }

    }
}
