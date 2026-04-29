using System;
using UnityEngine;

// This class represents AI Health component
// It can be used in any AI creature
public class AIHealth : MonoBehaviour
{
    public event Action Damaged;
    public event Action Died;

    public int CurrentHp { get; private set; }
    public int MaxHp { get; private set; }
    public bool IsDead { get; private set; }

    // Setting up hp varaibles
    public void Initialize(int maxHp) 
    {
        MaxHp = maxHp;
        CurrentHp = maxHp;
        IsDead = false;
    }

    public void ApplyDamage(int damage)
    {
        if (IsDead || damage <= 0) // if dead ignoring
            return;

        CurrentHp -= damage; // appying damage
        Damaged?.Invoke(); // invoking event

        if (CurrentHp <= 0) // if hp <= 0 then dying ;-)
        {
            CurrentHp = 0;
            IsDead = true;
            Died?.Invoke();
        }
    }
}