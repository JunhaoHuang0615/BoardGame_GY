using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    Dictionary<string, Weapon> weaponCache; //武器Data的缓存
    void Start()
    {
        weaponCache = new Dictionary<string, Weapon>();
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
