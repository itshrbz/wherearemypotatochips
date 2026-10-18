using System.Reflection;
using UnityEngine;

/// <summary>
/// Hornea el NavMesh de un NavMeshSurface recién creado por los builders de niveles
/// y lo guarda como asset junto a la escena (igual que el botón Bake del Inspector).
/// </summary>
public static class NivelBake
{
    public static void Hornear(Component surface)
    {
        var managerType = System.Type.GetType("Unity.AI.Navigation.Editor.NavMeshAssetManager, Unity.AI.Navigation.Editor");
        const BindingFlags any = BindingFlags.Public | BindingFlags.NonPublic;
        var instance = managerType?.GetProperty("instance", any | BindingFlags.Static | BindingFlags.FlattenHierarchy)?.GetValue(null);
        var bake = managerType?.GetMethod("StartBakingSurfaces", any | BindingFlags.Instance);

        if (instance == null || bake == null)
        {
            Debug.LogWarning("No pude hornear el NavMesh automaticamente. Selecciona el objeto NavMesh y presiona Bake.");
            return;
        }

        try
        {
            bake.Invoke(instance, new object[] { new Object[] { surface } });
        }
        catch (TargetInvocationException e)
        {
            Debug.LogWarning("El horneado automatico fallo (" + e.InnerException?.Message + "). Presiona Bake en el objeto NavMesh.");
        }
    }
}
