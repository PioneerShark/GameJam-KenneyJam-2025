using System;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using Unity.Burst.Intrinsics;

public class WeaponComponent : MonoBehaviour
{
    private bool primaryFire = true;
    private Vector2 attackDirection = Vector2.zero;

    private Transform attackSpawn;
    private Weapon currentWeapon;
    private int power;
    bool v2, v3, v4, v5 = false;
    bool firing = false;


    [SerializeField] private List<Weapon> weapons;

    void Awake()
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            weapons[i] = ScriptableObject.Instantiate(weapons[i]);
            weapons[i].primaryFire = ScriptableObject.Instantiate(weapons[i].primaryFire);
            weapons[i].secondaryFire = ScriptableObject.Instantiate(weapons[i].secondaryFire);
            weapons[i].primaryFire.weapon = weapons[i];
            weapons[i].secondaryFire.weapon = weapons[i];
        }


        currentWeapon = weapons[0];
        attackDirection.x = 0;
        attackDirection.y = 1;

        InitialiseWeaponStats();
    }

    // Update is called once per frame
    void Update()
    {
        if (attackSpawn == null) return;
        for (int i = 0; i < weapons.Count; i++)
        {
            weapons[i].RegenAmmo();
        }

        
        currentWeapon.lookVector = attackDirection;
        currentWeapon.firepoint = attackSpawn;
        currentWeapon.primaryFire.WeaponUpdate();
    }

    public void AttackStarted(bool primary)
    {
        primaryFire = primary;
        

        if (!currentWeapon.primaryFire.isAttacking && !currentWeapon.secondaryFire.isAttacking)
        {
            if (CheckAttack(currentWeapon.primaryFire) && primaryFire)
            {
                firing = true;
                currentWeapon.primaryFire.AttackStart(this.gameObject);
            }
            if (CheckAttack(currentWeapon.secondaryFire) && !primaryFire)
            {
                currentWeapon.secondaryFire.AttackStart(this.gameObject);
            }
        }
    }

    public void Reload()
    {
        //currentWeapon.primaryFire.TryReload();
        _ = currentWeapon.TryReload();
    }

    public void SetAttackSpawn(Transform setAttackSpawn)
    {
        attackSpawn = setAttackSpawn;
    }
    
    public Vector2 GetFirePoint()
    {
        return new(attackSpawn.localPosition.x, attackSpawn.localPosition.y);
    }

    public void SetAttackDirecton(Vector2 setAttackDirection)
    {
        attackDirection = setAttackDirection;
    }

    public void AttackCancelled(bool primary)
    {

        if (CheckAttack(currentWeapon.primaryFire) && primary)
        {
            currentWeapon.primaryFire.AttackEnd(this.gameObject);
            firing = false;
        }

        if (CheckAttack(currentWeapon.secondaryFire) && !primary)
        {
            currentWeapon.secondaryFire.AttackEnd(this.gameObject);
        }
    }

    public bool AttackCancel(int weapon)
    {

        bool attacking = firing;
        SwapWeapon(weapon);
        return attacking;
        

    }

    public void EquipWeapon(Weapon weapon)
    {
        weapons[1].primaryFire = ScriptableObject.Instantiate(weapon.primaryFire);
        weapons[1].secondaryFire = ScriptableObject.Instantiate(weapon.secondaryFire);
        weapons[1].primaryFire.weapon = weapon;
        weapons[1].secondaryFire.weapon = weapon;

        currentWeapon = weapons[1];
        InitialiseWeaponStats();

    }
    public void SwapWeapon()
    {
        if (weapons[0] == currentWeapon && weapons.Count > 1)
        {
            currentWeapon = weapons[1];
        }
        else
        {
            currentWeapon = weapons[0];
        }
        InitialiseWeaponStats();
    }

    public void SwapWeapon(int weapon)
    {
        AttackCancelled(true);
        currentWeapon = weapons[weapon];
        InitialiseWeaponStats();
    }
    private void InitialiseWeaponStats()
    {

        
    }
    private bool CheckAttack(AttackScriptable attack)
    {
        if (attack == null)
        {
            return false;
        }
        return true;
    }

    public Weapon GetCurrentWeapon()
    {
        return currentWeapon;
    }
    private void AttackStart()
    {
        AttackStarted(true);
    }

    public void UpdatePower(int value)
    {
        power = value;
        bool resume = true;
        switch (power)
        {
            case > 999:
                currentWeapon.primaryFire.damage = (value / 500);
                break;
            case > 799:
                if (v5) return;
                v5 = true;
                resume = AttackCancel(4);
                break;
            case > 599:
                if (v4) return;
                v4 = true;
                resume = AttackCancel(3);
                break;
            case > 399:
                if (v3) return;
                v3 = true;
                resume = AttackCancel(2);
                break;
            case > 199:
                if (v2) return;
                v2 = true;
                resume = AttackCancel(1);
                break;
             
        }
        ;
        if (resume)
        {

            Invoke("AttackStart", 0.3f);
            

        }

    }
}



