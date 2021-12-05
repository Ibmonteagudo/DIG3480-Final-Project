using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RubyController : MonoBehaviour
{
    public float speed = 3.0f;

    public Text ScoreText;
    public Text CogText;
    public Text WinText;

    public int maxHealth = 5;
    public int maxScore = 6;

    public Text FixedRobot;

    public Text winText;

    public ParticleSystem Hitparticles;

    public GameObject Hitparticlesprefab;

    public GameObject projectilePrefab;

    public AudioClip throwSound;
    public AudioClip hitSound;
    public AudioClip WinSound;
    public AudioClip LoseSound;
    public AudioClip BackgroundSound;

    public int health { get { return currentHealth; } }
    public int score { get { return currentScore; } }
    public int currentScene;
    int currentHealth;
    int currentCog;
    int currentScore = 0;

    public float timeInvincible = 2.0f;
    bool isInvincible;
    float invincibleTimer;

    Rigidbody2D rigidbody2d;
    float horizontal;
    float vertical;

    Animator animator;
    Vector2 lookDirection = new Vector2(1, 0);

    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        currentHealth = maxHealth;
        currentCog = 6;
        WinText.text = "";

        ScoreText.text = "Fixed Robots: " + currentScore;
        CogText.text = "Cogs:" + currentCog;


        audioSource = GetComponent<AudioSource>();

        audioSource.clip = BackgroundSound;
        audioSource.loop = true;
        audioSource.Play();

    }

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        Vector2 move = new Vector2(horizontal, vertical);

        if (!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
        {
            lookDirection.Set(move.x, move.y);
            lookDirection.Normalize();
        }

        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
        animator.SetFloat("Speed", move.magnitude);

        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer < 0)
                isInvincible = false;
        }

        if (currentScore == 6)
        {
            currentScene = SceneManager.GetActiveScene().buildIndex;
            if (currentScene == 0)
                winText.text = "Talk to Jambi to visit stage two!";
        }

        if (currentScore == 7)
        {
            winText.text = "You Win! Game created by Ibrahim Monteagudo";
            if (Input.GetKey(KeyCode.R))
            {
                SceneManager.LoadScene("FirstScene");
            }
        }

        if (Input.GetKeyDown(KeyCode.C) && currentCog != 0)
        {
            Launch();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            RaycastHit2D hit = Physics2D.Raycast(rigidbody2d.position + Vector2.up * 0.2f, lookDirection, 1.5f, LayerMask.GetMask("NPC"));
            if (hit.collider != null)
            {
                if (currentScore == 6)
                {
                    NonPlayerCharacter character = hit.collider.GetComponent<NonPlayerCharacter>();
                    if (character != null)
                    {
                        SceneManager.LoadScene("SecondScene");
                    }
                }

                else
                {
                    NonPlayerCharacter character = hit.collider.GetComponent<NonPlayerCharacter>();
                    if (character != null)
                    {
                        character.DisplayDialog();
                    }
                }
            }
        }
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }

        if (currentHealth == 0)
        {
            WinText.text = "You lost! Press R to restart";
            speed = 0;
            if (Input.GetKey(KeyCode.R))
            {
                SceneManager.LoadScene("FirstScene");
            }

        }
    }

    void FixedUpdate()
    {
        Vector2 position = rigidbody2d.position;
        position.x = position.x + speed * horizontal * Time.deltaTime;
        position.y = position.y + speed * vertical * Time.deltaTime;

        rigidbody2d.MovePosition(position);
    }

    public void ChangeHealth(int amount)
    {
        if (amount < 0)
        {
            animator.SetTrigger("Hit");
            if (isInvincible)
                return;

            isInvincible = true;
            invincibleTimer = timeInvincible;

            GameObject HitparticlesObject = Instantiate(Hitparticlesprefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

            PlaySound(hitSound);
        }

        if (currentHealth == 1)
        {
            audioSource.clip = BackgroundSound;
            audioSource.loop = true;
            audioSource.Stop();

            audioSource.clip = LoseSound;
            audioSource.loop = false;
            audioSource.Play();
        }


        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        UIHealthBar.instance.SetValue(currentHealth / (float)maxHealth);
    }

    public void ChangeCog(int amount)
    {
        currentCog = currentCog + amount;
        CogText.text = "Cogs:" + currentCog;
    }

    public void ChangeScore()
    {
        currentScore = currentScore + 1;
        ScoreText.text = "Fixed Robots: " + currentScore;

        if (currentScore == 7)
        {
            audioSource.clip = BackgroundSound;
            audioSource.loop = true;
            audioSource.Stop();

            audioSource.clip = WinSound;
            audioSource.loop = false;
            audioSource.Play();
        }

    }

    void Launch()
    {
        GameObject projectileObject = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

        Projectile projectile = projectileObject.GetComponent<Projectile>();
        projectile.Launch(lookDirection, 300);

        animator.SetTrigger("Launch");

        currentCog = currentCog - 1;
        CogText.text = "Cogs:" + currentCog;

        PlaySound(throwSound);
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

}
