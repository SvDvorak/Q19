using UnityEngine;

public class RenderDownSampling : MonoBehaviour
{
    private RenderTexture _texture;

    public void Start()
    {
        _texture = new RenderTexture(320, 240, 24);
        RenderTexture.active = _texture;
    }

    public void OnPreRender()
    {
        Camera.main.targetTexture = _texture;
    }

    public void OnPostRender()
    {
        Camera.main.targetTexture = null;
        Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _texture);
    }
}