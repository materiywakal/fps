using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class KillfeedItem : MonoBehaviour
{
    [SerializeField]
    Text text;

    public void Setup(string player, string source)
    {
        text.text = source + " killed " + player;
    }
}
