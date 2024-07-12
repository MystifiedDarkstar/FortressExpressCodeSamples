using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TrainEngine : MonoBehaviour
{
    [SerializeField] private List<GameObject> m_Carriages;
    [SerializeField] private int m_CarriagePlaceCost;
    [SerializeField] private GameObject[] m_CarriageButtons;

    private int m_SelectedButton = 0;
    private int m_ActiveCarraiges = 1;

    private void Start()
    {
        foreach (GameObject obj in m_CarriageButtons)
        {
            obj.GetComponentInChildren<TextMeshProUGUI>().text = "£" + m_CarriagePlaceCost;
        }
    }

    public GameObject FindClosestCarriage(Transform Transform)
    {
        float ClosestDistance = 0;
        int ClosestIndex = 0;

        for (int i = 0; i < m_Carriages.Count; i++) 
        {
            if (i == 0 && m_Carriages[i].activeInHierarchy)
            {
                ClosestDistance = Vector3.Distance(m_Carriages[0].transform.position, Transform.position);
                ClosestIndex = 0;
            }
            else if (m_Carriages[i].activeInHierarchy && !m_Carriages[i].GetComponent<TrainCarriage>().IsDamaged() && Vector3.Distance(m_Carriages[i].transform.position, Transform.position) < ClosestDistance)
            {
                ClosestDistance = Vector3.Distance(m_Carriages[i].transform.position, Transform.position);
                ClosestIndex = i;
            }
        }
        return m_Carriages[ClosestIndex];
    }

    public void ActivateNextCarriage()
    {
        if (GameObject.FindObjectOfType<CurrencyManager>().HasCurrency(m_CarriagePlaceCost))
        {
            for (int i = 0; i < m_Carriages.Count; i++)
            {
                if (m_Carriages[i].activeInHierarchy == false)
                {
                    GameObject.FindObjectOfType<CurrencyManager>().RemoveCurrency(m_CarriagePlaceCost);
                    m_CarriageButtons[m_SelectedButton].SetActive(false);
                    if (m_CarriageButtons.Length > m_SelectedButton + 1) m_CarriageButtons[m_SelectedButton + 1].SetActive(true);
                    m_Carriages[i].SetActive(true);
                    m_ActiveCarraiges++;
                    m_SelectedButton++;
                    if (i == 1)
                        GameObject.FindObjectOfType<UIController>().ActivateCarriage1UI();
                    else if (i == 2)
                        GameObject.FindObjectOfType<UIController>().ActivateCarriage2UI();

                    break;
                }
            }
        }
    }

    public int GetActiveCarraiges()
    {
        return m_ActiveCarraiges;
    }
}
