#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// A-04: genera UI_Game con Logo, Crosshair, EnergyBar y DialogueBox.
// Uso: Tools > Kit > Generar UI A-04 (con tu escena sandbox abierta).
public static class UIGameGenerator
{
    const string CarpetaPadre = "Assets/02_Prefabs";
    const string CarpetaUI = "Assets/02_Prefabs/UI";
    const string RutaPrefab = "Assets/02_Prefabs/UI/UI_Game.prefab";

    [MenuItem("Tools/Kit/Generar UI A-04")]
    static void Generar()
    {
        TMP_FontAsset fuente = BuscarFuente();

        // 1. Canvas raiz (reutiliza UI_Game si ya existe)
        GameObject root = GameObject.Find("UI_Game");
        if (root == null)
        {
            root = new GameObject("UI_Game", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(root, "UI_Game");
            root.layer = LayerMask.NameToLayer("UI");
        }

        Canvas canvas = root.GetComponent<Canvas>();
        if (canvas == null) canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        if (root.GetComponent<GraphicRaycaster>() == null) root.AddComponent<GraphicRaycaster>();

        // 2. Limpieza: quita el boton de prueba y lo que se va a regenerar
        BorrarHijo(root.transform, "Button");
        BorrarHijo(root.transform, "Crosshair");
        BorrarHijo(root.transform, "EnergyBar");
        BorrarHijo(root.transform, "DialogueBox");

        // 3. Logo (solo si falta)
        if (root.transform.Find("Logo") == null)
        {
            GameObject logo = Crear("Logo", root.transform);
            RectTransform lrt = logo.GetComponent<RectTransform>();
            lrt.anchorMin = lrt.anchorMax = lrt.pivot = new Vector2(0.5f, 1f);
            lrt.anchoredPosition = new Vector2(0, -80);
            lrt.sizeDelta = new Vector2(1400, 160);
            TextMeshProUGUI t = logo.AddComponent<TextMeshProUGUI>();
            t.text = "Where Are My Potato Chips?";
            t.fontSize = 100;
            t.alignment = TextAlignmentOptions.Center;
            t.color = new Color(0.55f, 0.05f, 0.05f);
            if (fuente != null) t.font = fuente;
        }

        // 4. Punto de mira
        GameObject cross = Crear("Crosshair", root.transform);
        RectTransform crt = cross.GetComponent<RectTransform>();
        crt.anchorMin = crt.anchorMax = crt.pivot = new Vector2(0.5f, 0.5f);
        crt.anchoredPosition = Vector2.zero;
        crt.sizeDelta = new Vector2(8, 8);
        Image cimg = cross.AddComponent<Image>();
        cimg.color = Color.white;
        cimg.raycastTarget = false;

        // 5. Barra de energia (abajo a la izquierda)
        GameObject bar = Crear("EnergyBar", root.transform);
        RectTransform brt = bar.GetComponent<RectTransform>();
        brt.anchorMin = brt.anchorMax = brt.pivot = new Vector2(0, 0);
        brt.anchoredPosition = new Vector2(40, 40);
        brt.sizeDelta = new Vector2(300, 25);

        GameObject fondo = Crear("Background", bar.transform);
        Estirar(fondo);
        Image fondoImg = fondo.AddComponent<Image>();
        fondoImg.color = new Color(0, 0, 0, 0.6f);
        fondoImg.raycastTarget = false;

        GameObject area = Crear("Fill Area", bar.transform);
        Estirar(area);

        GameObject fill = Crear("Fill", area.transform);
        Estirar(fill);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(1f, 0.85f, 0.1f);
        fillImg.raycastTarget = false;

        Slider slider = bar.AddComponent<Slider>();
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.interactable = false;
        slider.transition = Selectable.Transition.None;
        slider.value = 1;

        // 6. Caja de dialogo (abajo al centro)
        GameObject caja = Crear("DialogueBox", root.transform);
        RectTransform drt = caja.GetComponent<RectTransform>();
        drt.anchorMin = drt.anchorMax = drt.pivot = new Vector2(0.5f, 0f);
        drt.anchoredPosition = new Vector2(0, 40);
        drt.sizeDelta = new Vector2(1200, 200);
        Image dimg = caja.AddComponent<Image>();
        dimg.color = new Color(0, 0, 0, 0.75f);
        dimg.raycastTarget = false;

        GameObject texto = Crear("DialogueText", caja.transform);
        RectTransform trt = texto.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(30, 20);
        trt.offsetMax = new Vector2(-30, -20);
        TextMeshProUGUI dt = texto.AddComponent<TextMeshProUGUI>();
        dt.text = "Where are my potato chips?";
        dt.fontSize = 48;
        dt.alignment = TextAlignmentOptions.MidlineLeft;
        dt.color = Color.white;
        dt.raycastTarget = false;
        if (fuente != null) dt.font = fuente;

        // 7. Guardar como prefab
        if (!AssetDatabase.IsValidFolder(CarpetaUI))
            AssetDatabase.CreateFolder(CarpetaPadre, "UI");
        PrefabUtility.SaveAsPrefabAssetAndConnect(root, RutaPrefab, InteractionMode.AutomatedAction);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = root;
        Debug.Log("A-04: UI_Game generado y guardado en " + RutaPrefab);
    }

    static TMP_FontAsset BuscarFuente()
    {
        string[] guids = AssetDatabase.FindAssets("Creepster t:TMP_FontAsset");
        if (guids.Length == 0) return null;
        return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }

    static GameObject Crear(string nombre, Transform padre)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, nombre);
        go.transform.SetParent(padre, false);
        go.layer = LayerMask.NameToLayer("UI");
        return go;
    }

    static void Estirar(GameObject go)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void BorrarHijo(Transform padre, string nombre)
    {
        Transform t = padre.Find(nombre);
        if (t != null) Undo.DestroyObjectImmediate(t.gameObject);
    }
}
#endif
