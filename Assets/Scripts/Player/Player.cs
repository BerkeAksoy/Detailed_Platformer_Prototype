using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour, IDamagable
{

    public static float baseMoveSpeed = 6;
    //[SerializeField]
    private float moveSpeed = 6, jumpAbility = 12, climbingSpeed = 3, nextMagic = 0, nextSword = 0;
    private bool inWFAS, inWFASStarted, feared;

    private Rigidbody2D myRigidbody2D;
    private CapsuleCollider2D myBody2D;
    private BoxCollider2D myFeet2D;
    private GameObject myMagicBook2D;
    private GameObject mySword2D;
    private Animator animator;
    private PhysicsMaterial2D zeroFriction;
    public CharStats stats;
    private PlayerSpell spellToCast;
    [SerializeField]
    private GameObject popUpText;

    private bool canDoubleJump = true, isAwake = true, playerLR = true, grounded, inWater, onLadder;
    private bool alreadyRunning, alreadyClimbing, alreadySwimming;
    public bool canMove = true;

    void Start()
    {
        stats = GetComponent<CharStats>();
        myRigidbody2D = GetComponent<Rigidbody2D>();
        myBody2D = GetComponent<CapsuleCollider2D>();
        myFeet2D = GetComponent<BoxCollider2D>();
        myMagicBook2D = GameObject.Find("/" + name + "/Magic Book");
        mySword2D = GameObject.Find("/" + name + "/Hit_Box");
        animator = GetComponent<Animator>();
        zeroFriction = myBody2D.sharedMaterial;
    }

    void Update()
    {
        if (isAwake && canMove)
        {
            calculateMovement();
            Jump();
            Swim();
            climbing();
            castSpell();
            attack();

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                stats.chosenSpellSlot = 0;
                changeSpellTo(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                stats.chosenSpellSlot = 1;
                changeSpellTo(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                stats.chosenSpellSlot = 2;
                changeSpellTo(2);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag.Equals("Ground"))
        {
            //Debug.Log("Grounded");
            grounded = true;
        }

        if (collision.tag.Equals("Water"))
        {
            //Debug.Log("inWater");
            inWater = true;
        }

        if (collision.tag.Equals("Ladder"))
        {
            //Debug.Log("onLadder");
            onLadder = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag.Equals("Ground"))
        {
            //Debug.Log("Grounded false");
            grounded = false;
        }

        if (collision.tag.Equals("Water"))
        {
            //Debug.Log("inWater false");
            inWater = false;
            animator.SetBool("isSwimming", false);
            alreadySwimming = false;
            inWFAS = false;
        }

        if (collision.tag.Equals("Ladder"))
        {
            //Debug.Log("onLadder false");
            onLadder = false;
            animator.SetBool("isClimbing", false);
            alreadyClimbing = false;
        }
    }

    private void calculateMovement()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float a = moveSpeed;

        if (inWater)
        {
            a /= 3f;
        }

        if (!feared)
        {
            myRigidbody2D.velocity = new Vector2(horizontalInput * a, myRigidbody2D.velocity.y);
        }
        else
        {
            myRigidbody2D.velocity = new Vector2(-horizontalInput * a, myRigidbody2D.velocity.y);
        }

        FlipSprite(horizontalInput);

        if (horizontalInput != 0 && !alreadyRunning)
        {
            animator.SetBool("isRunning", true);
            alreadyRunning = true;
        }
        else if (horizontalInput == 0)
        {
            animator.SetBool("isRunning", false);
            alreadyRunning = false;
        }
    }

    private void FlipSprite(float inputValue)
    {
        if (inputValue != 0)
        {
            if (!playerLR && inputValue > 0)
            {
                transform.localScale = new Vector2(Mathf.Sign(myRigidbody2D.velocity.x), 1f);
                stats.flipBreathSprite();
                playerLR = true;
            }
            else if (playerLR && inputValue < 0)
            {
                transform.localScale = new Vector2(Mathf.Sign(myRigidbody2D.velocity.x), 1f);
                stats.flipBreathSprite();
                playerLR = false;
            }
        }
    }

    private void Jump()
    {
        if (!inWater)
        {
            if (grounded)
            {
                canDoubleJump = true;
            }

            if (Input.GetButtonDown("Jump"))
            {
                Vector2 groundJump = new Vector2(myRigidbody2D.velocity.x, Input.GetAxis("Jump") * jumpAbility);

                if (grounded)
                {
                    myRigidbody2D.velocity = groundJump;
                    animator.SetTrigger("Jump");
                    //grounded = false;
                    //animator.SetBool("isJumping", true);
                }
                else if (canDoubleJump)
                {
                    canDoubleJump = false;
                    myRigidbody2D.velocity = groundJump;
                    animator.SetTrigger("Jump");
                    //animator.SetBool("isJumping", true);
                }
            }
        }
        else if (grounded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                Vector2 waterJump = new Vector2(myRigidbody2D.velocity.x, Input.GetAxis("Jump") * jumpAbility * 2f / 3f);

                myRigidbody2D.velocity = waterJump;

                animator.SetTrigger("Jump");
                //grounded = false;
            }
        }
        
    }

    private void climbing()
    {
        if (onLadder)
        {
            float verticalInput = Input.GetAxis("Vertical");
            Vector2 climbVelocity = new Vector2(myRigidbody2D.velocity.x, verticalInput * climbingSpeed);

            if (!feared)
            {
                climbVelocity = new Vector2(myRigidbody2D.velocity.x, verticalInput * climbingSpeed);
            }
            else
            {
                climbVelocity = new Vector2(myRigidbody2D.velocity.x, -verticalInput * climbingSpeed);
            }

            myRigidbody2D.velocity = climbVelocity;

            if (Input.GetButton("Vertical") && !alreadyClimbing)
            {
                animator.SetBool("isClimbing", true);
                alreadyClimbing = true;
            }
            else if(!Input.GetButton("Vertical"))
            {
                animator.SetBool("isClimbing", false);
                alreadyClimbing = false;
            }
        }
    }

    private void Swim()
    {
        if (inWater)
        {
            float verticalInput = Input.GetAxis("Vertical");
            Vector2 swimVelocity = new Vector2(myRigidbody2D.velocity.x, verticalInput * climbingSpeed);

            if (!feared)
            {
                swimVelocity = new Vector2(myRigidbody2D.velocity.x, verticalInput * climbingSpeed);
            }
            else
            {
                swimVelocity = new Vector2(myRigidbody2D.velocity.x, -verticalInput * climbingSpeed);
            }

            if (!inWFAS && !inWFASStarted)
            {
                inWFASStarted = true;
                StartCoroutine(sInWFAS());
            }

            if (Input.GetButton("Vertical") && inWFAS)
            {
                myRigidbody2D.velocity = swimVelocity;
            }
            else if (!Input.GetButton("Vertical") && alreadySwimming)
            {
                if (myRigidbody2D.velocity.y < -6)
                {
                    myRigidbody2D.velocity = new Vector2(myRigidbody2D.velocity.x, -6);
                }
            }

            if (Input.GetButton("Vertical") && !alreadySwimming)
            {
                animator.SetBool("isSwimming", true);
                alreadySwimming = true;
            }
            else if(!Input.GetButton("Vertical") && alreadySwimming)
            {
                animator.SetBool("isSwimming", false);
                alreadySwimming = false;
            }
        }
        else if (!stats.fullBreath)
        {
            //StartCoroutine(refillBreath());
            StopCoroutine(breathCheck());

            stats.refillBreath();
        }
    }

    IEnumerator sInWFAS()
    {
        yield return new WaitForSeconds(0.3f);
        inWFAS = true;
        inWFASStarted = false;
        StartCoroutine(breathCheck());
    }

    IEnumerator breathCheck()
    {
        stats.updateBreathUI();

        while (inWFAS)
        {
            yield return new WaitForSeconds(2f);

            if(stats.currentBreath > 0)
            {
                stats.currentBreath--;
                stats.updateBreathUI();
                Debug.Log("My Breath: " + stats.currentBreath);
            }
            else
            {
                Debug.Log("Taking Damage");
                //takeDamage(10);
            }

            stats.fullBreath = false;
        }
    }

    private void attack()
    {
        if (Input.GetKeyDown(KeyCode.X) && Time.time > nextSword)
        {
            int hitChance = Random.Range(0, 100);

            if (hitChance >= stats.cMissChance) {
                int damage = stats.cPhyDmg;

                Debug.Log(stats.cCritChance);

                if (Random.Range(0, 100) < stats.cCritChance)
                {
                    damage = Mathf.FloorToInt(stats.cPhyDmg * stats.getDamageRandomizer() * stats.cCritMultiplier);
                    Debug.Log("Crit Timeeeeeeeeeeeeeeeeee !!!!!!!!!!!!!!!!!!!!!");
                }
                else
                {
                    damage = Mathf.FloorToInt(stats.cPhyDmg * stats.getDamageRandomizer());
                }

                mySword2D.GetComponent<FriendlyHitBox>().setDamage(damage);
            }
            else
            {
                mySword2D.GetComponent<FriendlyHitBox>().setDamage(0);
                Debug.Log("Missed!");
            }
            animator.SetTrigger("isAttacking");
            nextSword = Time.time + stats.cAttackSpeed;
        }
    }

    public void changeSpellTo(int slotNo)
    {
        if (stats.eqpMagicBooks[slotNo] != null)
        {
            spellToCast = stats.eqpMagicBooks[slotNo].playerSpell;
            stats.cCastSpeed = stats.eqpMagicBooks[slotNo].castRate;
            stats.calculateStats();
            //stats.cMgcDmg += stats.eqpMagicBooks[slotNo].spellDamage;
        }
        else
        {
            spellToCast = null;
        }
    }

    private void castSpell()
    {
        if (Input.GetButtonDown("Fire1") && Time.time > nextMagic && spellToCast != null) //Değişim
        {
            int spellCost = Mathf.FloorToInt((float)spellToCast.getSpellCost() * (float)(100 - stats.cManaCostReduce) / 100f);

            if (stats.currentMana >= spellCost)
            {
                animator.SetTrigger("CastSpell");
                nextMagic = Time.time + stats.cCastSpeed;

                stats.currentMana -= spellCost;
                stats.updateMana();
                Instantiate(spellToCast, myMagicBook2D.transform.position, Quaternion.identity);
            }
            else
            {
                Debug.Log("Not enough mana");
            }
        }
    }

    public void takeDamage(int value)
    {
        //Vector2 deathKick = new Vector2(-Mathf.Sign(myRigidbody2D.velocity.x) * 8.0f, 18.0f);
        Vector2 deathKick;

        if (isFacingRight())
        {
            deathKick = new Vector2(-4.0f, 2.0f);
        }
        else
        {
            deathKick = new Vector2(4.0f, 2.0f);
        }

        //myRigidbody2D.velocity = deathKick;


        if (isAwake)
        {
            stats.currentHP -= value;

            myBody2D.sharedMaterial = null;
            if (!inWFAS)
            {
                isAwake = false;
                animator.SetTrigger("Hurt");
            }
        }

        if (value > 0)
        {
            GameObject a = Instantiate(popUpText, transform.position, Quaternion.identity);
            a.GetComponent<TextMeshPro>().text = value.ToString();
            a.GetComponent<TextMeshPro>().color = Color.red;
        }
        else
        {
            GameObject a = Instantiate(popUpText, transform.position, Quaternion.identity);
            a.GetComponent<TextMeshPro>().text = "Missed!";
            a.GetComponent<TextMeshPro>().color = Color.red;
        }

        stats.updateHealth();

        if (stats.currentHP > 0 && !isAwake)
        {
            StartCoroutine(reBorn());
        }
        else if(stats.currentHP <= 0)
        {
            gameObject.layer = LayerMask.NameToLayer("DeadPlayer");
            Debug.Log("You died");
        }
    }

    public void fear()
    {
        feared = true;
        StartCoroutine(reFear());
    }

    IEnumerator reFear()
    {
        yield return new WaitForSeconds(3f);
        feared = false;
    }

    private IEnumerator reBorn()
    {
        yield return new WaitForSeconds(0.6f);
        animator.SetTrigger("Reborn");
        //gameObject.layer = LayerMask.NameToLayer("Player");
        myBody2D.sharedMaterial = zeroFriction;
        myRigidbody2D.freezeRotation = true;
        isAwake = true;
    }

















    // Getters and Setters

    public void setMoveSpeed(float moveSpeed)
    {
        this.moveSpeed = moveSpeed;
    }

    public bool isFacingRight()
    {
        return transform.localScale.x > 0;
    }

    public bool isGrounded()
    {
        return grounded;
    }

    public Rigidbody2D getRigidbody2D()
    {
        return myRigidbody2D;
    }

    public Animator getAnimator()
    {
        return animator;
    }
}
