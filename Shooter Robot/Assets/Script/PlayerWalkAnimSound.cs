using UnityEngine;

public class PlayerWalkAnimSound : MonoBehaviour
{
    private AudioSource walkSound;

    private void Start()
    {
        walkSound = GetComponent<AudioSource>();
    }

    public void RightWalkAnimSound()
    {
        walkSound.Play();
    }
}
