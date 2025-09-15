using UnityEngine;
using System.IO;
using System.IO.Compression;
using System;

public class Export : MonoBehaviour
{
    //Generates the JSON or LUA files
    public void ExportFile(string path, string content)
    {
        if (SystemInfo.operatingSystem.Contains("Windows"))
        {
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine(content);
            }
        }
        else
        {
            System.IO.File.WriteAllText(path, content);
        }
    }

    //Compresses the several files into a ZIP file (ready to load on the game)
    public void ExportCompressedMod(string[] filePath, string[] relativePaths, string zipPath)
    {
        using (FileStream zipToOpen = new FileStream(zipPath, FileMode.Create))
        {
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
            {
                for (int i=0; i<filePath.Length; i++)
                {
                    string entryName = Path.GetFileName(filePath[i]);
                    archive.CreateEntryFromFile(filePath[i], relativePaths[i] + entryName);
                }
            }
        }
    }
}
