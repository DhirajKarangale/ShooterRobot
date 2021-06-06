using System.Collections;
using UnityEngine;

enum Direction
{
    Left,
    Right
}

public class Player : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rigidbody;
    private ConstantForce constantForce;
    private AudioSource audioSource;
    private Direction direction = Direction.Right;

    [SerializeField] GameObject missile;
    [SerializeField] ParticleSystem rightMuzzle, leftMuzzle, rightFire, leftFire, boost;
    [SerializeField] Transform leftArm, rightArm;
    [SerializeField] Transform missilePoint;
    [SerializeField] Light leftLight, rightLight;
    [SerializeField] float speed = 4f;

    private ParticleSystem.EmissionModule rightMuzzleEmission, leftMuzzleEmission, rightFireEmission, leftFireEmission;
    private ParticleSystem.MainModule boostEmission;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        rigidbody = GetComponent<Rigidbody>();
        constantForce = rigidbody.GetComponent<ConstantForce>();
        animator = GetComponentInChildren<Animator>();

        rightMuzzleEmission = rightMuzzle.emission;
        rightMuzzleEmission.rateOverTime = 0;

        leftMuzzleEmission = leftMuzzle.emission;
        leftMuzzleEmission.rateOverTime = 0;

        rightFireEmission = rightFire.emission;
        rightFireEmission.rateOverTime = 0;

        leftFireEmission = leftFire.emission;
        leftFireEmission.rateOverTime = 0;

        boostEmission = boost.main;
               
    }

    private void FixedUpdate()
    {
        // Move Left Right
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (!LeanTween.isTweening(gameObject))
            {
                if (isGrounded()) animator.Play("Walk");
                else animator.Play("Idle");

                if (direction != Direction.Left) LeanTween.rotateAroundLocal(gameObject, Vector3.up, 180, 0.3f).setOnComplete(TurnLeft);
                else transform.Translate(Vector3.forward * speed * Time.deltaTime);
            }
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            if (!LeanTween.isTweening(gameObject))
            {
                if (isGrounded()) animator.Play("Walk");
                else animator.Play("Idle");

                if (direction != Direction.Right) LeanTween.rotateAroundLocal(gameObject, Vector3.up, -180, 0.3f).setOnComplete(TurnRight);
                else transform.Translate(Vector3.forward * speed * Time.deltaTime);
            }
        }
        else
        {
            animator.Play("Idle");
        }

        // Rotate Arm
        if (Input.GetKey(KeyCode.UpArrow))
        {
            rightArm.Rotate(Vector3.back * 200 * Time.deltaTime);
            leftArm.Rotate(Vector3.back * 200 * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            rightArm.Rotate(Vector3.forward * 200 * Time.deltaTime);
            leftArm.Rotate(Vector3.forward * 200 * Time.deltaTime);
        }
        
        // Fly
        if (Input.GetKey(KeyCode.Z))
        {
            constantForce.force = Vector3.zero;
            if (rigidbody.velocity.y < 4f) rigidbody.AddRelativeForce(Vector3.up * 20);
         
            if (!boostEmission.loop)
            {
                boost.Play();
                boostEmission.loop = true;
            }
        }
        else
        {
            constantForce.force = new Vector3(0, -10, 0);
            boostEmission.loop = false;
        }

        // Shoot
        if (Input.GetKey(KeyCode.X))
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
                StartCoroutine("LightControl");
            }
            rightMuzzleEmission.rateOverTime = leftMuzzleEmission.rateOverTime = 10;
            rightFireEmission.rateOverTime = leftFireEmission.rateOverTime = 30;
        }
        else
        {
            audioSource.Stop();
            rightMuzzleEmission.rateOverTime = leftMuzzleEmission.rateOverTime = 0;
            rightFireEmission.rateOverTime = leftFireEmission.rateOverTime = 0;
            leftLight.intensity = rightLight.intensity = 0;
            StopCoroutine("LightControl");
        }

        // Missile
        if (Input.GetKeyDown(KeyCode.C))
        {
            LaunchMissile();
        }

        bool isGrounded()
        {
            return Physics.Raycast(transform.position + transform.forward * 0.4f + transform.up * 0.1f, Vector3.down, 0.1f);
        }

        void TurnLeft()
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);
            direction = Direction.Left;
        }
        void TurnRight()
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);
            direction = Direction.Right;
        }
    }

    IEnumerator LightControl()
    {
        while (true)
        {
            leftLight.intensity = rightLight.intensity = 1;
            yield return new WaitForSeconds(0.3f);
            leftLight.intensity = rightLight.intensity = 0;
            yield return new WaitForSeconds(0.3f);
        }
    }

    private void LaunchMissile()
    {
        if (!LeanTween.isTweening(gameObject))
        {
            Vector3 position = transform.position;

            if(direction == Direction.Right)
            {
                position.x += 1;
                position.y += 1;
            }

            if (direction == Direction.Left)
            {
                position.x -= 1;
                position.y += 1;
            }

            for(int i = 0; i < 5; i++)
            {
                Vector3 origin = position + Vector3.up * Random.Range(-1, 1) + Vector3.left * Random.Range(-1, 1);
                GameObject currentMissile = Instantiate(missile, origin, Quaternion.AngleAxis(direction == Direction.Right ? 0 : 180, Vector3.up)) as GameObject;
                Vector3 targetPosition = missilePoint.position + missilePoint.forward * 20 + missilePoint.up * Random.Range(-1, 1);
                currentMissile.SendMessage("LaunchMissile", targetPosition);
            }
        }
         
       
    }
}
