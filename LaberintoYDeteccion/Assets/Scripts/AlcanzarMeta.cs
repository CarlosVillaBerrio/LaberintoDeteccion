using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AlcanzarMeta : MonoBehaviour
{
    public AudioClip comienzo; // SONIDOS QUE SIRVEN DE RETROALIMENTACION
    public AudioClip nivelCompletado;
    public AudioClip atrapada;
    public AudioClip saleDeCasa;
    
    IEnumerator RecargarEscena()  // EFECTUA UN REINICIO DE ESCENA
    {
        yield return new WaitForSeconds(1.9f);
        SceneManager.LoadScene("HouseEscape");
    }

    private void OnCollisionEnter(Collision collision) // COLLISIONES EN DIFERENTES PUNTOS DEL NIVEL PARA LAS RETROALIMENTACIONES
    {
        if (collision.gameObject.GetComponent<UnityChan.UnityChanControlScriptWithRgidBody>())
        {
            if(gameObject.tag == "Meta")
            {
                collision.gameObject.GetComponent<UnityChan.UnityChanControlScriptWithRgidBody>().unityChanVoice.Stop();
                collision.gameObject.GetComponent<UnityChan.UnityChanControlScriptWithRgidBody>().unityChanVoice.PlayOneShot(nivelCompletado);
                StartCoroutine(RecargarEscena());
            }
            if (gameObject.tag == "Entrada")
            {
                collision.gameObject.GetComponent<UnityChan.UnityChanControlScriptWithRgidBody>().unityChanVoice.Stop();
                collision.gameObject.GetComponent<UnityChan.UnityChanControlScriptWithRgidBody>().unityChanVoice.PlayOneShot(saleDeCasa);
            }
            if (gameObject.tag == "Inicio")
            {
                collision.gameObject.GetComponent<UnityChan.UnityChanControlScriptWithRgidBody>().unityChanVoice.Stop();
                collision.gameObject.GetComponent<UnityChan.UnityChanControlScriptWithRgidBody>().unityChanVoice.PlayOneShot(comienzo);
            }
        }
    }
}
