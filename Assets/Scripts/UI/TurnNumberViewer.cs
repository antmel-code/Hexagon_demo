using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnNumberViewer : MonoBehaviour
{
    [SerializeField]
    Text text;

    private void Awake()
    {
        if (!text)
        {
            Debug.LogError("Text component isn't assigned");
            Destroy(this);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        GameMaster.onTurnStart += UpdateView;
    }

    private void OnDestroy()
    {
        GameMaster.onTurnStart -= UpdateView;
    }

    // Update is called once per frame
    void UpdateView()
    {
        text.text = GameMaster.Instance.TurnNumber.ToString();
    }
}
