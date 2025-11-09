using UnityEngine;
using UnityEngine.UI; // You MUST add this line to control UI elements
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject gameOverScreen; // Drag your panel here in the Inspector
    // Public slot to hold the health bar
    public Slider healthSlider;

    // Call this function at the start to set the max health
    public void SetMaxHealth(int maxHealth)
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
    }

    // Call this function every time the player takes damage
    public void UpdateHealth(int currentHealth)
    {
        healthSlider.value = currentHealth;
    }
    
    public void ShowGameOverScreen()
{
    gameOverScreen.SetActive(true);
}

// This function will be called by your button
public void RestartGame()
{
    // This reloads the currently active scene
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}
}