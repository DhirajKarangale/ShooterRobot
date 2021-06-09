using UnityEngine;
using UnityEngine.UI;
using EasyJoystick;

enum Direction
{
    Left,
    Right
}

public class Player : MonoBehaviour
{
    private Animator animator;
    private Rigidbody playerRigidbody;
    private ConstantForce playerConstantForce;
    private AudioSource audioSource;
    [SerializeField] AudioSource thrustSound;
    private Direction direction = Direction.Right;
    [SerializeField] Joystick moveJoystick;
    [SerializeField] Joystick attackJoystick;

    [SerializeField] GameObject missileBox;
    [SerializeField] GameObject explosionEffect;
    [SerializeField] GameObject UICanvas;
    [SerializeField] GameObject missile;
    [SerializeField] ParticleSystem rightMuzzle, leftMuzzle, rightFire, leftFire, boost;
    [SerializeField] Transform leftArm, rightArm;
    [SerializeField] Transform missilePoint;
    [SerializeField] Light leftLight, rightLight;
    [SerializeField] float speed = 4f;
    [SerializeField] int health =1000;
    [SerializeField] Slider healthSlider;
    [SerializeField] Text messageText;
    [SerializeField] GameObject messageTextObject;
    [SerializeField] Text missileCountText;
    [SerializeField] byte missileCount = 4;

    private ParticleSystem.EmissionModule rightMuzzleEmission, leftMuzzleEmission, rightFireEmission, leftFireEmission;
    private ParticleSystem.MainModule boostEmission;

    public static bool isPlayerDead;

    private void Awake()
    {
        missileCountText.text = missileCount.ToString();
        DesableMsgTxt();
        healthSlider.value = health;
        gameObject.SetActive(true);
        UICanvas.SetActive(true);
        audioSource = GetComponent<AudioSource>();
        playerRigidbody = GetComponent<Rigidbody>();
        playerConstantForce = playerRigidbody.GetComponent<ConstantForce>();
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
        MoveFly();
        RotateArmShoot();
    }
   
    private void MoveFly()
    {
        // Move Left Right
        if (Input.GetKey(KeyCode.LeftArrow) || (moveJoystick.Horizontal() < 0))
        {
            if (!LeanTween.isTweening(gameObject))
            {
                if (isGrounded()) animator.Play("Walk");
                else animator.Play("Idle");

                if (direction != Direction.Left) LeanTween.rotateAroundLocal(gameObject, Vector3.up, 180, 0.3f).setOnComplete(TurnLeft);
                else transform.Translate(Vector3.forward * speed * Time.deltaTime);
            }
        }
        else if (Input.GetKey(KeyCode.RightArrow) || (moveJoystick.Horizontal() > 0))
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

        // Fly
        if (Input.GetKey(KeyCode.Z) || (moveJoystick.Vertical() > 0))
        {
            playerConstantForce.force = Vector3.zero;
            if (playerRigidbody.velocity.y < 4f) playerRigidbody.AddRelativeForce(Vector3.up * 20);

            if (!boostEmission.loop)
            {
                boost.Play();
                boostEmission.loop = true;
            }
        }
        else if (moveJoystick.Vertical() < 0)
        {
            playerRigidbody.AddForce(Vector3.down * 10);
        }
        else
        {
            playerConstantForce.force = new Vector3(0, -10, 0);
            boostEmission.loop = false;
        }

    }
    private void RotateArmShoot()
    {
        Vector3 moveVector = (Vector3.up * attackJoystick.Horizontal() + Vector3.left * attackJoystick.Vertical());
        Vector3 moveVector2 = (Vector3.down * attackJoystick.Horizontal() + Vector3.right * attackJoystick.Vertical());
        if ((attackJoystick.Horizontal() != 0) || (attackJoystick.Vertical() != 0))
        {
           rightArm.transform.rotation = Quaternion.LookRotation(Vector3.back, moveVector);
           leftArm.transform.rotation = Quaternion.LookRotation(Vector3.back, moveVector2);

            // Shoot
            if (!audioSource.isPlaying)
            {
                leftLight.intensity = rightLight.intensity = 1.5f;
                audioSource.Play();
            }
            rightMuzzleEmission.rateOverTime = leftMuzzleEmission.rateOverTime = 10;
            rightFireEmission.rateOverTime = leftFireEmission.rateOverTime = 30;
        }
        else
        {
            // Stop Shoot
            audioSource.Stop();
            rightMuzzleEmission.rateOverTime = leftMuzzleEmission.rateOverTime = 0;
            rightFireEmission.rateOverTime = leftFireEmission.rateOverTime = 0;
            leftLight.intensity = rightLight.intensity = 0;
        }
    }

    public void MissileButton()
    {
        if (missileCount > 0)
        {
            LaunchMissile();
            missileCount--;
            missileCountText.color = Color.white;
            missileCountText.text = missileCount.ToString();
        }
        else
        {
            missileCountText.color = Color.red;
            missileCountText.text = missileCount.ToString();
            messageTextObject.SetActive(true);
            messageText.text = "Not Enough Missile, Find missile Box.";
            Invoke("DesableMsgTxt", 1);
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
    private void Destroy()
    {
        isPlayerDead = true;
        Destroy(Instantiate(explosionEffect, transform.position, Quaternion.identity), 1);
        gameObject.SetActive(false);
        UICanvas.SetActive(false);
    }

    private void TurnLeft()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        direction = Direction.Left;
    }
    private void TurnRight()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        direction = Direction.Right;
    }

    private bool isGrounded()
    {
        return Physics.Raycast(transform.position + transform.forward * 0.4f + transform.up * 0.1f, Vector3.down, 0.1f);
    }

    private void DesableMsgTxt()
    {
        messageTextObject.SetActive(false);
    }
    private void OnParticleCollision(GameObject other)
    {
        if (other.name == "MuzzleEnemy")
        {
            if (health <= 0) Destroy();
            else
            { 
                health -= 4;
                healthSlider.value = health;
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "MissileBox")
        {
            missileCount += 10;
            missileCountText.color = Color.green;
            missileCountText.text = missileCount.ToString();
            Destroy(missileBox);
        }
    }

    /* IEnumerator LightControl()
     {
         while (true)
         {
             leftLight.intensity = rightLight.intensity = 1;
             yield return new WaitForSeconds(0.3f);
             leftLight.intensity = rightLight.intensity = 0;
             yield return new WaitForSeconds(0.3f);
         }
     }*/
}
