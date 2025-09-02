using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabSwitch : MonoBehaviour
{
    [SerializeField] private GameObject ViewHat;
    [SerializeField] private GameObject ViewWeapon;
    [SerializeField] private GameObject ViewPant;


    public void SwitchToHat()
    {
        ViewHat.SetActive(true);
        ViewWeapon.SetActive(false);
        ViewPant.SetActive(false);
    }
    public void SwitchToWeapon()
    {
        ViewHat.SetActive(false);
        ViewWeapon.SetActive(true);
        ViewPant.SetActive(false);
    }
    public void SwitchToPant()
    {
        ViewHat.SetActive(false);
        ViewWeapon.SetActive(false);
        ViewPant.SetActive(true);
    }

}
