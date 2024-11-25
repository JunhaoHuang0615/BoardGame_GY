using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitData 
{
    public int ID { get; set; } // 单位的唯一标识符
    public string Name { get; set; } // 单位名称
    public CharacterType Type { get; set; } // 单位类型（如Soldier等）
    public int FlyHeight { get; set; } // 单位的飞行高度
    public int MaxHealth { get; set; } // 最大生命值
    public int Defense { get; set; } // 防御力
    public int MoveRange { get; set; } // 移动范围
    public int MoveSpeed { get; set; } // 移动速度
    public PawnType PawnType { get; set; }

    // 如果需要克隆功能
    public UnitData Clone()
    {
        return new UnitData
        {
            ID = this.ID,
            Name = this.Name,
            Type = this.Type,
            FlyHeight = this.FlyHeight,
            MaxHealth = this.MaxHealth,
            Defense = this.Defense,
            MoveRange = this.MoveRange,
            MoveSpeed = this.MoveSpeed,
            PawnType = this.PawnType
        };
    }
}
