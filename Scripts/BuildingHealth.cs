using UnityEngine;

public class BuildingHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    
    private void Start()
    {
        currentHealth = maxHealth;
    }
    
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            DestroyBuilding();
        }
    }
    
    private void DestroyBuilding()
    {
        // Add code here to handle building destruction
        Destroy(gameObject);
    }
}