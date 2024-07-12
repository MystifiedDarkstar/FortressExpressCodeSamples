using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainCore : MonoBehaviour
{
    [SerializeField] private GameObject m_UIControllerREF;
    [SerializeField] private GameObject m_ObjectPool;

    private float m_CoreHealth;
    [SerializeField] private float m_CoreMaxHealth;

    private float m_ActiveCarriages;
    [SerializeField] private float m_CarriageResistanceBonus;
    private float m_ActiveDamageResistance;

    private void Start()
    {
        m_CoreHealth = m_CoreMaxHealth;
        m_UIControllerREF.GetComponent<UIController>().UpdateEngineHeadHealthUI(m_CoreHealth / m_CoreMaxHealth);

    }
    private void Update()
    {
        m_ActiveDamageResistance = (m_CarriageResistanceBonus * m_ActiveCarriages) / 100;
    }
    public void DealDamage(int Damage)
    {
        m_CoreHealth -= (Damage * (1 - m_ActiveDamageResistance));
        

        if (m_CoreHealth <= 0 )
            CoreDeath();

        m_UIControllerREF.GetComponent<UIController>().UpdateEngineHeadHealthUI(m_CoreHealth / m_CoreMaxHealth);
    }

    private void CoreDeath()
    {
        Destroy(this.gameObject);
        // Deactivate all objects.
        m_ObjectPool.SetActive(false);
        GameObject.FindObjectOfType<AISpawnController>().EndGame();
        GameObject.FindObjectOfType<CurrencyManager>().EndGame();
        GameObject.FindObjectOfType<GameStats>().InitialiseUI();
    }

    public void HealCore(float HP)
    {
        m_CoreHealth = Mathf.Clamp(m_CoreHealth + HP, 0, m_CoreMaxHealth);
        m_UIControllerREF.GetComponent<UIController>().UpdateEngineHeadHealthUI(m_CoreHealth / m_CoreMaxHealth);
    }

    public void AddActiveCarriage()
    {
        m_ActiveCarriages++;
    }

    public void RemoveActiveCarriage()
    {
        m_ActiveCarriages--;
    }
}
