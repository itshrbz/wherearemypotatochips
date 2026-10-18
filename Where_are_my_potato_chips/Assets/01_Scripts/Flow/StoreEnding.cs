using System.Collections;
using UnityEngine;

/// <summary>Vuelta a la tienda real después del tercer espacio: el bucle vuelve a empezar.</summary>
public class StoreEnding : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return new WaitForSeconds(4f);

        var queue = FindAnyObjectByType<BuyerQueue>(FindObjectsInactive.Include);
        if (queue != null) yield return queue.PlayEnding();
        else yield return DialogueBox.SayAll("???", new[] { "Potatos." }, 3f);

        yield return new WaitForSeconds(1f);
        LevelFlowManager.FinishGame();
    }
}
