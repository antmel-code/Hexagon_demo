using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CellResourceViewer : MonoBehaviour
{
    [SerializeField]
    RectTransform parrentObject;

    [SerializeField]
    ResourceViewer resourceViewerPrefab;

    public HexCell cell;

    List<GameObject> viewers = new List<GameObject>();

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        Clear();
        foreach (ResourceType res in cell.Resources)
        {
            ResourceViewer newViewer = Instantiate(resourceViewerPrefab, parrentObject);
            Image image = newViewer.ResourceSprite;
            image.sprite = GameDataPresenter.Instance.GetSpriteByResourceType(res);
            viewers.Add(newViewer.gameObject);
        }
    }

    void Clear()
    {
        foreach (GameObject viewer in viewers)
        {
            Destroy(viewer);
        }
    }
}
