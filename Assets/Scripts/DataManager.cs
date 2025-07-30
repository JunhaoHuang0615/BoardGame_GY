using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    Dictionary<string, Weapon> weaponCache; //武器Data的缓存
    ResourcesMananger resourcesMananger;
    public static DataManager Instance { get; private set; }
    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        weaponCache = new Dictionary<string, Weapon>();
        resourcesMananger = FindObjectOfType<ResourcesMananger>();
        LoadWeaponData();
    }

    private void LoadWeaponData()
    {
        byte[] loadedData = resourcesMananger.LoadCSVBytes(CSVResource.WEAPON, "Table/WeaponData");
        if (loadedData != null) {
            //先解密再压缩 原因： 我们是先压缩再加密
            byte[] decryptData = DecryptData(loadedData);
            byte[] decompressData = DecompressData(decryptData);

            ParseWeaponCSVData(decompressData);
        }

    }
    private void ParseWeaponCSVData(byte[] data)
    {
        string csvText = Encoding.UTF8.GetString(data);

        using (StringReader reader = new StringReader(csvText))
        {
            string line;
            bool firstLine = true;

            while ((line = reader.ReadLine()) != null)
            {
                if (firstLine) {
                    //第一行是标题，跳过
                    firstLine = false;
                    continue;
                }
                string[] fields = line.Split(','); //把每一行的数据根据逗号来拆分
                if (fields.Length >= 7) {
                    try
                    {
                        string name = fields[0];
                        string weaponType = fields[1];
                        string Imagepath = fields[2];
                        float attackability = float.Parse(fields[3]);
                        float defense = float.Parse(fields[4]);
                        float speed = float.Parse(fields[5]);
                        float accuracy = float.Parse(fields[6]);
                        float range = float.Parse(fields[7]);
                        string range_pattern = fields[8];

                        Weapon weapon = new Weapon()
                        {
                            name = name,
                            attackAbility = attackability,
                            denfense = defense,
                            speed = speed,
                            accuracy = accuracy,
                            weaponImagePath = Imagepath,
                            WeaponType = weaponType,
                            weaponImage = resourcesMananger.LoadWeaponImage(Imagepath),
                            range = range,
                            range_pattern = range_pattern,
                        };
                        weaponCache[name] = weapon;

                    }
                    catch (FormatException ex) {
                        Debug.LogError($"Error parsing line : {line}, Excepttion : {ex.Message}");
                    }
                }
                else
                {
                    Debug.LogError($"Invalid CSV line : {line} Excepted at least 7");
                }
            }
            Debug.Log("Weapon cached");
        }
    }

    public Weapon GetWeapon(string weaponName)
    {
        if (weaponCache.ContainsKey(weaponName))
        {
            return weaponCache[weaponName].Clone();
        }
        return null;
    }

    //解密方法
    public byte[] DecryptData(byte[] data)
    {
        byte key = 0xAA; //钥匙，可以自定义
        for (int i = 0; i < data.Length; i++)
        {
            data[i] ^= key;  // ^ : 异或运算 数字键6上面的符号 
        }
        // a ^ k = b 加密过程
        // b ^ k = a 解密过程
        return data;
    }

    //解压方法 GZIP
    public byte[] DecompressData(byte[] data) 
    {
        using (MemoryStream inputStream = new MemoryStream(data))
        using (MemoryStream outputStream = new MemoryStream()) 
        {
            using (GZipStream gZipStream = new GZipStream(inputStream, CompressionMode.Decompress)) { 
                gZipStream.CopyTo(outputStream);
            }

            return outputStream.ToArray();
        }
    
    }
}
