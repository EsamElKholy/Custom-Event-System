using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum SkinColor
{
    RED,
    GREEN,
    BLUE,
    CYAN,
    DEFAULT
}

public class ColorChanger : MonoBehaviour
{
    private new Renderer renderer;
    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<Renderer>();
    }

    public void ChangeColorToRed()
    {
        ChangeColor(SkinColor.RED);
    }

    public void ChangeColorToGreen()
    {
        ChangeColor(SkinColor.GREEN);
    }

    public void ChangeColorToBlue()
    {
        ChangeColor(SkinColor.BLUE);
    }

    public void ChangeColorToCyan()
    {
        ChangeColor(SkinColor.CYAN);
    }

    public void ResetColor()
    {
        ChangeColor(SkinColor.DEFAULT);
    }

    private void ChangeColor(SkinColor skinColor)
    {
        if (!renderer)
        {
            renderer = GetComponent<Renderer>();
        }

        if (renderer)
        {
            Material material = null;

            if (!Application.isPlaying)
            {
                material = renderer.material;
            }
            else
            {
                material = renderer.sharedMaterial;
            }

            switch (skinColor)
            {
                case SkinColor.RED:
                    {
                        material.color = Color.red;
                    }
                    break;
                case SkinColor.GREEN:
                    {
                        material.color = Color.green;
                    }
                    break;
                case SkinColor.BLUE:
                    {
                        material.color = Color.blue;
                    }
                    break;
                case SkinColor.CYAN:
                    {
                        material.color = Color.cyan;
                    }
                    break;
                case SkinColor.DEFAULT:
                    {
                        material.color = Color.white;
                    }
                    break;
                default:
                    break;
            }
        }
       
    }
}
