using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.IO.Compression;

public class CSVToBytesConvertor
{
    [MenuItem("Tools/CSVTool/ConvertCSVToBytes")]
    static void ConvertCSVToBytes()
    {
        string csvPath = "Assets/CSVTable/WeaponData.csv"; //输入路径，表格路径
        string bytesPath = "Assets/Resources/Table/WeaponData.bytes"; //输出路径

        //读取CSV
        byte[] csvData =  File.ReadAllBytes(csvPath);

        byte[] compressedData = CompressData(csvData);

        byte[] encryptData = EncryptData(compressedData);

        //输出Bytes
        File.WriteAllBytes(bytesPath, encryptData);

        //刷新
        AssetDatabase.Refresh();

        Debug.Log("CSV工具转换完成");
    }

    //GZIP压缩方法
    static byte[] CompressData(byte[] data )
    {
        using (MemoryStream outputstream = new MemoryStream()) {
            using (GZipStream gzipstream = new GZipStream(outputstream, CompressionMode.Compress)) {
                gzipstream.Write(data,0,data.Length);
                
            }
            return outputstream.ToArray();
        }
    }

    //加密的方法 XOR
    static byte[] EncryptData(byte[] data) {
        byte key = 0xAA; //钥匙，可以自定义
        for (int i = 0; i < data.Length; i++) { 
            data[i] ^= key;  // ^ : 异或运算 数字键6上面的符号 
        }
        return data;
    }
}
