using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public int rutina;
    public float cronometro;
    public Animator ani;
    public Quaternion angulo;
    public float grado;
    //

    public float velocidadDeCorrer = 6f; 
    public float velocidadDeCaminar = 2f; 
    //
    public int enemyDamage = 10;

    public GameObject target;

    void Start()
    {
        ani = GetComponent<Animator>();
        target = GameObject.Find("Ellen");
    }

    void Update()
    {
        Comportamiento_Enemy();
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player") 
        {
            // Verifica si el jugador tiene el componente GameDataController
            GameDataController playerData = other.GetComponent<GameDataController>();
            if (playerData != null)
            {
                playerData.RestarVida(enemyDamage); // Resta la vida del jugador
                //playerData.RestaurarVida();//Restaurar la vida
            }
        }
    }
    public void Comportamiento_Enemy()
    {
        // Estado cuando el jugador está lejos
        if (Vector3.Distance(transform.position, target.transform.position) > 10)
        {
            cronometro += Time.deltaTime;

            // Resetear las animaciones
            ani.SetBool("run", false);
            ani.SetBool("walk", false);
            ani.SetBool("idle", true); // Mantiene el idle activo

            if (cronometro >= 1.15f) // Idle por 1.15 segundos
            {
                rutina = Random.Range(0, 2); // Selecciona nueva rutina
                cronometro = 0;
            }

            switch (rutina)
            {
                case 0: // Idle
                    ani.SetBool("idle", true); // Activa idle
                    break;
                case 1: // Gira y camina
                    grado = Random.Range(0, 360);
                    angulo = Quaternion.Euler(0, grado, 0);
                    rutina++;
                    break;
                case 2: // Caminar por 2 segundos
                    ani.SetBool("idle", false);
                    ani.SetBool("walk", true); // Activa caminar
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, angulo, 0.5f);
                    transform.Translate(Vector3.forward * velocidadDeCaminar * Time.deltaTime); // Usar velocidad de caminar
                    if (cronometro >= 2.0f) // Sincroniza el tiempo de caminar
                    {
                        rutina = 0; // Reinicia la rutina
                        cronometro = 0; // Reinicia el cronómetro
                    }
                    break;
            }
        }
        else // Estado de persecución (run)
        {
            var lookPos = target.transform.position - transform.position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, 3);

            // Forzar la animación de correr mientras persigue
            ani.SetBool("walk", false); // Desactiva caminar
            ani.SetBool("idle", false); // Desactiva idle
            ani.SetBool("run", true);   // Mantiene la animación de correr activa

            // Movimiento del enemigo hacia el jugador mientras persigue
            transform.Translate(Vector3.forward * velocidadDeCorrer * Time.deltaTime); // Usar velocidad de correr
        }
    }
}
