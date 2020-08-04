using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player")]
	[SerializeField] float moveSpeed;
    [SerializeField] float padding;
    [SerializeField] int health = 200;
 
    [Header("Projectile")]
    [SerializeField] GameObject laserPrefab;
    [SerializeField] float fireSpeed;
    [SerializeField] float fireCycle;

    [Header("ShotOffset")]
    [SerializeField] float xOffset;
    [SerializeField] float yOffset;

    [Header("SFXs")]
    [SerializeField] AudioClip fireSFXs;

    Coroutine firingCoroutine;

    float xMin;
    float xMax;
    float yMin;
    float yMax;


    // Start is called before the first frame update
    void Start()
    {
        SetUpMoveBoundaries();
    }

    private void SetUpMoveBoundaries()
    {
        Camera gameCamera = Camera.main;
        xMin = gameCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x + padding;
        xMax = gameCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x - padding;
        yMin = gameCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).y + padding;
        yMax = gameCamera.ViewportToWorldPoint(new Vector3(0, 1, 0)).y - padding;
    }


    // Update is called once per frame
    void Update()
    {
		Move();
        Shoot();
    }

    private void Shoot()
    {
        if (Input.GetButtonDown("Fire1")) 
        {
            firingCoroutine = StartCoroutine(FireCoutinuously());
        }
        if (Input.GetButtonUp("Fire1"))
        {
            StopCoroutine(firingCoroutine);
        }
    }

    IEnumerator FireCoutinuously()
    {
        while(true) //shoot while holding down the button
        {
            //instantiate and set offset to fire from gun
            Vector3 offset = new Vector3(xOffset, yOffset, 0);
            GameObject laser = Instantiate
                (laserPrefab,
                transform.position + offset,
                Quaternion.identity) as GameObject;

            //then, fire
            laser.GetComponent<Rigidbody2D>().velocity = new Vector2(fireSpeed, 0);
            AudioSource.PlayClipAtPoint(fireSFXs, Camera.main.transform.position);
            yield return new WaitForSeconds(fireCycle);
        }
  
    }

    private void Move()
	{
		var deltaX = Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed;
		var deltaY = Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed;

        var newXPos = Mathf.Clamp(transform.position.x + deltaX, xMin, xMax);
        var newYPos = Mathf.Clamp(transform.position.y + deltaY, yMin, yMax);

		transform.position = new Vector2(newXPos, newYPos);
	}

    private void OnTriggerEnter2D(Collider2D other)
    {
        DamageDealer damageDealer = other.gameObject.GetComponent<DamageDealer>();
        if (!damageDealer) { return; }
        ProcessHit(damageDealer);
    }

    private void ProcessHit(DamageDealer damageDealer)
    {
        health -= damageDealer.Getdamage();
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

}
