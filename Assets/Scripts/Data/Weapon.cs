using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Weapon
{
    public string WeaponName { get; set; }
    public string AttackType { get; set; }
    public int Attack { get; set; }
    public float Speed { get; set; }
    public int Defense { get; set; }
    public float Accuracy { get; set; } //命中率
    public string ImagePath { get; set; }

    public int Range { get; set; }
    public string Range_Pattern { get; set; }
    public Sprite WeaponImage { get; set; } // 用于存储加载后的图片


    public Weapon Clone()
    {
        return new Weapon
        {
            WeaponName = this.WeaponName,
            AttackType = this.AttackType,
            Attack = this.Attack,
            Speed = this.Speed,
            Defense = this.Defense,
            Accuracy = this.Accuracy,
            ImagePath = this.ImagePath,
            WeaponImage = this.WeaponImage, // 画像の参照は共有して問題ない
            Range = this.Range,
            Range_Pattern = this.Range_Pattern,
        };
    }
}
