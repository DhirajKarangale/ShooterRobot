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
    private Direction direction = Direction.Right;
    private float playerPosition;

    [Header("Attack")]
    [SerializeField] Joystick attackJoystick;
    [SerializeField] ParticleSystem rightMuzzle, leftMuzzle, rightFire, leftFire, boost;
    [SerializeField] Light leftLight, rightLight;
    [SerializeField] byte missileCount = 4;
    [SerializeField] GameObject missile;
    [SerializeField] Transform missilePoint;
    [SerializeField] Text missileCountText;
    private ParticleSystem.EmissionModule rightMuzzleEmission, leftMuzzleEmission, rightFireEmission, leftFireEmission;
    private ParticleSystem.MainModule boostEmission;

    [Header("Movement")]
    [SerializeField] float speed = 4f;
    [SerializeField] float jumpForce = 15;
    [SerializeField] Joystick moveJoystick;
    [SerializeField] Transform leftArm, rightArm;

    [Header("Sound")]
    private AudioSource bulletSound;
    [SerializeField] AudioSource thrustSound;

    [Header("Death")]
    [SerializeField] int health = 1000;
    [SerializeField] Slider healthSlider;
    [SerializeField] GameObject explosionEffect;
    [SerializeField] GameObject UICanvas;
    public static bool isPlayerDead;

    [Header("Items")]
    [SerializeField] GameObject missileBox;
    [SerializeField] GameObject missileboxEffect;
    [SerializeField] GameObject healthPack;
    [SerializeField] GameObject healthPackEffect;

    [Header("Others")]
    [SerializeField] Text messageText;
    [SerializeField] GameObject messageTextObject;



    private void Awake()
    {
        missileCountText.text = missileCount.ToString();
        DesableMsgTxt();
        healthSlider.value = health;
        gameObject.SetActive(true);
        UICanvas.SetActive(true);
        bulletSound = GetComponent<AudioSource>();
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
        if (moveJoystick.Horizontal() < 0)
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
        if (moveJoystick.Vertical() > 0)
        {
            if (playerPosition > transform.position.x)
            {
                direction = Direction.Left;
            }
            else if (playerPosition < transform.position.x)
            {
                direction = Direction.Right;
            }
            playerConstantForce.force = Vector3.zero;
            if (playerRigidbody.velocity.y < 4f) playerRigidbody.AddRelativeForce(Vector3.up * jumpForce);

            if (!boostEmission.loop)
            {
                thrustSound.Play();
                boost.Play();
                boostEmission.loop = true;
            }
            playerPosition = transform.position.x;
        }
        else if (moveJoystick.Vertical() < 0)
        {
            thrustSound.Stop();
            playerRigidbody.AddForce(Vector3.down * 10);
        }
        else
        {
            thrustSound.Stop();
            playerConstantForce.force = new Vector3(0, -10, 0);
            boostEmission.loop = false;
        }

    }
    private void RotateArmShoot()
    {
        Vector3 moveVector, moveVector2;
        if (direction == Direction.Right)
        {
            moveVector = (Vector3.up * attackJoystick.Horizontal() + Vector3.left * attackJoystick.Vertical());
            moveVector2 = (Vector3.down * attackJoystick.Horizontal() + Vector3.right * attackJoystick.Vertical());
        }
        else
        {
            moveVector = (Vector3.down * attackJoystick.Horizontal() + Vector3.right * attackJoystick.Vertical());
            moveVector2 = (Vector3.up * attackJoystick.Horizontal() + Vector3.left * attackJoystick.Vertical());
        }

        if ((attackJoystick.Horizontal() != 0) || (attackJoystick.Vertical() != 0))
        {
            // Rotate Player According to attack direction
            if ((attackJoystick.Horizontal() != 0))
            {
                if (attackJoystick.Horizontal() > 0)
                {
                    if (direction == Direction.Left)
                    {
                        //LeanTween.rotateAroundLocal(gameObject, Vector3.up, 180, 0.3f);
                        transform.rotation = Quaternion.Euler(0, 90, 0);
                        direction = Direction.Right;
                    }
                }

                if (attackJoystick.Horizontal() < 0)
                {
                    if (direction == Direction.Right)
                    {
                        // LeanTween.rotateAroundLocal(gameObject, Vector3.up, -180, 0.3f);
                        transform.rotation = Quaternion.Euler(0, -90, 0);
                        direction = Direction.Left;
                    }
                }
            }

            if (direction == Direction.Right)
            {
                rightArm.transform.rotation = Quaternion.LookRotation(Vector3.back, moveVector);
                leftArm.transform.rotation = Quaternion.LookRotation(Vector3.back, moveVector2);
            }
            else
            {
                rightArm.transform.rotation = Quaternion.LookRotation(Vector3.forward, moveVector);
                leftArm.transform.rotation = Quaternion.LookRotation(Vector3.forward, moveVector2);
            }

            // Shoot
            if (!bulletSound.isPlaying)
            {
                leftLight.intensity = rightLight.intensity = 1.5f;
                bulletSound.Play();
            }
            rightMuzzleEmission.rateOverTime = leftMuzzleEmission.rateOverTime = 10;
            rightFireEmission.rateOverTime = leftFireEmission.rateOverTime = 30;
        }
        else
        {
            // Stop Shoot
            bulletSound.Stop();
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

            for(int i = 0; i < 4; i++)
            {
                Vector3 origin = position + Vector3.up * Random.Range(-0.6f, 1f) + Vector3.left * Random.Range(-1, 1);
                GameObject currentMissile = Instantiate(missile, origin, rightArm.transform.rotation);
                Vector3 targetPosition = missilePoint.position + missilePoint.forward * 20 + missilePoint.up * Random.Range(-2, 2);
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
        direction = Direction.Left;
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }
    private void TurnRight()
    {
        direction = Direction.Right;
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
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
  
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "HealthPack")
        {
            if (health < 1000)
            {
                Destroy(Instantiate(healthPackEffect, transform.position, Quaternion.Euler(-90,0,0)), 1);
                health = 1000;
                healthSlider.value = health;
                Destroy(healthPack);
            }
        }
        if (other.gameObject.tag == "MissileBox")
        {
            Destroy(Instantiate(missileboxEffect, transform.position, Quaternion.Euler(-90, 0, 0)), 1);
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
