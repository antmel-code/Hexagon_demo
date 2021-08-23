using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HexSelector : MonoBehaviour
{
    public System.Action<HexCoordinates> OnMouseUp = (HexCoordinates coords) => { };
    public System.Action<HexCoordinates> OnMouseDown = (HexCoordinates coords) => { };
    public System.Action<HexCoordinates> OnDrag = (HexCoordinates coords) => { };

    [SerializeField]
    Camera camera;

    [SerializeField]
    HexGridViewer gridViewer;

    HexCoordinates currentCenter = new HexCoordinates(0, 0);

    List<HexGridViewer> gridViewers = new List<HexGridViewer>();

    [SerializeField]
    int influence = 3;

    [SerializeField]
    float fadeDistance = 20f;

    [SerializeField]
    float highlightDistance = 10f;

    // Start is called before the first frame update
    void Start()
    {
        int Summ(int a, int n) {
            int summ = 0;
            for (int i = a; i <= n; i++)
            {
                summ += i;
            }
            return summ;
        }

        int selectorsCount = Summ(1, influence - 1) * 6 + 1;

        for (int i = 0; i < selectorsCount; i++)
        {
            gridViewers.Add(Instantiate(gridViewer));
        }
    }

    void UpdateHeighlighting(Vector3 center)
    {
        foreach (HexGridViewer hexViewer in gridViewers)
        {
            Vector3 position = hexViewer.transform.position;
            Vector3 distanceVector = hexViewer.transform.position - center;
            float distance = distanceVector.magnitude;
            float fade = 1 - Mathf.Clamp01(distance / fadeDistance);
            float highlight = 1 - Mathf.Clamp01(distance / highlightDistance);
            hexViewer.SetOpacity(fade);
            hexViewer.SetHighlight(highlight);
            if (HexCoordinates.FromPosition(position) == currentCenter)
                hexViewer.SetWaves(1);
            else
                hexViewer.SetWaves(0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        RaycastHit hit;
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);

        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(pointerData, results);

        bool tuchUI = results.Count > 0;
        
        if (!tuchUI && Physics.Raycast(ray, out hit))
        {
            HexCoordinates pointedHexCoords = HexCoordinates.FromPosition(hit.point);

            if (currentCenter != pointedHexCoords)
                OnDrag.Invoke(pointedHexCoords);
            if (Input.GetMouseButtonDown(0))
                OnMouseDown.Invoke(pointedHexCoords);
            if (Input.GetMouseButtonUp(0))
                OnMouseUp.Invoke(pointedHexCoords);

            RefreshHexSelectors(pointedHexCoords);
            UpdateHeighlighting(hit.point);
        }
    }

    void RefreshHexSelectors(HexCoordinates center)
    {
        if (currentCenter == center)
            return;

        currentCenter = center;

        int offset = influence - 1;
        int i = 0;

        for (int r = 0, z = center.Z - offset; z <= center.Z; z++, r++)
        {
            for (int x = center.X - r; x <= center.X + offset; x++, i++)
            {
                if (HexMapMaster.Instance.GetCellOnMap(new HexCoordinates(x, z)))
                    gridViewers[i].transform.position = HexMapMaster.Instance.GetCellOnMap(new HexCoordinates(x, z)).transform.position;
            }
        }
        for (int r = 0, z = center.Z + offset; z > center.Z; z--, r++)
        {
            for (int x = center.X - offset; x <= center.X + r; x++, i++)
            {
                if (HexMapMaster.Instance.GetCellOnMap(new HexCoordinates(x, z)))
                    gridViewers[i].transform.position = HexMapMaster.Instance.GetCellOnMap(new HexCoordinates(x, z)).transform.position;
            }
        }
    }
}
