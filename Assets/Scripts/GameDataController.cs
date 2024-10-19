using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameDataController : MonoBehaviour
{
    public GameDataRepository gameDataRepository;
    public GameData gameData;

    public int life = 100;
    public bool invencibility = false;
    public float invencibilityTime = 1f;
    public float breakTime = 0.2f;

    public GameObject damageOverlay;
    public float fadeDuration = 1f;

    public Image healthBarImage; // Referencia al componente Image de la barra de vida

    private Image damageImage;
    private Color originalColor;

    void Start()
    {
        gameDataRepository = new GameDataRepository();
        gameData = gameDataRepository.LoadGame();
        life = gameData.healthplayer;

        // Obtener la referencia al componente Image del panel de daño
        damageImage = damageOverlay.GetComponent<Image>();
        originalColor = damageImage.color;

        // Asegurarse de que el panel esté invisible al inicio
        damageOverlay.SetActive(false);

        // Actualizar la barra de vida al inicio
        UpdateHealthBar();
    }

    public void RestarVida(int quantity)
    {
        if (!invencibility && life > 0)
        {
            life -= quantity;
            gameData.healthplayer = life;  // Actualizar GameData
            gameDataRepository.SaveGame(gameData);  // Guardar el estado actualizado

            // Actualizar la barra de vida
            UpdateHealthBar();

            // Iniciar el efecto de daño
            StartCoroutine(ShowDamageOverlay());

            // Iniciar invencibilidad temporal
            StartCoroutine(Invencibility());
        }
    }

    void UpdateHealthBar()
    {
        if (healthBarImage != null)
        {
            // Life está entre 0 y 100, y Fill Amount va de 0.0 a 1.0
            healthBarImage.fillAmount = life / 100f;
        }
    }
    public void RestaurarVida()
    {
        life = 100; // Restablecer la vida a 100
        gameData.healthplayer = life;  // Actualizar GameData
        gameDataRepository.SaveGame(gameData);  // Guardar el estado actualizado
    }

    IEnumerator Invencibility()
    {
        invencibility = true;
        yield return new WaitForSeconds(invencibilityTime);
        invencibility = false;
    }

    IEnumerator ShowDamageOverlay()
    {
        // Activar el overlay
        damageOverlay.SetActive(true);
        damageImage.color = originalColor;

        // Desvanecer el overlay
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(originalColor.a, 0f, elapsedTime / fadeDuration);
            damageImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // Desactivar el overlay cuando se haya desvanecido completamente
        damageOverlay.SetActive(false);
    }
}
