using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface HealthInterface
{
    public void SetInitialHealth();

    public void ChangeHealth(float amount);

    public bool IsDead();

    public bool IsAlive();

    public void Death();

    public void Recover(float amount);

    public void TakeDamage(float damage);

    public void HealDamage(float amount);

    public void ShowHealth();
}
