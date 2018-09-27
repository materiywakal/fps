using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreBoard : MonoBehaviour {

    [SerializeField]
    GameObject playerScoreboardItemPrefab;

    [SerializeField]
    Transform playerList;

	private void OnEnable()
    {
        Player[] players = GameManager.GetAllPlayers();

        foreach(Player player in players)
        {
            GameObject itemGO = (GameObject)Instantiate(playerScoreboardItemPrefab, playerList);
            PlayerScoreboardItem item = itemGO.GetComponent<PlayerScoreboardItem>();
            if (item != null)
                item.Setup(player.name, player.kills.ToString(), player.deaths.ToString());
        }
    }
    private void OnDisable()
    {
        foreach(Transform child in playerList)
        {
            Destroy(child.gameObject);
        }
    }
}
