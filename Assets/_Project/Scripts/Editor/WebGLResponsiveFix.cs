#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using UnityEngine;

public class WebGLResponsiveFix
{
    // Este método se ejecuta automáticamente justo después de que Unity termina de hacer el Build
    [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        // Solo nos interesa modificar si es una build de WebGL
        if (target != BuildTarget.WebGL) return;

        string htmlPath = Path.Combine(pathToBuiltProject, "index.html");
        
        if (File.Exists(htmlPath))
        {
            string html = File.ReadAllText(htmlPath);
            
            // Buscamos el canvas y le inyectamos CSS para que ocupe el 100% de la pantalla,
            // anulando la resolución rígida que Unity le pone por defecto.
            if (!html.Contains("style=\"width: 100%; height: 100%;\""))
            {
                html = html.Replace("id=\"unity-canvas\"", "id=\"unity-canvas\" style=\"width: 100%; height: 100%;\"");
                File.WriteAllText(htmlPath, html);
                Debug.Log("✅ [WebGLResponsiveFix] Se inyectó código elástico/responsive en el index.html automáticamente.");
            }
        }
    }
}
#endif
