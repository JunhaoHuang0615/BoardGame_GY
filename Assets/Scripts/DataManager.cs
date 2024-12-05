using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public class DataManager : MonoBehaviour
{   
    public static DataManager instance{ get; private set; }
    private ResourcesMananger resourceManager;

    // 缓存了武器信息
    private Dictionary<string, Weapon> weaponDataCache = new Dictionary<string, Weapon>();
    private Dictionary<string, UnitData> playerDataCache = new Dictionary<string, UnitData>();
    private Dictionary<string, UnitData> enemyDataCache = new Dictionary<string, UnitData>();
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
        LoadAndParseChracterData("TableBytes/PlayerChracter",CSVResource.PlayerChracter);
        LoadAndParseChracterData("TableBytes/EnemyChracter",CSVResource.EnemyChracter);

        //TestWeaponData("Sword");
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

    public void LoadAndParseChracterData(string filePath,CSVResource playerOrEnenmy)
    {
        byte[] csvBytes = resourceManager.GetCSVBytes(playerOrEnenmy, filePath);

        if (csvBytes != null)
        {
            // bytes データを解凍・復号化し、パースする
            byte[] decryptedData = DecryptData(csvBytes);
            byte[] decompressedData = DecompressData(decryptedData);

            string csvText = Encoding.UTF8.GetString(decompressedData);
            ParseChracterCSV(csvText, playerOrEnenmy);
        }
        else
        {
            Debug.LogError($"Failed to load CSV bytes from {filePath}");
        }
    }
    private void ParseChracterCSV(string csvText, CSVResource playerOrEnenmy)
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
                        int id = int.Parse(fields[0]);
                        string name = fields[1];
                        CharacterType type = (CharacterType)Enum.Parse(typeof(CharacterType), fields[2], true);
                        int flyHeight = int.Parse(fields[3]);
                        int maxHealth = int.Parse(fields[4]);
                        int defense = int.Parse(fields[5]);
                        int moveRange = int.Parse(fields[6]);
                        int moveSpeed = int.Parse(fields[7]);
                        PawnType pawntype = (PawnType)Enum.Parse(typeof(PawnType), fields[8], true);

                        string weaponField = fields[9];
                        List<string> weapons = weaponField.Split('|').ToList();

                        UnitData unit = new UnitData()
                        {
                            ID = id,
                            Name = name,
                            Type = type,
                            FlyHeight = flyHeight,
                            MaxHealth = maxHealth,
                            Defense = defense,
                            MoveRange = moveRange,
                            MoveSpeed = moveSpeed,
                            PawnType = pawntype,
                            Weapons = weapons
                        };
                        if(playerOrEnenmy == CSVResource.PlayerChracter)
                        {
                            playerDataCache[unit.Name] = unit;
                        }
                        else
                        {
                            enemyDataCache[unit.Name] = unit;
                        }
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

    public UnitData GetChracterData(string unitName,CSVResource playerOrEnmy)
    {   
        if(playerOrEnmy == CSVResource.PlayerChracter)
        {
            if (playerDataCache.ContainsKey(unitName))
            {
                // キャッシュからクローンを返す
                return playerDataCache[unitName].Clone();
            }
        }
        else
        {
            if (enemyDataCache.ContainsKey(unitName))
            {
                // キャッシュからクローンを返す
                return enemyDataCache[unitName].Clone();
            }
        }


        Debug.LogWarning($"Weapon not found: {unitName}");
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
