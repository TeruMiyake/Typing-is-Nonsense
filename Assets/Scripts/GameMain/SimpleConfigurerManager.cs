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
        if (missLimitInput == null || int.Parse(missLimitInput.GetComponent<TMP_InputField>().text) > 999 || int.Parse(missLimitInput.GetComponent<TMP_InputField>().text) < 0)
            missLimitInput.GetComponent<TMP_InputField>().text = "360";
    }
}
