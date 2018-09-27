using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[RequireComponent(typeof(WeaponManager))]
public class PlayerShoot : NetworkBehaviour
{

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private LayerMask mask;

    private PlayerWeapon currentWeapon;
    private WeaponManager weaponManager;
    private PlayerMovement playerMovement;

    private void Start()
    {
        if (cam == null)
        {
            Debug.LogError("Playershoot: no cam");
        }
        weaponManager = GetComponent<WeaponManager>();
        playerMovement = GetComponent<PlayerMovement>();
    }
    private void Update()
    {
        currentWeapon = weaponManager.GetCurrentWeapon();

        if (PauseMenu.IsOn)
            return;

        if(Input.GetKeyDown(KeyCode.R))
        {
            if (currentWeapon.bullets < currentWeapon.maxBullets)
            {
                weaponManager.Reload();
                return;
            }
        }

        if (currentWeapon.fireRate == 0f)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Shoot();
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                InvokeRepeating("Shoot", 0f, 1f/currentWeapon.fireRate);
            }
            else if(Input.GetButtonUp("Fire1"))
            {
                CancelInvoke("Shoot");
            }
        }
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            weaponManager.EquipPrimaryWeapon();
        }
        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            weaponManager.EquipSecondaryWeapon();
        }
    }
    //Вызывается на сервере, когда игрок стреляет
    [Command]
    private void CmdOnShoot()
    {
        RpcDoShootEffect();
    }
    //Вызывается на всех клиентах когда нужно сделать
    //эффект выстрела
    [ClientRpc]
    private void RpcDoShootEffect()
    {
        weaponManager.GetCurrentGraphics().muzzleFlash.Play();
        weaponManager.GetCurrentGraphics().gunAnimator.Play("Shoot");
    }
    //Вызывается на сервере когда игрок поподает во что-то
    //передаются координаты попадания, и нормаль поверхности
    [Command]
    private void CmdOnHit(Vector3 _pos, Vector3 _normal)
    {
        RpcDoHitEffect(_pos, _normal);
    }
    //Вызывается на всех клиентах, чтобы запустить эффект попадания
    [ClientRpc]
    private void RpcDoHitEffect(Vector3 _pos, Vector3 _normal)
    {
        GameObject _hitEffect = (GameObject)Instantiate(weaponManager.GetCurrentGraphics().hitEffectPrefab, _pos, Quaternion.LookRotation(_normal));
        Destroy(_hitEffect, 1f);
    }
    [Client]
    private void Shoot()
    {
        if (!isLocalPlayer || weaponManager.isReloading)
            return;

        if(currentWeapon.bullets<=0)
        {
            weaponManager.Reload();
            return;
        }       
        currentWeapon.bullets--;

        //Вызываем метод OnShoot на сервере
        CmdOnShoot();

        RaycastHit _hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out _hit, currentWeapon.range, mask))
        {
            if (_hit.collider.tag == "Player")
            {
                CmdPlayerShot(_hit.collider.name, currentWeapon.damage, transform.name);
            }

            //Мы попали во что-то, поэтому вызываем метод OnHit на сервере
            CmdOnHit(_hit.point, _hit.normal);
        }

        if (currentWeapon.bullets <= 0)
        {
            weaponManager.Reload();
        }
    }

    [Command]
    private void CmdPlayerShot(string _playerID, int _damage, string _sourceID)
    {

        Player _player = GameManager.GetPlayer(_playerID);
        _player.RpcTakeDamage(_damage, _sourceID);
    }
}
