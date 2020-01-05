using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(PropSpawner))]
public class PropSpawnerEditor : Editor
{
    private PropSpawner _spawner;
    private VisualElement _RootElement;
    private VisualTreeAsset _VisualTree;

    private List<Editor> objectPreviewEditors;

    public void OnEnable()
    {
        _spawner = (PropSpawner)target;

        _RootElement = new VisualElement();
        _VisualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Editor/PropSpawnerTemplate.uxml");
        
        //Load the style
        StyleSheet stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/PropSpawnerStyles.uss");
        _RootElement.styleSheets.Add(stylesheet);

    }


    public override VisualElement CreateInspectorGUI()
    {
        //Clear the visual element
        _RootElement.Clear();

        //Clone the visual tree into our Visual Element so it can be drawn
        _VisualTree.CloneTree(_RootElement);

        //Add A Callback For Each Button
        UQueryBuilder<VisualElement> builder = _RootElement.Query(classes: new string[] { "prefab-button" });
        builder.ForEach(AddButtonIcon);

        return _RootElement;
    }

    public void AddButtonIcon(VisualElement button)
    {
        IMGUIContainer icon = new IMGUIContainer(() =>
        {
            //Get the object path from the name
            string path = "Assets/Props/" + button.name + ".prefab";

            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            //Return and existing editor, or create a new one.
            Editor editor = GetPreviewEditor(asset);

            //Create a preview from this editor
            editor.OnPreviewGUI(GUILayoutUtility.GetRect(90, 90), null);
        });

        icon.focusable = false;

        //Add the icon into the icon element.
        button.hierarchy.ElementAt(0).Add(icon);

        //Spawn the prefab when the button is clicked
        button.RegisterCallback<MouseDownEvent>(evnt =>
        {
            SpawnPrefab(button.name);
        }, TrickleDown.TrickleDown);

    }

    /// <summary>
    /// Spawns a prefab from the props folder into the spawner object.
    /// </summary>
    /// <param name="prefabToSpawn"></param>
    private void SpawnPrefab(string prefabToSpawn)
    {
        string path = "Assets/Props/" + prefabToSpawn + ".prefab";

        GameObject gameObject = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));
        gameObject.transform.SetParent(_spawner.transform);
        gameObject.transform.localPosition = new Vector3(0, 0, 0);

    }

    /// <summary>
    /// Checks through the list of editors for the prefabs and returns the editor for preview.
    /// </summary>
    /// <param name="asset">The GameObject to preview.</param>
    /// <returns></returns>
    public Editor GetPreviewEditor(GameObject asset)
    {
        if (objectPreviewEditors == null)
        {
            objectPreviewEditors = new List<Editor>();
        }

        //Check if there's already a preview
        foreach(Editor editor in objectPreviewEditors)
        {
            if((GameObject)editor.target == asset)
            {
                return editor;
            }
        }

        Editor newEditor = Editor.CreateEditor(asset);
        objectPreviewEditors.Add(newEditor);
        return newEditor;

    }

    private void OnDisable()
    {
        //Cleanup the previews
        foreach(Editor e in objectPreviewEditors) {
            Editor.DestroyImmediate(e);
        }
    }
}
