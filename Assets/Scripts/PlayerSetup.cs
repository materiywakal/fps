using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerController))]
public class PlayerSetup : NetworkBehaviour
{

    [SerializeField]
    Behaviour[] componentsToDisable;

    [SerializeField]
    string remoteLayerName = "RemotePlayer";

    [SerializeField]
    string dontDrawLayerName = "DontDraw";
    [SerializeField]
    GameObject playerGraphics;

    [SerializeField]
    GameObject playerUIPrefab;

    public GameObject playerUIInstance;

    private void Start()
    {
        //выключаем компоненты на других игроках,
        //которые мы можем использовать
        if (!isLocalPlayer)
        {
            DisableComponents();
            AssignRemoteLayer();
        }
        else
        {

            //выключаем графику для локального персонажа
            SetLayerRecursively(playerGraphics,LayerMask.NameToLayer(dontDrawLayerName));

            //Создаем интерфейс
            playerUIInstance = Instantiate(playerUIPrefab);
            playerUIInstance.name = playerUIPrefab.name;

            //Настраиваем интерфейс
            PlayerUI ui = playerUIInstance.GetComponent<PlayerUI>();
            if (ui != null)
                ui.SetPlayer(GetComponent<Player>());

            GetComponent<Player>().SetupPlayer();

        }
    }
    public void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach(Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
    public override void OnStartClient()
    {
        base.OnStartClient();

        string _netID = GetComponent<NetworkIdentity>().netId.ToString();
        Player _player = GetComponent<Player>();

        GameManager.RegisterPlayer(_netID, _player);
    }
    private void AssignRemoteLayer()
    {
        SetLayerRecursively(gameObject, LayerMask.NameToLayer(remoteLayerName));
    }
    private void DisableComponents()
    {
        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsToDisable[i].enabled = false;
        }
    }
    //Когда персонаж не в игре
    private void OnDisable()
    {
        Destroy(playerUIInstance);

        //включаем камеру сцены
        if(isLocalPlayer)
            GameManager.instance.SetSceneCameraActive(true);

        GameManager.UnRegisterPlayer(transform.name);
    }
}
