using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Samples.SpatialKeyboard;

public static class Lab4UiValidation
{
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Lab4.unity");
        var cameras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        Require(cameras.Count(c => c.enabled && c.CompareTag("MainCamera")) == 1,
            "Expected one enabled MainCamera.");
        var camera = cameras.Single(c => c.enabled && c.CompareTag("MainCamera"));
        var manager = UnityEngine.Object.FindFirstObjectByType<GlobalNonNativeKeyboard>();
        Require(manager != null && manager.cameraTransform == camera.transform,
            "Keyboard camera must match active rig.");
        Require(manager.keyboardPrefab != null, "Keyboard prefab missing.");
        var tablet = GameObject.Find("TabletCanvas");
        Require(tablet.GetComponent<Canvas>().worldCamera == camera, "Canvas camera mismatch.");
        Require(Mathf.Approximately(tablet.transform.localScale.x, 0.00115f), "Tablet scale mismatch.");
        var close = tablet.GetComponentsInChildren<Button>(true).Single(b => b.name == "CloseButton");
        Require(close.onClick.GetPersistentMethodName(0) == "Close" &&
            close.onClick.GetPersistentTarget(0) is SpatialTablet, "Close callback missing.");
        Require(close.GetComponent<RectTransform>().sizeDelta.y == 90f, "Close target too small.");
        var display = tablet.GetComponentInChildren<XRKeyboardDisplay>(true);
        Require(display != null && display.inputField != null, "Keyboard input binding missing.");
        var pokes = UnityEngine.Object.FindObjectsByType<XRPokeInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(p => p.transform.root.name == "XR Origin Hands (XR Rig)").ToArray();
        Require(pokes.Length == 2, "Expected two hand poke interactors.");
        foreach (var poke in pokes)
        {
            var serialized = new SerializedObject(poke);
            Require(serialized.FindProperty("m_EnableUIInteraction").boolValue, "Poke UI disabled.");
            Require(serialized.FindProperty("m_ClickUIOnDown").boolValue, "Poke click-on-down disabled.");
        }
        foreach (var root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject) == 0,
                    "Missing script: " + transform.name);
        Debug.Log("LAB4_UI_VALIDATION_PASSED: one active camera, keyboard, Close, hand poke and scene scripts.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new Exception(message);
    }
}
