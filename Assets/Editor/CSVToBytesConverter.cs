using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEngine;

public class CSVToBytesConverter
{
    // 菜单工具，用于在 Unity 编辑器中触发
    [MenuItem("Tools/Convert and Secure CSV to Bytes")]
    static void ConvertCSVToCompressedEncryptedBytes()
    {
        string csvPath = "Assets/Resources/CSVTable/WeaponsData.csv"; // CSV 文件路径
        string bytesPath = "Assets/Resources/TableBytes/WeaponsData.bytes";// 输出的 .bytes 文件路径

        // 读取 CSV 文件
        byte[] csvData = File.ReadAllBytes(csvPath);

        // 压缩 CSV 数据
        byte[] compressedData = CompressData(csvData);

        // 加密数据
        byte[] encryptedData = EncryptData(compressedData);

        // 写入加密压缩后的 Bytes 文件
        File.WriteAllBytes(bytesPath, encryptedData);

        // 刷新 Unity 资源数据库，确保资源被识别
        AssetDatabase.Refresh();

        Debug.Log("CSV 压缩、加密并转换为 .bytes 完成。");
    }

    // 使用 Gzip 进行压缩
    static byte[] CompressData(byte[] data)
    {
        using (MemoryStream outputStream = new MemoryStream())
        {
            using (GZipStream gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
            {
                gzipStream.Write(data, 0, data.Length);
            }
            return outputStream.ToArray();
        }
    }

    // 使用 XOR 加密（简单加密算法）
    static byte[] EncryptData(byte[] data)
    {
        byte key = 0xAA; // 加密密钥，可以自定义
        for (int i = 0; i < data.Length; i++)
        {
            data[i] ^= key; // XOR 操作
        }
        return data;
    }
}