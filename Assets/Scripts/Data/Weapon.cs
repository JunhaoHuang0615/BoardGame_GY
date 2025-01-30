using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Weapon
{
    public string name; //武器名字

    public float attackAbility; //攻击力 

    public float denfense; //防御力

    public float accuracy; //命中率

    public float speed; //速度

    public Sprite weaponImage;

    public string weaponImagePath; //图片路径

    public string WeaponType; //武器类型


    public Weapon Clone()
    {
        return new Weapon
        {
            name = this.name,
            attackAbility = this.attackAbility,
            denfense = this.denfense,
            accuracy = this.accuracy,
            speed = this.speed,
            weaponImage = this.weaponImage,
            weaponImagePath = this.weaponImagePath,
            WeaponType = this.WeaponType



        };
    }

}
