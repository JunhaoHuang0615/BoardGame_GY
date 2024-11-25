using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEngine;

public class CSVToBytesConverter
{
    // 菜单工具，支持处理多个 CSV 文件
    [MenuItem("Tools/Convert and Secure CSV Files to Bytes")]
    static void ConvertCSVFilesToCompressedEncryptedBytes()
    {
        // 定义所有 CSV 文件及其对应的 .bytes 文件路径
        string[] csvFiles = {
            "Assets/Resources/CSVTable/WeaponsData.csv",
            "Assets/Resources/CSVTable/EnemyChracter.csv",
            "Assets/Resources/CSVTable/PlayerChracter.csv"
        };

        string[] bytesFiles = {
            "Assets/Resources/TableBytes/WeaponsData.bytes",
            "Assets/Resources/TableBytes/EnemyChracter.bytes",
            "Assets/Resources/TableBytes/PlayerChracter.bytes"
        };

        // 遍历所有文件并处理
        for (int i = 0; i < csvFiles.Length; i++)
        {
            string csvPath = csvFiles[i];
            string bytesPath = bytesFiles[i];

            try
            {
                // 读取 CSV 数据
                byte[] csvData = File.ReadAllBytes(csvPath);

                // 压缩数据
                byte[] compressedData = CompressData(csvData);

                // 加密数据
                byte[] encryptedData = EncryptData(compressedData);

                // 输出为 .bytes 文件
                File.WriteAllBytes(bytesPath, encryptedData);

                Debug.Log($"成功处理: {csvPath} -> {bytesPath}");
            }
            catch (IOException ex)
            {
                Debug.LogError($"文件处理失败: {csvPath}. 错误: {ex.Message}");
            }
        }

        // 刷新 Unity 资源数据库
        AssetDatabase.Refresh();
        Debug.Log("所有 CSV 文件已成功压缩、加密并转换为 .bytes 文件。");
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

    // 使用 XOR 加密
    static byte[] EncryptData(byte[] data)
    {
        byte key = 0xAA; // 加密密钥，可以更改
        for (int i = 0; i < data.Length; i++)
        {
            data[i] ^= key; // 执行 XOR 操作
        }
        return data;
    }
}