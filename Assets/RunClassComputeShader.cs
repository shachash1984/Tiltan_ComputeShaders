using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunClassComputeShader : MonoBehaviour {

    public ComputeShader shader;

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space))
    //        RunShader();
    //}

    void RunShader()
    {
        int kernelHandle = shader.FindKernel("CSMain");

        RenderTexture tex = new RenderTexture(256, 256, 24);
        tex.enableRandomWrite = true;
        tex.Create();

        shader.SetTexture(kernelHandle, "Result", tex);
        shader.Dispatch(kernelHandle, 256 / 8, 256 / 8, 1);

        Renderer rend = GetComponent<Renderer>();
        rend.material.SetTexture("_MainTex", tex);
    }
}
