﻿using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DataManager : MonoBehaviour
{   
    public static DataManager instance{ get; private set; }
    private ResourcesMananger resourceManager;

    // 缓存了武器信息
    private Dictionary<string, Weapon> weaponDataCache = new Dictionary<string, Weapon>();
    //TODO:图片的缓存
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        resourceManager = FindObjectOfType<ResourcesMananger>();

        // 最初に全データを解析するか、後で必要に応じて解析する（怠惰なロード）
        LoadAndParseWeaponData("TableBytes/WeaponsData");

        TestWeaponData("Sword");
    }

    // データのロードと解析
    public void LoadAndParseWeaponData(string filePath)
    {
        byte[] csvBytes = resourceManager.GetCSVBytes(CSVResource.Weapon, filePath);

        if (csvBytes != null)
        {
            // bytes データを解凍・復号化し、パースする
            byte[] decryptedData = DecryptData(csvBytes);
            byte[] decompressedData = DecompressData(decryptedData);

            string csvText = Encoding.UTF8.GetString(decompressedData);
            ParseWeaponCSV(csvText);
        }
        else
        {
            Debug.LogError($"Failed to load CSV bytes from {filePath}");
        }
    }

    // CSV 文字列をパースして武器データをキャッシュ
    private void ParseWeaponCSV(string csvText)
    {
        using (StringReader reader = new StringReader(csvText))
        {
            string line;
            bool isFirstLine = true; // 最初の行（ヘッダー）をスキップするためのフラグ

            while ((line = reader.ReadLine()) != null)
            {
                if (isFirstLine)
                {
                    // 最初の行はヘッダーなのでスキップ
                    isFirstLine = false;
                    continue;
                }

                string[] fields = line.Split(',');

                // データの数が足りているか確認
                if (fields.Length >= 7)
                {
                    try
                    {
                        // 数値フィールドのパースと例外処理
                        int attack = int.Parse(fields[3]);
                        float speed = float.Parse(fields[4]);
                        int defense = int.Parse(fields[5]);
                        float accuracy = float.Parse(fields[6]);
                        int range = int.Parse(fields[7]);

                        Weapon weapon = new Weapon()
                        {
                            WeaponName = fields[0],
                            AttackType = fields[1],
                            Attack = attack,
                            Speed = speed,
                            Defense = defense,
                            Accuracy = accuracy,
                            ImagePath = fields[2],  // 画像パス
                            Range = range,
                            Range_Pattern = fields[8],
                            WeaponImage = resourceManager.LoadWeaponImage(fields[2]) // 画像をロード
                        };

                        // 武器データをキャッシュ
                        weaponDataCache[weapon.WeaponName] = weapon;
                    }
                    catch (FormatException ex)
                    {
                        // パースエラーの処理、ログに出力
                        Debug.LogError($"Error parsing line: {line}. Exception: {ex.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Invalid CSV line: {line}. Expected at least 7 fields.");
                }
            }

            Debug.Log("CSV data parsed and weapon data cached.");
        }
    }

    // 武器データを取得
    public Weapon GetWeapon(string weaponName)
    {
        if (weaponDataCache.ContainsKey(weaponName))
        {
            // キャッシュからクローンを返す
            return weaponDataCache[weaponName].Clone();
        }

        Debug.LogWarning($"Weapon not found: {weaponName}");
        return null;
    }


    // 解密データ
    private byte[] DecryptData(byte[] data)
    {
        byte key = 0xAA;
        for (int i = 0; i < data.Length; i++)
        {
            data[i] ^= key;
        }
        return data;
    }

    // 解压データ
    private byte[] DecompressData(byte[] data)
    {
        using (MemoryStream inputStream = new MemoryStream(data))
        using (MemoryStream outputStream = new MemoryStream())
        {
            using (GZipStream gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            {
                gzipStream.CopyTo(outputStream);
            }
            return outputStream.ToArray();
        }
    }


    //测试获取的信息
    // 武器データをテストするメソッド
    private void TestWeaponData(string weaponName)
    {
        // DataManagerから武器データを取得
        Weapon weapon = this.GetWeapon(weaponName);

        if (weapon != null)
        {
            // データが正しくロードされたか出力して確認
            Debug.Log($"Weapon Name: {weapon.WeaponName}");
            Debug.Log($"Weapon Type: {weapon.AttackType}");
            Debug.Log($"Weapon Attack: {weapon.Attack}");
            Debug.Log($"Weapon Speed: {weapon.Speed}");
            Debug.Log($"Weapon Defense: {weapon.Defense}");
            Debug.Log($"Weapon Accuracy: {weapon.Accuracy}");
            Debug.Log($"Weapon Image Path: {weapon.ImagePath}");

        }
        else
        {
            Debug.LogError($"Weapon {weaponName} not found in cache.");
        }
    }
}
