using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public Slider loadingSlider;
    public Text text;

    public void SetProgress(float progress)
    {
        loadingSlider.value = progress;
        text.text = Mathf.Round(progress*100) + "%";
    }
}
