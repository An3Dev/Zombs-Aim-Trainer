using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IKillable
{
    void Kill();
}

public interface IDamageable<T, GameObject>
{
    void Damage(T damageTaken, GameObject damager);
}
