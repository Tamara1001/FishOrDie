using UnityEngine;
using UnityEditor;
using System.IO;

public class DumpHierarchy
{
    public static void Dump()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Player.prefab");
        using (StreamWriter writer = new StreamWriter("dump.txt"))
        {
            DumpRecursive(prefab.transform, "", writer);
        }
    }

    private static void DumpRecursive(Transform t, string indent, StreamWriter writer)
    {
        writer.WriteLine(indent + t.name + " (pos: " + t.localPosition + ", scale: " + t.localScale + ")");
        foreach (Transform child in t)
        {
            DumpRecursive(child, indent + "  ", writer);
        }
    }
}
