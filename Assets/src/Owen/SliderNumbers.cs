/* COPYWRITE INFRINGMENT 
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ShowSliderValueTMP : MonoBehaviour
{
    public void UpdateLabel(float value)
    {
        TextMeshProUGUI lbl = GetComponent<TextMeshProUGUI>();
        if (lbl != null)
            lbl.text = Mathf.RoundToInt(value * 100) + "%";
    }

}
https://github.com/almartson/UnityMainMenuTemplate/blob/master/Assets/Scripts/ShowSliderValueTMP.cs
*/


using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ShowSliderMappedValue : MonoBehaviour
{
    public Slider slider;
    private Text lbl;

    [Header("Display range")]
    public int displayMin = 1;
    public int displayMax = 10;

    private void Awake()
    {
        lbl = GetComponent<Text>();
        if (slider != null)
        {
            slider.wholeNumbers = true; // whole numbers
            slider.onValueChanged.AddListener(UpdateLabel);
            UpdateLabel(slider.value); // initialize
        }
    }

    public void UpdateLabel(float value)
    {
        if (lbl != null && slider != null)
        {
            //cleanup slider numbers more
            float mappedValue = Mathf.Lerp(displayMin, displayMax, 
                (value - slider.minValue) / (slider.maxValue - slider.minValue));
            
            lbl.text = Mathf.RoundToInt(mappedValue).ToString(); //lbl.text = Mathf.RoundToInt(value * 100) + "%";
        }
    }

    private void OnDestroy()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(UpdateLabel);
    }
}
