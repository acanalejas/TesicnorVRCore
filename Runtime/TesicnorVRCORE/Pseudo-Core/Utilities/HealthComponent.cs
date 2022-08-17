using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class HealthComponent : MonoBehaviour, HealthInterface
{
    #region PARAMETERS
    /// <summary>
    /// La vida máxima
    /// </summary>
    [Header("La vida máxima")]
    public float maxHealth = 100;

    /// <summary>
    /// La vida actual
    /// </summary>
    [Header("La vida actual")]
    public float Health;

    /// <summary>
    /// La imagen que nos muestra la vida
    /// </summary>
    [Header("La imagen que nos muestra la vida")]
    [SerializeField] protected Image HealthIMG;

    /// <summary>
    /// El texto que nos dice cuanta vida tenemos
    /// </summary>
    [Header("El texto que nos dice cuanta vida tenemos")]
    [SerializeField] protected TextMeshProUGUI HealthText;

    /// <summary>
    /// Evento que se dispara al morir
    /// </summary>
    [Header("Evento que se dispara al morir")]
    public UnityEvent onDeath;

    /// <summary>
    /// Evento que se dispara al revivir
    /// </summary>
    [Header("Evento que se dispara al revivir")]
    public UnityEvent onRecover;

    [HideInInspector] public bool isDead = false;
    [HideInInspector] public bool isAlive = true;
    #endregion

    #region FUNCTIONS
    /// <summary>
    /// Método que cambia la vida del objeto
    /// </summary>
    /// <param name="amount"></param>
    public void ChangeHealth(float amount)
    {
        Health += amount;

        ShowHealth();
        if (IsDead()) Death();
    }

    /// <summary>
    /// Método que se usa al morir
    /// </summary>
    public virtual void Death()
    {
        onDeath.Invoke();
    }

    /// <summary>
    /// Esta vivo el objeto?
    /// </summary>
    /// <returns></returns>
    public bool IsAlive()
    {
        isAlive = Health >= 0;
        return isAlive;
    }

    /// <summary>
    /// Esta muerto el objeto?
    /// </summary>
    /// <returns></returns>
    public bool IsDead()
    {
        isDead = Health <= 0;
        return isDead;
    }

    /// <summary>
    /// Método que se llama para revivir el objeto
    /// </summary>
    /// <param name="amount"></param>
    public virtual void Recover(float amount)
    {
        isDead = false;
        isAlive = true;

        Health = amount;
        onRecover.Invoke();
    }

    /// <summary>
    /// Setea la vida inicial
    /// </summary>
    public virtual void SetInitialHealth()
    {
        Health = maxHealth;
        ShowHealth();
    }

    /// <summary>
    /// Función que se llama para hacer daño al objeto
    /// </summary>
    /// <param name="damage"></param>
    public virtual void TakeDamage(float damage)
    {
        ChangeHealth(-damage);
    }

    /// <summary>
    /// Funcion que se llama para curar al objeto
    /// </summary>
    /// <param name="amount"></param>
    public virtual void HealDamage(float amount)
    {
        ChangeHealth(amount);
    }

    /// <summary>
    /// Muestra por pantalla la vida que nos queda en funcion de una imagen y/o un texto
    /// </summary>
    public virtual void ShowHealth()
    {
        if (HealthIMG)
        {
            HealthIMG.fillMethod = Image.FillMethod.Horizontal;
            HealthIMG.fillAmount = Health / maxHealth;
        }

        if (HealthText)
        {
            HealthText.text = ((int)Health).ToString() + " / " + ((int)maxHealth).ToString();
        }
    }
    #endregion
}
