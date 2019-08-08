using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]

public class heartrate_resize: MonoBehaviour
{
    public Material heartrate;
    public float blurSize = 0.08f;
    public Vector2 blurCenterPos = new Vector2(0.5f, 0.5f);
    [Range(1, 48)]
    public int samples = 0;
    public Material radialBlurMaterial = null;
    public int time_measure;
    public static float time_level = 0.08f;
    public int middle = 21;
    public int max = 48;


    // Start is called before the first frame update
    void Start()
    {

        StartCoroutine("blursizecontrol");
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (blurSize > 0.0f)
        {
            radialBlurMaterial.SetInt("_Samples", samples);
            radialBlurMaterial.SetFloat("_BlurSize", blurSize);
            radialBlurMaterial.SetVector("_BlurCenterPos", blurCenterPos);
            Graphics.Blit(source, destination, radialBlurMaterial);

        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
    IEnumerator blursizecontrol()
    {
        bool nowblur = false;
        samples = 21;

        time_measure = 0;
        middle = 21;
        max = 48;
        StartCoroutine("timecontrol");

        while (true)
        {
            if (nowblur == false && samples >= middle)
            {
                samples += 1;
                blurSize += 0.1f;
                if (samples >= max)
                {
                    nowblur = true;
                }
            }
            else if (nowblur == true && samples <= max)
            {
                samples -= 1;
                blurSize -= 0.1f;
                if (samples <= middle)
                {
                    nowblur = false;
                }
            }



            yield return new WaitForSeconds(time_level);
        }

    }


    IEnumerator timecontrol()
    {

        while (true)
        {

            time_measure++;
            if (time_measure == 30)
            {

                middle = middle - 7;
                max = max - 16;
                samples = (middle + max) / 2;
                time_measure = 0;
                time_level = time_level + 0.04f;
                if (middle <= 0 && max <= 0)
                {
                    samples = 0;
                    time_level = 0.08f;
                    StopCoroutine("blursizecontrol");
                    middle = 21;
                    max = 48;
                    StopCoroutine("timecontrol");
                }
            }

            yield return new WaitForSeconds(1.0f);
        }
    }

}
