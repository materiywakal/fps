using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[RequireComponent(typeof(PlayerSetup))]
public class Player : NetworkBehaviour {

    [SyncVar]
    private bool _isDead = false;
    public bool isDead
    {
        get { return _isDead; }
        protected set { _isDead = value; }
    }

    [SerializeField]
    private int maxHealth = 100;

    [SyncVar]
    private int currentHealth;

    public int kills;
    public int deaths;

    [SerializeField]
    private Behaviour[] disableOnDeath;
    [SerializeField]
    private GameObject[] disableGameObjectsOnDeath;
    public bool[] wasEnabled;

    [SerializeField]
    private GameObject deathEffect;

    [SerializeField]
    private GameObject spawnEffect;

    [SerializeField]
    private GameObject Graphics;

    private bool firstSetup = true;
    private bool matchEnd = false;

    private void Update()
    {      
        if (isLocalPlayer)
        {
            CmdMatchEnd(GameManager.IsMatchEnd());
            if (matchEnd && !isDead)
            {
                EndMatchForPlayer();
            }
            if(matchEnd)
            {
                GetComponent<PlayerSetup>().playerUIInstance.SetActive(true);
                GetComponent<PlayerSetup>().playerUIInstance.GetComponent<PlayerUI>().UIEnd();
                if (Cursor.lockState != CursorLockMode.None)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
            
        }
    }

    public void SetupPlayer()
    {
        if (isLocalPlayer)
        {
            GameManager.instance.SetSceneCameraActive(false);
            GetComponent<PlayerSetup>().playerUIInstance.SetActive(true);
            CmdBroadCastNewPlayerSetup();
        }
    }

    [Command]
    public void CmdBroadCastNewPlayerSetup()
    {
        RpcSetupPlayerOnAllClients();
    }

    [ClientRpc]
    private void RpcSetupPlayerOnAllClients()
    {
        if (firstSetup)
        {
            wasEnabled = new bool[disableOnDeath.Length];
            for (int i = 0; i < wasEnabled.Length; i++)
            {
                wasEnabled[i] = disableOnDeath[i].enabled;
            }
            firstSetup = false;
        }
        SetDefaults();
    }

    [ClientRpc]
    public void RpcTakeDamage(int _amount, string _sourceID)
    {
        if (isDead)
            return;

        currentHealth -= _amount;

        if (currentHealth <= 0)
        {
            Die(_sourceID);
        }
    }

    private void Die(string _sourceID)
    {
        isDead = true;

        deaths++;
        GameManager.GetPlayer(_sourceID).kills++;

        GameManager.instance.onPlayerKilledCallback(gameObject.transform.name, GameManager.GetPlayer(_sourceID).transform.name);

        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled= false;
        }

        for (int i = 0; i < disableGameObjectsOnDeath.Length; i++)
        {
            disableGameObjectsOnDeath[i].SetActive(false);
        }

        Collider _col = GetComponent<Collider>();
        if (_col != null)
            _col.enabled = false;

        gameObject.GetComponent<Rigidbody>().useGravity = false;

        gameObject.GetComponent<Animator>().enabled = false;

        if (!matchEnd)
        {
            GameObject _gfxIns = (GameObject)Instantiate(deathEffect, transform.position, Quaternion.identity);
            Destroy(_gfxIns, 2f);
        }

        if (isLocalPlayer)
        {
            GameManager.instance.SetSceneCameraActive(true);
            GetComponent<PlayerSetup>().playerUIInstance.SetActive(false);
        }

        Debug.Log(transform.name + "is dead!");

        Transform _spawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = _spawnPoint.position;
        transform.rotation = _spawnPoint.rotation;

        GetComponent<ConfigurableJoint>().targetPosition = new Vector3(0, -transform.position.y + 1, 0);

        if(!matchEnd)
            StartCoroutine(Respawn());
    }
    [Command]
    private void CmdDie(string _sourceID)
    {
        RpcDie(_sourceID);
    }
    [ClientRpc]
    private void RpcDie(string _sorceID)
    {
        Die(_sorceID);
    }

    [Command]
    private void CmdMatchEnd(bool boolean)
    {
        RpcMatchEnd(boolean);
    }
    [ClientRpc]
    private void RpcMatchEnd(bool boolean)
    {
        matchEnd = boolean;
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(GameManager.instance.matchSettings.respawnTime);

        SetupPlayer();
    }

    public void SetDefaults()
    {
        isDead = false;

        currentHealth = maxHealth;

        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled = wasEnabled[i];
        }

        for (int i = 0; i < disableGameObjectsOnDeath.Length; i++)
        {
            disableGameObjectsOnDeath[i].SetActive(true);
        }

        Collider _col = GetComponent<Collider>();
        if (_col != null)
            _col.enabled = true;

        gameObject.GetComponent<Rigidbody>().useGravity = true;  
        gameObject.GetComponent<Animator>().enabled = true;
        if (!matchEnd)
        {
            GameObject _gfxIns = (GameObject)Instantiate(spawnEffect, transform.position, Quaternion.identity);
            Destroy(_gfxIns, 2f);
        }
    }

    public float GetHealthPct()
    {
        return (float)currentHealth / maxHealth;
    }

    public void EndMatchForPlayer()
    {
        CmdDie(transform.name);
        GetComponent<PlayerSetup>().playerUIInstance.SetActive(true);
        GetComponent<PlayerSetup>().playerUIInstance.GetComponent<PlayerUI>().UIEnd();
        if (Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
