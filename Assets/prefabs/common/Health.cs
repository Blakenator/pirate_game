using UnityEngine;

public class Health : MonoBehaviour
{
    public int team = 0;
    public float health = 100f;
    private float inititialHealth;
    public Rigidbody rigidbody;

    public System.Action<Vector3, Rigidbody> onDamage;
    public System.Action OnHealthDestroy;

    private void Start()
    {
        inititialHealth = health;
    }

    public void DoDamage(Vector3 point, Rigidbody cannonball, float damage)
    {
        health -= damage;
        onDamage?.Invoke(point, cannonball);
        if (health < 0)
        {
            OnHealthDestroy.Invoke();
        }
    }

    public bool IsEnemy(int yourTeam)
    {
        return team >= 0 && team != yourTeam;
    }

    public float GetInitialHealth()
    {
        return inititialHealth;
    }
}
