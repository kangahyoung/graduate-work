using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class StartEXEFile : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        //string filePath = Application.StartupPath + @"\\WindowsFormsApp1.exe";
        Debug.Log(Path.GetFullPath(".") + "\\WindowsFormsApp1.exe");
        //Debug.Log(Application.StartupPath + "");
        System.Diagnostics.Process.Start(Path.GetFullPath(".") + "\\WindowsFormsApp1.exe");

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
