using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{

    [SerializeField]
    private RectTransform enduranceFill;

    [SerializeField]
    private RectTransform healthBarFill;

    [SerializeField]
    private Text ammoText;

    [SerializeField]
    private GameObject pauseMenu;

    [SerializeField]
    private GameObject scoreboard;

    [SerializeField]
    private Text nameText;

    private Camera playerCamera;
    [SerializeField]
    private LayerMask mask;

    [SerializeField]
    private Image crosshair;

    [SerializeField]
    private Text topScoreText;
    [SerializeField]
    private Text youScoreText;

    [SerializeField]
    private GameObject endPanel;
    [SerializeField]
    private Text winnerText;

    [SerializeField]
    private GameObject killfeed;

    private Player player;
    private PlayerController controller;
    private WeaponManager weaponManager;

    public void SetPlayer(Player _player)
    {
        player = _player;
        controller = player.GetComponent<PlayerController>();
        weaponManager = player.GetComponent<WeaponManager>();
        playerCamera = player.GetComponentInChildren<Camera>();
    }

    private void Start()
    {
        PauseMenu.IsOn = false;
        nameText.text = player.name;
    }

    private void Update()
    {
        SetEnduranceAmount(controller.GetEndurancePct());
        SetHealthAmount(player.GetHealthPct());
        SetAmmoAmount(weaponManager.GetCurrentWeapon().bullets, weaponManager.GetCurrentWeapon().maxBullets);
        SetCrosshairColor();
        UpdateScore();

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }

        if(Input.GetKeyDown(KeyCode.Tab))
        {
            scoreboard.SetActive(true);
        } else if(Input.GetKeyUp(KeyCode.Tab))
        {
            scoreboard.SetActive(false);
        }
    }

    private void TogglePauseMenu()
    {
        pauseMenu.SetActive(!pauseMenu.activeSelf);

        PauseMenu.IsOn = pauseMenu.activeSelf;
    }

    private void SetEnduranceAmount(float _amount)
    {
        enduranceFill.localScale = new Vector3(_amount, 1f, 1f);
    }
    private void SetHealthAmount(float _amount)
    {
        healthBarFill.localScale = new Vector3(_amount, 1f, 1f);
    }
    private void SetAmmoAmount(int _amount, int _maxAmount)
    {
        ammoText.text = _amount + " / " + _maxAmount;
    }

    private void SetCrosshairColor()
    {
        RaycastHit _hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out _hit, 1000, mask))
        {
            if(_hit.collider.tag=="Player")
            {
                crosshair.color = Color.red;
            }
            else
            {
                crosshair.color = Color.white;
            }
        }
        else
        {
            crosshair.color = Color.white;
        }
    }

    private void UpdateScore()
    {
        topScoreText.text = GameManager.ReturnTopScore().ToString();
        youScoreText.text = GameManager.ReturnPlayerScore(player.name).ToString();
    }

    public void UIEnd()
    {
        enduranceFill.parent.gameObject.SetActive(false);
        healthBarFill.parent.gameObject.SetActive(false);
        nameText.transform.parent.gameObject.SetActive(false);
        ammoText.transform.parent.gameObject.SetActive(false);
        crosshair.gameObject.SetActive(false);
        youScoreText.transform.parent.gameObject.SetActive(false);
        killfeed.SetActive(false);
        endPanel.SetActive(true);
        winnerText.text = GameManager.ReturnTopScorePlayerName();
    }
}