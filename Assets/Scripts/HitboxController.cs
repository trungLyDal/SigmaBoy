using UnityEngine;

public class HitboxController : MonoBehaviour
{

    public int damageAmount = 1;
   void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();

            if (enemy != null)
            {
                enemy.TakeDamage(damageAmount); 
            }
            
        }
    }
}