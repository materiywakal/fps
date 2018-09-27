using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class WeaponManager : NetworkBehaviour
{

    [SerializeField]
    private Transform weaponHolder;

    [SerializeField]
    private PlayerWeapon primaryWeapon;
    [SerializeField]
    private PlayerWeapon secondaryWeapon;

    private PlayerWeapon currentWeapon;
    private WeaponGraphics currentGraphics;

    GameObject _weaponIns;

    public bool isReloading = false;

    private void Start()
    {
        EquipWeapon(primaryWeapon);
    }
    public PlayerWeapon GetCurrentWeapon()
    {
        return currentWeapon;
    }
    public WeaponGraphics GetCurrentGraphics()
    {
        return currentGraphics;
    }
    public void EquipPrimaryWeapon()
    {
        if (weaponHolder.GetChild(0).gameObject != null)
            Destroy(weaponHolder.GetChild(0).gameObject);
        EquipWeapon(primaryWeapon);
    }
    public void EquipSecondaryWeapon()
    {
        if (weaponHolder.GetChild(0).gameObject != null)
            Destroy(weaponHolder.GetChild(0).gameObject);
        EquipWeapon(secondaryWeapon);
    }
    private void EquipWeapon(PlayerWeapon _weapon)
    {
        currentWeapon = _weapon;

        _weaponIns = (GameObject)Instantiate(_weapon.graphics, weaponHolder.position, weaponHolder.rotation);
        _weaponIns.transform.SetParent(weaponHolder);

        currentGraphics = _weaponIns.GetComponent<WeaponGraphics>();

        if (isLocalPlayer)
            Utility.SetLayerRecursively(_weaponIns, LayerMask.NameToLayer("Weapon"));
    }

    public void Reload()
    {
        if (isReloading)
            return;

        StartCoroutine(Reload_Coroutine());
    }

    private IEnumerator Reload_Coroutine()
    {
        CmdOnReload();

        yield return new WaitForSeconds(currentWeapon.reloadTime);

        currentWeapon.bullets = currentWeapon.maxBullets;;

        CmdOnReload();
    }

    [Command]
    private void CmdOnReload()
    {
        RpcOnReload();
    }

    [ClientRpc]
    private void RpcOnReload()
    {
        isReloading = !isReloading;
        if (isReloading)
            currentGraphics.gunAnimator.SetTrigger("IsReloading");
        else
            currentGraphics.gunAnimator.ResetTrigger("IsReloading");
    }
}