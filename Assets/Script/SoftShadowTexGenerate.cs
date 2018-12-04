using UnityEditor;
using UnityEngine;
using System.IO;
using Sirenix.OdinInspector;
[CreateAssetMenu]
public class SoftShadowTexGenerate : ScriptableObject {
    public int width;
    public Texture2D tex;

    [Button]
    public void Generate() {
        if(tex == null) {
            tex = new Texture2D(width, width, TextureFormat.ARGB32, false, true);
        }
        Debug.Log("Start writting png");
        for(int i = 0; i < width; i++) {
            for(int j = 0; j < width; j++) {
                Color c = new Color(1,1,1,1);
                float value = 1;
                if(i <= j) {
                    if(i == 0) {
                        value = 1;
                    }
                    else {
                        value = (float)i / j;
                    }
                }
                c = Color.Lerp(Color.white, Color.black, value);
                tex.SetPixel(i, j, c);
            }
        }
        
        Debug.Log("Finished writting png");
    }
}