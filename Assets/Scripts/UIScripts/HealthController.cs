using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthController : MonoBehaviour
{
    public int currentHealth; 
    public int MaxHealthPerLine = 20; //一行的最大值
    public ObjectPool objectPool;
    public int maxHealth; 
    public List<GameObject> healthSlider;

    public TextMeshProUGUI txt_currenthealth;
    public TextMeshProUGUI txt_maxhealth;

    void Awake()
    {   
        if(objectPool == null)
        {
            objectPool = FindObjectOfType<ObjectPool>();
        }
        //Initialize_Healthbar(20,20);
    }
    void Update()
    {
/*        if (Input.GetKeyDown(KeyCode.A))
        {
            StartCoroutine(DamageTaken_Smooth(39));
        }*/
    }

    //根据角色属性决定
    public void Initialize_Healthbar(int currentHealth, int maxHealth)
    {
        healthSlider = new List<GameObject>();
        this.currentHealth = currentHealth;
        this.maxHealth = maxHealth;
        //创建slider组
        //1.决定需要创建多少个Slider  向上取整。 Max:50, 一行血量是20， 应该有3行。  Max:41,也应该是3行
        int lineCount = Mathf.CeilToInt((float)maxHealth / MaxHealthPerLine); //需要创建的slider数量
        //2. 假如说是MaxHealth为41，第三行的血量应该是多少： 取余操作
        int lastline_health = maxHealth % MaxHealthPerLine; //如果只有一行的情况，可以直接等于maxHealth
        if(lastline_health == 0) //说明被整除了， 20 或者 40 又或者是60
        {
            lastline_health = MaxHealthPerLine;
        }
        float yOffset = 32f;
        for (int i = 0; i < lineCount; i++)
        {
            var temp_slider = objectPool.GetGameObject(GameObjectType.HEALTHBAR_SLIDER);
            //var temp_slider = (GameObject)Instantiate(Resources.Load("Prefabs/UI/HealthBarSlider"));
            healthSlider.Add(temp_slider);
            temp_slider.transform.SetParent(this.transform, true);
            temp_slider.transform.localPosition = new Vector3(0, i * yOffset, 0);
            temp_slider.GetComponent<Slider>().value = MaxHealthPerLine;
            temp_slider.GetComponent<MaskImage>().maskImage.fillAmount = 1; //默认全都设置为1
        }
        //需要对最后一行的Slider做操作：

        healthSlider[lineCount - 1].GetComponent<MaskImage>().maskImage.fillAmount = (float)lastline_health / (float)MaxHealthPerLine;

        //TODO:要针对当前的血量，来设置value值: 假如说最大值为50，但是当前血量是25， 那么第一行的slider的value应该为MaxHealthPerLine，第二行slider的value为5，第三行的value应该为0
        // 设置血条的 value
        int remainingHealth = currentHealth; // 剩余的血量

        for (int i = 0; i < lineCount; i++)
        {
            // 当前行的最大血量
            int lineMaxHealth = MaxHealthPerLine;

            // 如果这是最后一行（最上面的一行），剩余血量会小于 MaxHealthPerLine
            if (i == lineCount - 1)
            {
                lineMaxHealth = lastline_health; //最上面一行的最大值
            }

            // 获取当前行的血条Slider组件
            var slider = healthSlider[i].GetComponent<Slider>();

            // 如果当前剩余血量大于当前行的最大血量，设为该行最大血量
            // 从最下面的一行开始设置血量
            if (remainingHealth >= lineMaxHealth)
            {
                slider.value = lineMaxHealth;
                remainingHealth -= lineMaxHealth; // 扣除已经用完的血量
            }
            else
            {
                // 如果剩余血量小于当前行的最大血量，则设置当前血量
                slider.value = remainingHealth;
                remainingHealth = 0; // 剩余血量已用完
            }
        }

        txt_currenthealth.text = currentHealth.ToString();
        txt_maxhealth.text = maxHealth.ToString(); //F8 F是转换成float类型，并且保留8位小数

        //ToDo:
        //1. 根据血量最大值，调整行数

    }

    public IEnumerator DamageTaken(int damage)
    {
        int original_health = currentHealth;
        int health_after = currentHealth - damage;

        // 确保不会小于零
        health_after = Mathf.Max(health_after, 0);

        // 计算动画的持续时间
        float time_consuming = 1f; // 1秒内完成动画
        float time_Elapsed = 0f; // 时间经过

        // 计算总的血量变化
        int health_delta = original_health - health_after;

        // 逐行更新血条的动画
        while (time_Elapsed < time_consuming)
        {
            // 根据当前经过的时间计算平滑的血量
            int currentDamage = (int)Mathf.Lerp(0, health_delta, time_Elapsed / time_consuming);

            // 当前剩余的血量
            int remainingHealth = original_health - currentDamage;

            // 更新每一行血条的value
            int tempRemainingHealth = remainingHealth;
            for (int i = 0; i < healthSlider.Count; i++)
            {
                // 获取当前行的Slider
                var slider = healthSlider[i].GetComponent<Slider>();
                // 当前行的最大血量
                int lineMaxHealth = MaxHealthPerLine;

                // 如果是最后一行
                if (i == healthSlider.Count - 1)
                {
                    lineMaxHealth = maxHealth % MaxHealthPerLine;
                }

                // 设置当前行的Slider的value
                if (tempRemainingHealth >= lineMaxHealth)
                {
                    slider.value = lineMaxHealth;
                    tempRemainingHealth -= lineMaxHealth;
                }
                else
                {
                    slider.value = tempRemainingHealth;
                    tempRemainingHealth = 0;
                }
            }

            // 更新血量文本
            txt_currenthealth.text = remainingHealth.ToString();

            // 如果血量已扣完，退出循环
            if (remainingHealth <= 0)
            {
                txt_currenthealth.text = "0";
                yield break;
            }

            // 增加时间
            time_Elapsed += Time.deltaTime;

            // 等待下一帧
            yield return null;
        }

        // 扣血动画结束后，确保血量正确，确保动画结束之后的血条正确
        currentHealth = health_after;

        // 更新血条
        int finalHealth = currentHealth;
        int finalRemainingHealth = finalHealth;
        for (int i = 0; i < healthSlider.Count; i++)
        {
            var slider = healthSlider[i].GetComponent<Slider>();
            int lineMaxHealth = MaxHealthPerLine;

            if (i == healthSlider.Count - 1)
            {
                lineMaxHealth = maxHealth % MaxHealthPerLine;
            }

            if (finalRemainingHealth >= lineMaxHealth)
            {
                slider.value = lineMaxHealth;
                finalRemainingHealth -= lineMaxHealth;
            }
            else
            {
                slider.value = finalRemainingHealth;
                finalRemainingHealth = 0;
            }
        }

        // 最终更新文本
        txt_currenthealth.text = currentHealth.ToString();
    }

    public IEnumerator DamageTaken_Smooth(int damage)
    {
        int original_health = currentHealth;
        int health_after = currentHealth - damage;

        // 确保不会小于零
        health_after = Mathf.Max(health_after, 0);

        // 计算动画的持续时间
        float time_consuming = 1f; // 1秒内完成动画
        float time_Elapsed = 0f; // 时间经过

        // 计算总的血量变化
        int health_delta = original_health - health_after;

        // 逐行更新血条的动画
        while (time_Elapsed < time_consuming)
        {
            // 通过SmoothStep来优化血量的减少速度
            float t = time_Elapsed / time_consuming;  // 归一化的时间
            float easedT = Mathf.SmoothStep(0f, 1f, t);  // 经过SmoothStep缓动函数的时间

            // 根据当前经过的时间计算平滑的血量
            int currentDamage = (int)Mathf.Lerp(0, health_delta, easedT);

            // 当前剩余的血量
            int remainingHealth = original_health - currentDamage;

            // 更新每一行血条的value
            int tempRemainingHealth = remainingHealth;
            for (int i = 0; i < healthSlider.Count; i++)
            {
                // 获取当前行的Slider
                var slider = healthSlider[i].GetComponent<Slider>();
                // 当前行的最大血量
                int lineMaxHealth = MaxHealthPerLine;

                // 如果是最后一行
                if (i == healthSlider.Count - 1)
                {
                    //lineMaxHealth = maxHealth % MaxHealthPerLine; // 如果maxHealth 正好是20， 那就会导致lineMaxHealth为0
                    lineMaxHealth = maxHealth % MaxHealthPerLine == 0 ? MaxHealthPerLine : maxHealth % MaxHealthPerLine;
                }

                // 设置当前行的Slider的value
                if (tempRemainingHealth >= lineMaxHealth)
                {
                    slider.value = lineMaxHealth;
                    tempRemainingHealth -= lineMaxHealth;
                }
                else
                {
                    slider.value = tempRemainingHealth;
                    tempRemainingHealth = 0;
                }
            }

            // 更新血量文本
            txt_currenthealth.text = remainingHealth.ToString();

            // 如果血量已扣完，退出循环
            if (remainingHealth <= 0)
            {
                txt_currenthealth.text = "0";
                yield break;
            }

            // 增加时间
            time_Elapsed += Time.deltaTime;

            // 等待下一帧
            yield return null;
        }

        // 扣血动画结束后，确保血量正确
        currentHealth = health_after;

        // 更新血条
        int finalHealth = currentHealth;
        int finalRemainingHealth = finalHealth;
        for (int i = 0; i < healthSlider.Count; i++)
        {
            var slider = healthSlider[i].GetComponent<Slider>();
            int lineMaxHealth = MaxHealthPerLine;

            if (i == healthSlider.Count - 1)
            {
                lineMaxHealth = maxHealth % MaxHealthPerLine == 0 ? MaxHealthPerLine : maxHealth % MaxHealthPerLine;
            }

            if (finalRemainingHealth >= lineMaxHealth)
            {
                slider.value = lineMaxHealth;
                finalRemainingHealth -= lineMaxHealth;
            }
            else
            {
                slider.value = finalRemainingHealth;
                finalRemainingHealth = 0;
            }
        }

        // 最终更新文本
        txt_currenthealth.text = currentHealth.ToString();
    }

    public void ReturnSlider()
    {
        foreach (var item in healthSlider)
        {
            objectPool.ReturnGameObject(GameObjectType.HEALTHBAR_SLIDER, item);
        }
    }
}


[Serializable]
public class MySlider
{   

    public Slider healthSlider;
    public Image backGround;
}
