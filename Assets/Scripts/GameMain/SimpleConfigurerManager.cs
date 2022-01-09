using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class SimpleConfigurerManager : MonoBehaviour
{
    public GameObject missLimitInput;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void onMissLimitEndEdit()
    {
        int misslim;
        if (missLimitInput.GetComponent<TMP_InputField>().text == null) misslim = 50;
        else
        {
            misslim = int.Parse(missLimitInput.GetComponent<TMP_InputField>().text);
        }
        if (misslim > 360) misslim = 360;
        else if (misslim < 0) misslim = 0;

        missLimitInput.GetComponent<TMP_InputField>().text = misslim.ToString();
        PlayerPrefs.SetInt("MissLimit", misslim);
    }
}
