using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerColors", menuName = "Player Colors")]
public class PlayerColors : ScriptableObject
{
    public static string Path
    {
        get => "GameSettings/PlayerColors";
    }

    public static PlayerColors Instance
    {
        get
        {
            PlayerColors settings = Resources.Load<PlayerColors>(Path);
            if (settings == null)
            {
                Debug.LogError("Could not find PlayerColors.asset");
            }
            return settings;
        }
    }

    [System.Serializable]
    public struct PlayerColor
    {
        public PlayerIdentifier player;
        public Color color;
    }

    [SerializeField]
    PlayerColor[] playerColors;

    public Dictionary<PlayerIdentifier, Color> PlayerColorsDictionary
    {
        get
        {
            Dictionary<PlayerIdentifier, Color> dictionary = new Dictionary<PlayerIdentifier, Color>();
            foreach (PlayerColor pair in playerColors)
            {
                dictionary.Add(pair.player, pair.color);
            }
            return dictionary;
        }
    }
}
