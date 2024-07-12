using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyRangedAttack : MonoBehaviour
{
    [SerializeField] private float m_projectileSpeed; // Speed of the bullet when spawned
    [SerializeField] private int m_projectileDamage;

    [SerializeField] private float m_fireRate;
    private float m_CurrentFireRate;

    [SerializeField] private GameObject m_BulletPrefab;
    [SerializeField] private ObjectPool m_ObjectPool;

    private GameObject m_Train;
    private GameObject m_Target;

    private Vector3 m_Direction;

    private void Awake()
    {
        if (GameObject.FindObjectOfType<TrainEngine>() != null) { m_Train = GameObject.FindObjectOfType<TrainEngine>().gameObject; }
        else { m_Train = GameObject.FindObjectOfType<TrainEngineAlt>().gameObject; }
        m_ObjectPool = GameObject.FindObjectOfType<ObjectPool>();
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate which train carriage is closest, set that as target
        if (m_Train.GetComponent<TrainEngine>() && m_Train.GetComponent<TrainEngine>().enabled) { m_Target = m_Train.GetComponent<TrainEngine>().FindClosestCarriage(transform); }
        else { m_Target = m_Train.GetComponent<TrainEngineAlt>().FindClosestCarriage(transform); }

        m_CurrentFireRate -= Time.deltaTime;

        m_Direction = m_Target.transform.position - transform.position;

       // cehck if firerate cooldown is over, and if we have bullets to shoot
       if (m_CurrentFireRate <= 0)
       {
           //Shoot
           Fire();
           m_CurrentFireRate = m_fireRate;
       }
    }
    /// <summary> When called checks if the player can fire, if so it fires a bullet in mouse direction. Then proceeds to check if player needs to reload. </summary>
    private void Fire()
    {
        GameObject Bullet = m_ObjectPool.GetObjectFromPool(m_BulletPrefab);

        Bullet.SetActive(false);

        Bullet.transform.position = transform.position;

        Bullet.SetActive(true);

        if (Bullet.GetComponent<Rigidbody2D>() != null)
        {
            Bullet.GetComponent<Rigidbody2D>().AddForce(m_Direction.normalized * m_projectileSpeed, ForceMode2D.Impulse);
            Bullet.GetComponent<EnemyBullet>().SetDamage(m_projectileDamage);
        }
    }
}
