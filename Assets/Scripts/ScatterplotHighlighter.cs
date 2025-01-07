using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ScatterplotHighlighter : MonoBehaviour
{
    private Material originalMaterial;
    public Material highlightMaterial; 

    void Start()
    {
        // XRRayInteractorの設定
        var interactor = GetComponent<XRRayInteractor>();
        if (interactor != null)
        {
            interactor.selectEntered.AddListener(OnSelectEntered);
        }
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (args.interactableObject == null) return;

        // 選択されたQuadの情報を取得
        GameObject selectedQuad = args.interactableObject.transform.gameObject;
        string[] pathParts = GetHierarchyPath(selectedQuad).Split('/');

        if (pathParts.Length < 3) return;

        string wall = pathParts[1]; // Right, Left, Floor
        string[] indices = pathParts[2].Split(',');
        int row = int.Parse(indices[0]);
        int col = int.Parse(indices[1]);

        HighlightRelatedQuads(row, col, wall);
    }

    private string GetHierarchyPath(GameObject obj)
    {
        string path = obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = obj.name + "/" + path;
        }
        return path;
    }

    private void HighlightRelatedQuads(int row, int col, string selectedWall)
    {
        // 各壁のルートオブジェクト取得
        Transform rightWall = transform.Find("Right");
        Transform leftWall = transform.Find("Left");
        Transform floor = transform.Find("Floor");

        // ハイライトをリセット
        ResetAllHighlights();

        // 選択された壁に応じてハイライト
        switch (selectedWall)
        {
            case "Right":
                HighlightRow(rightWall, row);
                HighlightColumn(rightWall, col);
                HighlightRow(floor, row);
                HighlightColumn(leftWall, row);
                break;
            case "Left":
                HighlightRow(leftWall, row);
                HighlightColumn(leftWall, col);
                HighlightColumn(rightWall, row);
                HighlightRow(floor, col);
                break;
            case "Floor":
                HighlightRow(floor, row);
                HighlightColumn(floor, col);
                HighlightRow(rightWall, row);
                HighlightColumn(leftWall, col);
                break;
        }
    }

    private void HighlightRow(Transform wall, int row)
    {
        for (int i = 0; i < 8; i++)
        {
            Transform quad = wall.Find($"{row},{i}");
            if (quad != null)
            {
                AddHighlight(quad.gameObject);
            }
        }
    }

    private void HighlightColumn(Transform wall, int col)
    {
        for (int i = 0; i < 8; i++)
        {
            Transform quad = wall.Find($"{i},{col}");
            if (quad != null)
            {
                AddHighlight(quad.gameObject);
            }
        }
    }

    private void AddHighlight(GameObject quad)
    {
        Renderer renderer = quad.GetComponent<Renderer>();
        Material[] materials = renderer.materials;

        Material[] newMaterials = new Material[materials.Length + 1];
        materials.CopyTo(newMaterials, 0);
        newMaterials[materials.Length] = highlightMaterial;

        renderer.materials = newMaterials;
    }

    private void ResetAllHighlights()
    {
        ResetWallHighlights(transform.Find("Right"));
        ResetWallHighlights(transform.Find("Left"));
        ResetWallHighlights(transform.Find("Floor"));
    }

    private void ResetWallHighlights(Transform wall)
    {
        foreach (Transform child in wall)
        {
            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material[] materials = renderer.materials;
                if (materials.Length > 1)
                {
                    Material[] newMaterials = new Material[1];
                    newMaterials[0] = materials[0];
                    renderer.materials = newMaterials;
                }
            }
        }
    }
}
