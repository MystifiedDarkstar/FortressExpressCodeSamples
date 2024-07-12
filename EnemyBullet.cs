using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [SerializeField] private float bulletSpeed = 5;
    [SerializeField] private float timeTilDisable = 5;
    [SerializeField] private GameObject bulletParticles;
    private int damage;
    // Start is called before the first frame update
    void OnEnable()
    {
        Invoke("DisableOnTime", timeTilDisable);
    }

    private void OnDisable()
    {
        if (bulletParticles)
        {
            GameObject obj = ObjectPool.p.GetObjectFromPool(bulletParticles, false);
            ParticleSystem particles = obj.GetComponent<ParticleSystem>();
            particles.Stop();
            obj.transform.position = transform.position;
            obj.SetActive(true);
            particles.Play();
        }
    }

    private void DisableOnTime()
    {
        gameObject.SetActive(false);
    }

    public void SetDamage(int damageSet)
    {
        damage = damageSet;
    }

    public float GetDamage()
    {
        return damage;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("TrainHead")) 
        {
            GameObject.FindObjectOfType<TrainCore>().DealDamage(damage);
            this.gameObject.SetActive(false);
        }
        else if (collision.gameObject.CompareTag("TrainCarraige"))
        {
            GameObject.FindObjectOfType<TrainCarriage>().DealDamage(damage);
            this.gameObject.SetActive(false);
            this.gameObject.transform.position = new Vector3(10, 10, 0);
        }
    }
}
