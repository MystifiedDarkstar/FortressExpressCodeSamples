using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyMeleeAttack : MonoBehaviour
{
    [SerializeField] int m_AttackDamage;
    [SerializeField] float m_AttackSpeed;
    [SerializeField] float m_AttackDistance;
    [SerializeField] private GameObject m_AttackParticles;
    [SerializeField] private float m_ParticlesAngleOffset;
    float m_currentAttackTimer;

    private EnemyBase m_EnemyBase;
    private GameObject m_Train;
    private GameObject m_Target;

    private void Awake()
    {
        m_EnemyBase = GetComponent<EnemyBase>();
        if (GameObject.FindObjectOfType<TrainEngine>() != null) { m_Train = GameObject.FindObjectOfType<TrainEngine>().gameObject; }
        else { m_Train = GameObject.FindObjectOfType<TrainEngineAlt>().gameObject; }
       


    }

    public Component GetTrainEngine()
    {
        if (GameObject.FindObjectOfType<TrainEngine>() != null) { return GameObject.FindObjectOfType<TrainEngine>(); }
        else { return GameObject.FindObjectOfType<TrainEngineAlt>(); }
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate which train carriage is closest, set that as target
        if (m_Train.GetComponent<TrainEngine>() && m_Train.GetComponent<TrainEngine>().enabled) { m_Target = m_Train.GetComponent<TrainEngine>().FindClosestCarriage(transform); }
        else { m_Target = m_Train.GetComponent<TrainEngineAlt>().FindClosestCarriage(transform); }

        // Check what type of carriage we are attacking, head or carriage.
        // IF It is a head, enemy is attacking the core
        if (m_Target.CompareTag("TrainHead") && m_currentAttackTimer <= 0.0f && Vector2.Distance(transform.position, m_Target.transform.position) <= m_AttackDistance)
        {
            AttackCore();
            m_currentAttackTimer = m_AttackSpeed;
        }
        // If it is a carriage, enemy is attacking the carriage HP
        else if (m_Target.CompareTag("TrainCarraige") && m_currentAttackTimer <= 0.0f && Vector2.Distance(transform.position, m_Target.transform.position) <= m_AttackDistance)
        {
            AttackCarriage();
            m_currentAttackTimer = m_AttackSpeed;
        }

        m_currentAttackTimer -= Time.deltaTime;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_AttackDistance);
    }

    private float GetAngle()
    {
        return Mathf.Round(m_EnemyBase.GetAngleMoving() / 90.0f) * 90.0f;
    }

    private void SpawnParticles()
    {
        GameObject obj = ObjectPool.p.GetObjectFromPool(m_AttackParticles, false);
        obj.transform.position = transform.position;
        obj.transform.eulerAngles = new Vector3(0, 0, GetAngle() + m_ParticlesAngleOffset);
        obj.SetActive(true);
    }
    private void AttackCore()
    {
        m_Train.GetComponentInChildren<TrainCore>().DealDamage(m_AttackDamage);
        SpawnParticles();

    }
    private void AttackCarriage()
    {
        m_Target.GetComponent<TrainCarriage>().DealDamage(m_AttackDamage);
        SpawnParticles();
    }
}
