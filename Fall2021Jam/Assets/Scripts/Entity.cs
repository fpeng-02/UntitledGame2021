using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [SerializeField] private float health;
    private bool isAlive;
    //knockback multiplier

    public void setHealth(float health)
    {
        this.health = health;
    }
    public float getHealth()
    {
        return health;
    }
    public virtual void applyHit(float damage, Vector3 vector)
    {
        //Debug.Log("Hit Recieved!");
        health -= damage;
        gameObject.GetComponent<Rigidbody2D>().AddForce(vector, ForceMode2D.Impulse);

        //Debug.Log(health);
    }

    public virtual void Update()
    {
        if (health <= 0)
        {
            Destroy(this.gameObject);
        }
    }
}
