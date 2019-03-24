using UnityEngine;

public class ScreenshotTaker : MonoBehaviour
{
    public Vector2[] resolutions = new Vector2[]
    {
        new Vector2( 640, 960 ),
        new Vector2( 640, 1136 ),
        new Vector2( 1080, 1920 ),
        new Vector2( 768, 1024 ),
        new Vector2( 1536, 2048 )
    };
    
#if UNITY_EDITOR
    
    private bool takeHiResShot = false;
 
     public static string ScreenShotName(int width, int height) {
         return string.Format("{0}/../screenshots/screen_{1}x{2}_{3}.png", 
                              Application.dataPath, 
                              width, height, 
                              System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
     }
 
     public void TakeHiResShot() {
         takeHiResShot = true;
     }
 
     void LateUpdate()
     {
         takeHiResShot |= Input.GetKeyDown("k");
         if (takeHiResShot)
         {
             for ( int resIndex = 0; resIndex < resolutions.Length; resIndex++ )
             {
                 DoTakeShot( (int) resolutions[ resIndex ].x, (int) resolutions[ resIndex ].y );
             }
         }

         if ( Input.GetKeyDown("s"))
         {
             for ( int resIndex = 0; resIndex < resolutions.Length; resIndex++ )
             {
                 DoTakeShot( (int) resolutions[ resIndex ].x, (int) resolutions[ resIndex ].y );
             }
         }
     }
     
     void DoTakeShot( int resWidth, int resHeight )
     {
         RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
         GetComponent<Camera>().targetTexture = rt;
         Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
         GetComponent<Camera>().Render();
         RenderTexture.active = rt;
         screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
         GetComponent<Camera>().targetTexture = null;
         RenderTexture.active = null; // JC: added to avoid errors
         Destroy(rt);
         byte[] bytes = screenShot.EncodeToPNG();
         string filename = ScreenShotName(resWidth, resHeight);
         System.IO.File.WriteAllBytes(filename, bytes);
         Debug.Log(string.Format("Took screenshot to: {0}", filename));
         takeHiResShot = false;
     }
#endif
     
 }