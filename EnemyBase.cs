using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.GraphicsBuffer;

public class EnemyBase : MonoBehaviour
{
    [SerializeField] string m_EnemyName;
    
    [SerializeField] float m_MovementSpeed;
    private float m_CurrentMovementSpeed;

    private float m_CurrentHealth;
    [SerializeField] private float m_MaxHealth;

    [SerializeField] int m_CurrencyReward;
    [SerializeField] private GameObject deathParticles;

    private bool m_IsDead = false;

    private SpriteRenderer m_Sprite;
    private Color m_OriginalColor;
    private Coroutine coroutine;
    private float m_AngleMoving;
    private Animator m_anim;

    private GameObject m_Train;
    private GameObject m_Target;

    public UnityEvent EnemyDeath;

    private void Awake()
    {
        m_anim = GetComponent<Animator>();
        m_Sprite = GetComponent<SpriteRenderer>();
        m_OriginalColor = m_Sprite.color;
        if (GameObject.FindObjectOfType<TrainEngine>() != null) { m_Train = GameObject.FindObjectOfType<TrainEngine>().gameObject; }
        else { m_Train = GameObject.FindObjectOfType<TrainEngineAlt>().gameObject; }

        EnemyDeath.AddListener(GameObject.FindObjectOfType<AISpawnController>().MinusCountToKill);
        EnemyDeath.AddListener(delegate { GameObject.FindObjectOfType<CurrencyManager>().AddCurrency(m_CurrencyReward); });
        EnemyDeath.AddListener(delegate { GameObject.FindObjectOfType<TrainCore>().HealCore(m_MaxHealth / 2); });
        EnemyDeath.AddListener(delegate 
        {
            SoundManager.s.PlaySound(SoundEffect.ENEMYDEATH, 0.2f);
            if (deathParticles)
            {
                GameObject obj = ObjectPool.p.GetObjectFromPool(deathParticles);
                obj.transform.position = transform.position;
            }

        });
    }

    public void InitaliseEnemy(float SpeedMod, float HealthMod)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        m_Sprite.color = m_OriginalColor;
        m_CurrentHealth = m_MaxHealth * HealthMod;
        m_CurrentMovementSpeed = m_MovementSpeed * SpeedMod;
        m_IsDead = false;
    }

    public Component GetTrainEngine()
    {

        if (GameObject.FindObjectOfType<TrainEngine>() != null) { return GameObject.FindObjectOfType<TrainEngine>(); }
        else { return GameObject.FindObjectOfType<TrainEngineAlt>(); }
    }

    private void Update()
    {
        // Calculate which train carriage is closest, set that as target
        //m_Target = m_Train.GetComponent<TrainEngine>().FindClosestCarriage(transform);

        if (m_Train.GetComponent<TrainEngine>()) { m_Target = m_Train.GetComponent<TrainEngine>().FindClosestCarriage(transform); }
        else { m_Target = m_Train.GetComponent<TrainEngineAlt>().FindClosestCarriage(transform); }

        // Move towards target carriage
        if (m_Target.activeInHierarchy == true && Vector2.Distance(transform.position, m_Target.transform.position) >= 2.0f)
        {
            transform.position = Vector2.MoveTowards(transform.position, m_Target.transform.position, m_CurrentMovementSpeed * Time.deltaTime);

            // Getting angle from movetowards
            Vector2 direction = m_Target.transform.position - transform.position;
            float angleRadians = Mathf.Atan2(direction.y, direction.x);
            m_AngleMoving = angleRadians * Mathf.Rad2Deg;
            direction.Normalize();
            m_anim.SetFloat("moveX", direction.x);
            m_anim.SetFloat("moveY", direction.y);

        }
    }

    public float GetAngleMoving()
    {
        return m_AngleMoving;
    }

    public void DealDamage(float Damage)
    {
        // Deal Damage
        m_CurrentHealth -= Damage;

        if(coroutine == null)
        {
            coroutine = StartCoroutine(SwitchColor());
        }

        // Check if Enemy Is Dead
        if (m_CurrentHealth <= 0 && !m_IsDead)
        {
            m_IsDead = true;
            EnemyDeath.Invoke();

            this.gameObject.transform.position = new Vector3(10, 10, 0);
            this.gameObject.SetActive(false);
        }
    }

    IEnumerator SwitchColor()
    {
        m_Sprite.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        m_Sprite.color = m_OriginalColor;
        coroutine = null;
    }
}
