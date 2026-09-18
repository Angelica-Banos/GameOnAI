using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.UI;
using TMPro;

public class SetupGameAutomated : EditorWindow
{
    [MenuItem("GameOnAI/Configurar Sprites y Diálogos Automático")]
    public static void SetupAll()
    {
        Debug.Log("Iniciando configuración automática...");

        // 1. Setup Player Prefab
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/objetos/player_prefab.prefab");
        if (playerPrefab != null)
        {
            SetupCharacter(playerPrefab, "Assets/Characters/Characters textures/Jugador/IMG_8120.png", "IMG_8120_0", "PlayerAnimController");
        }
        else
        {
            Debug.LogError("No se encontró el prefab del jugador en Assets/objetos/player_prefab.prefab");
        }

        // 2. Setup Enemy Prefab
        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/objetos/enemy_prefab.prefab");
        if (enemyPrefab != null)
        {
            SetupCharacter(enemyPrefab, "Assets/Characters/Characters textures/NPCs - Redes Sociales/Facebook/Sprite_Facebook.png", "Sprite_Facebook_0", "EnemyAnimController");
        }
        else
        {
            Debug.LogError("No se encontró el prefab del enemigo en Assets/objetos/enemy_prefab.prefab");
        }

        // 3. Create Dialogue Canvas
        CreateDialogueCanvas();

        AssetDatabase.SaveAssets();
        Debug.Log("¡Configuración automática terminada!");
    }

    private static void SetupCharacter(GameObject prefab, string texturePath, string spriteName, string controllerName)
    {
        // Add SpriteRenderer if missing
        SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
        if (sr == null) sr = prefab.AddComponent<SpriteRenderer>();

        // Find sprite
        Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
        Sprite targetSprite = null;
        foreach (var asset in allAssets)
        {
            if (asset is Sprite && asset.name == spriteName)
            {
                targetSprite = asset as Sprite;
                break;
            }
        }

        if (targetSprite != null)
        {
            sr.sprite = targetSprite;

            // Ajustar el collider al tamaño del nuevo sprite
            CapsuleCollider2D capsule = prefab.GetComponent<CapsuleCollider2D>();
            if (capsule != null)
            {
                capsule.size = targetSprite.bounds.size;
                capsule.offset = targetSprite.bounds.center;
            }

            BoxCollider2D box = prefab.GetComponent<BoxCollider2D>();
            if (box != null)
            {
                box.size = targetSprite.bounds.size;
                box.offset = targetSprite.bounds.center;
            }
        }
        else
        {
            Debug.LogWarning("No se encontró el sprite " + spriteName + " en " + texturePath);
        }

        // Setup Animator
        Animator animator = prefab.GetComponent<Animator>();
        if (animator == null) animator = prefab.AddComponent<Animator>();

        // Create Animator Controller
        string controllerPath = "Assets/script/" + controllerName + ".controller";
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);
            controller.AddParameter("MoveY", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);

            // Create a basic state
            AnimatorStateMachine sm = controller.layers[0].stateMachine;
            AnimatorState idleState = sm.AddState("Idle");
            sm.defaultState = idleState;
        }

        animator.runtimeAnimatorController = controller;

        EditorUtility.SetDirty(prefab);
        Debug.Log("Configurado prefab: " + prefab.name);
    }

    private static void CreateDialogueCanvas()
    {
        if (Object.FindFirstObjectByType<DialogueManager>() != null)
        {
            Debug.Log("DialogueManager ya existe en la escena.");
            return;
        }

        GameObject canvasObj = new GameObject("DialogueCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject panelObj = new GameObject("DialoguePanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 0.3f);
        panelRect.offsetMin = new Vector2(0, 0);
        panelRect.offsetMax = new Vector2(0, 0);

        GameObject nameObj = new GameObject("NameText");
        nameObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = "Nombre";
        nameText.fontSize = 32;
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.2f, 0.8f);
        nameRect.anchorMax = new Vector2(0.8f, 1f);
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;

        GameObject textObj = new GameObject("DialogueText");
        textObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI dialogText = textObj.AddComponent<TextMeshProUGUI>();
        dialogText.text = "Hola, este es un diálogo de prueba.";
        dialogText.fontSize = 24;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.2f, 0.1f);
        textRect.anchorMax = new Vector2(0.8f, 0.8f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        GameObject leftImgObj = new GameObject("LeftImage");
        leftImgObj.transform.SetParent(panelObj.transform, false);
        Image leftImg = leftImgObj.AddComponent<Image>();
        RectTransform leftRect = leftImgObj.GetComponent<RectTransform>();
        leftRect.anchorMin = new Vector2(0.02f, 0.1f);
        leftRect.anchorMax = new Vector2(0.18f, 0.9f);
        leftRect.offsetMin = Vector2.zero;
        leftRect.offsetMax = Vector2.zero;

        GameObject rightImgObj = new GameObject("RightImage");
        rightImgObj.transform.SetParent(panelObj.transform, false);
        Image rightImg = rightImgObj.AddComponent<Image>();
        RectTransform rightRect = rightImgObj.GetComponent<RectTransform>();
        rightRect.anchorMin = new Vector2(0.82f, 0.1f);
        rightRect.anchorMax = new Vector2(0.98f, 0.9f);
        rightRect.offsetMin = Vector2.zero;
        rightRect.offsetMax = Vector2.zero;
        rightImgObj.SetActive(false);

        DialogueManager manager = canvasObj.AddComponent<DialogueManager>();
        manager.dialoguePanel = panelObj;
        manager.nameText = nameText;
        manager.dialogueText = dialogText;
        manager.leftCharacterImage = leftImg;
        manager.rightCharacterImage = rightImg;

        panelObj.SetActive(false);

        Debug.Log("Canvas de Diálogo creado en la escena.");
    }
}
