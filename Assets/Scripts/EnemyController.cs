using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour {

    GridController gc;
    DefenceController dc;
    GameObject healthBox;
    ParticleSystem psDamage;

    Vector3 targetPoint;
    int pathPointIndex = 0, finalPointIndex;

    public float health, maxHealth = 50.0f;
    public float targetSpeed = 1.0f, speed;
    public int value = 150;
    public bool injured;

	// Use this for initialization
    void Awake()
    {
        gc = GameObject.FindGameObjectWithTag("GameController").GetComponent<GridController>();
        dc = GameObject.FindGameObjectWithTag("GameController").GetComponent<DefenceController>();
        healthBox = transform.FindChild("Health Box").gameObject;
        psDamage = GetComponentInChildren<ParticleSystem>();

        GetComponentInChildren<Animator>().speed = speed * 1.25f / transform.localScale.x;
        finalPointIndex = dc.GetPathLength();

        health = maxHealth;
        injured = false;
	}
	
	// Update is called once per frame
	void Update () {
        targetPoint = dc.GetPathPoint(pathPointIndex);
        if ((transform.position - targetPoint).sqrMagnitude < 0.01f)
        {
            pathPointIndex++;
            if (pathPointIndex >= finalPointIndex)
            {
                dc.ReduceHealth(10);
                Destroy(gameObject);
            }
        }
        Vector3 direction = (targetPoint - transform.position).normalized;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction, Vector3.up), 10.0f);
        transform.position += direction * speed * Time.deltaTime;
        healthBox.transform.rotation = Quaternion.identity;

        healthBox.transform.localScale = Vector3.one * 0.25f * health / maxHealth;

        speed = speed * 0.8f + targetSpeed * 0.2f;

        if (health <= maxHealth * 0.5f)
        {
            SetInjured();
        }
        if (health <= 0.0f)
        {
            dc.AddMoney(value);
            Destroy(gameObject);
        }

        gc.NotifyOfEnemy(transform.position.x, transform.position.z);
	}

    void OnTriggerEnter(Collider collider)
    {
        if (collider.transform.root.tag == "Laser")
        {
            Damage(Mathf.Pow(collider.transform.localScale.x / Global.UnitBeamRadius, 2.0f) * 0.2f);
        }
    }

    public void SetStats(float newHealth, float newSpeed, int newValue)
    {
        maxHealth = newHealth;
        targetSpeed = newSpeed;
        value = newValue;
        health = maxHealth;
        injured = false;
    }

    void SetInjured()
    {
        if (!injured)
        {
            targetSpeed *= 0.5f;
            injured = true;
            GetComponentInChildren<Animator>().SetBool("Injured", true);
        }
        transform.GetChild(0).localRotation
            = Quaternion.RotateTowards(transform.GetChild(0).localRotation, Quaternion.Euler(0.0f, -40.0f, 0.0f), 1.0f);
        GetComponentInChildren<Animator>().speed = speed * 3.75f / transform.localScale.x;
    }

    public void Damage(float amount)
    {
        health -= amount;
        if (amount >= 0.1f)
        {
            psDamage.Play();
        }
    }
}
