using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class Killfeed : MonoBehaviour
{
    [SerializeField]
    GameObject killfeedItemPrefab;

    private void Start()
    {
        GameManager.instance.onPlayerKilledCallback += OnKill;
    }

    public void OnKill(string player, string source)
    {
        GameObject go = (GameObject)Instantiate(killfeedItemPrefab, this.transform);
        go.GetComponent<KillfeedItem>().Setup(player, source);
        go.transform.SetAsFirstSibling();

        Destroy(go, 4f);
    }
}
