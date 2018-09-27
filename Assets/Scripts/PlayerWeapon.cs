using UnityEngine;

[System.Serializable]
public class PlayerWeapon
{
    public string name = "";

    public int damage = 10;
    public float range = 100f;

    public float fireRate = 0f;

    public int maxBullets = 30;
    public int bullets;

    public GameObject graphics;

    public float reloadTime = 2f;

    public PlayerWeapon()
    {
        bullets = maxBullets;
    }
}
