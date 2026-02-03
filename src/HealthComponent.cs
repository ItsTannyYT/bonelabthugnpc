using System;
using UnityEngine;

namespace BonelabHostileNpc
{
    public class HealthComponent : MonoBehaviour
    {
        public float MaxHealth = 100f;
        public float CurrentHealth = 100f;

        public event Action OnDeath;

        public void Awake()
        {
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);
        }

        public void ApplyDamage(float amount)
        {
            if (CurrentHealth <= 0f)
            {
                return;
            }

            CurrentHealth = Mathf.Clamp(CurrentHealth - amount, 0f, MaxHealth);
            if (CurrentHealth <= 0f)
            {
                OnDeath?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            if (CurrentHealth <= 0f)
            {
                return;
            }

            CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0f, MaxHealth);
        }
    }
}
