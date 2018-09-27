using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

public class GameManager : NetworkBehaviour{

    public static GameManager instance;

    public MatchSettings matchSettings;

    [SerializeField]
    private GameObject sceneCamera;

    public delegate void OnPlayerKilledCallback(string player, string source);
    public OnPlayerKilledCallback onPlayerKilledCallback;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("more than one gamemanager");
        }
        else
        {
            instance = this;
        }
    }

    public void SetSceneCameraActive(bool _isActive)
    {
        if (sceneCamera == null)
            return;

        sceneCamera.SetActive(_isActive);
    }

    #region Player Tracking

    private static Dictionary<string, Player> players = new Dictionary<string, Player>();

    public static void RegisterPlayer(string _netID, Player _player)
    {
        string _playerID = "Player " + _netID;
        players.Add(_playerID, _player);
        _player.transform.name = _playerID;
    }
    public static void UnRegisterPlayer(string _playerID)
    {
        players.Remove(_playerID);
    }

    public static bool IsRegistered(string _playerID)
    {
        Player[] _players = GetAllPlayers();
        for(int i=0;i<_players.Length;i++)
        {
            if (_players[i].name == _playerID)
                return true;
        }
        return false;
    }

    public static Player GetPlayer(string _playerID)
    {
        return players[_playerID];
    }

    public static Player[] GetAllPlayers()
    {
        return players.Values.ToArray();
    }

    public static int ReturnTopScore()
    {
        Player[] players = GetAllPlayers();
        int score = 0;
        foreach (Player player in players)
        {
            if (score < player.kills)
                score = player.kills;
        }
        return score;
    }

    public static bool IsMatchEnd()
    {
        if (ReturnTopScore() >= GameManager.instance.matchSettings.scoreToWin)
            return true;
        else
            return false;
    }

    public static string ReturnTopScorePlayerName()
    {
        Player[] players = GetAllPlayers();
        int score = 0;
        string name = "";
        foreach (Player player in players)
        {
            if (score < player.kills)
            {
                score = player.kills;
                name = player.name;
            }
        }
        return name;
    }

    public static int ReturnPlayerScore(string _playerID)
    {
        return players[_playerID].kills;
    }
    #endregion 
}
