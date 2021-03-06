using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [SerializeField] private float duration;
    [SerializeField] private float damage;
    [SerializeField] private float knock;

    public float getDamage()
    {
        return damage;
    }
    public float getDuration()
    {
        return duration;
    }
    public float getKnock()
    {
        return knock;
    }
    public void setDuration(float duration)
    {
        this.duration = duration;
    }
}
