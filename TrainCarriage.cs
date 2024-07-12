using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TrainCarriage : MonoBehaviour
{
    [SerializeField] private GameObject m_UIControllerREF;

    private float m_CarraigeHealth;
    [SerializeField] private float m_CarraigeMaxHealth;

    [SerializeField] private int m_CarraigeID;

    [SerializeField] private GameObject m_RepairButton;

    // Carriage death variables
    [SerializeField] private GameObject m_CarriageExplosion;
    [SerializeField] private SpriteRenderer[] m_Wheels;
    [SerializeField] private Slot[] m_Slots;
    [SerializeField] private ParticleSystem m_BurningParticles;
    [SerializeField] private Color m_DamagedColor;
    [SerializeField] private AudioClip m_DestroyCarriageSound;
    private Color m_OriginalColor;
    private SpriteRenderer m_Sprite;
    //

    private ShakeObject shake;

    private enum CarriageState { Online, Damaged }
    CarriageState m_CurrentState;

    public UnityEvent CarraigeDamaged;
    public UnityEvent CarraigeSpawned;

    private void Awake()
    {
        m_Sprite = GetComponent<SpriteRenderer>();
        m_OriginalColor = m_Sprite.color;

        // Damaging carriage
        CarraigeDamaged.AddListener(delegate { m_RepairButton.SetActive(true); });
        CarraigeDamaged.AddListener(GameObject.FindObjectOfType<GameStats>().AddCarriageDamaged);
        CarraigeDamaged.AddListener(delegate { SoundManager.s.PlaySound(SoundEffect.TRAIN, m_DestroyCarriageSound); });
        CarraigeDamaged.AddListener(delegate { m_BurningParticles.Play(); });
        CarraigeDamaged.AddListener(delegate { m_Sprite.color = m_DamagedColor; });
        CarraigeDamaged.AddListener(delegate { m_CurrentState = CarriageState.Damaged; });
        CarraigeDamaged.AddListener(GameObject.FindObjectOfType<TrainCore>().RemoveActiveCarriage);

        CarraigeDamaged.AddListener(delegate
        {
            foreach (SpriteRenderer sprite in m_Wheels)
            {
                sprite.color = m_DamagedColor;
            }
            foreach (Slot slot in m_Slots)
            {
                slot.DisableWeapon();
            }
            GameObject obj = ObjectPool.p.GetObjectFromPool(m_CarriageExplosion);
            obj.transform.position = transform.position;
        });

        // Spawning carriage
        CarraigeSpawned.AddListener(delegate { m_BurningParticles.Stop(); });
        CarraigeSpawned.AddListener(delegate { m_Sprite.color = m_OriginalColor; });
        CarraigeSpawned.AddListener(delegate { m_CarraigeHealth = m_CarraigeMaxHealth; });
        CarraigeSpawned.AddListener(delegate { m_CurrentState = CarriageState.Online; });
        CarraigeSpawned.AddListener(GameObject.FindObjectOfType<TrainCore>().AddActiveCarriage);

        CarraigeSpawned.AddListener(delegate
        {
            foreach (SpriteRenderer sprite in m_Wheels)
            {
                sprite.color = m_OriginalColor;
            }
            foreach (Slot slot in m_Slots)
            {
                slot.EnableWeapon();
            }
        });


        shake = GetComponent<ShakeObject>();

        if (m_CarraigeID == 1)
            CarraigeSpawned.AddListener(delegate { m_UIControllerREF.GetComponent<UIController>().UpdateCarriage1HealthUI(m_CarraigeHealth / m_CarraigeMaxHealth); });
        else
            CarraigeSpawned.AddListener(delegate { m_UIControllerREF.GetComponent<UIController>().UpdateCarriage2HealthUI(m_CarraigeHealth / m_CarraigeMaxHealth); });

        CarraigeSpawned.Invoke();

    }
    public bool IsDamaged()
    {
        if (m_CurrentState == CarriageState.Damaged)
            return true;
        else
            return false;
    }
    public void DealDamage(int Damage)
    {
        m_CarraigeHealth -= Damage;

        if(m_CurrentState == CarriageState.Online)
        {
            SoundManager.s.PlaySound(SoundEffect.TRAIN, 0.2f);
            shake.StartShaking();
            if (m_CarraigeHealth <= 0)
                CarraigeDamaged.Invoke();
        }


        if (m_CarraigeID == 1)
            m_UIControllerREF.GetComponent<UIController>().UpdateCarriage1HealthUI(m_CarraigeHealth / m_CarraigeMaxHealth);
        else if (m_CarraigeID == 2)
            m_UIControllerREF.GetComponent<UIController>().UpdateCarriage2HealthUI(m_CarraigeHealth / m_CarraigeMaxHealth);

    }

    public void RepairCarriage()
    {
        GameObject.FindObjectOfType<GameStats>().AddCarriageRepaired();
        CarraigeSpawned.Invoke();
    }
}
