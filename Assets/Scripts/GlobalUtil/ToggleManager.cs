using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Image

/// <summary>
/// Unity 標準の Toggle はトグル時に元の画像を消してくれないため、スクリプトで対応
/// </summary>
public class ToggleManager : MonoBehaviour
{
    Image backgroundImage;

    // Start is called before the first frame update
    private void Awake()
    {
        backgroundImage = transform.Find("Background").GetComponent<Image>();
    }
    void Start()
    {
        // toggled なら元画像を消し、!toggled なら元画像を表示
        backgroundImage.enabled = !GetComponent<Toggle>().isOn;
    }

    // Update is called once per frame
    void Update()
    {
    }
    
    public void ValueChangedHandler(bool isOn)
    {
        if (isOn) backgroundImage.enabled = false;
        else backgroundImage.enabled = true;
    }
}
