using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] Text timerText;

    // Start is called before the first frame update
    void Start()
    {
        timerText = GetComponentInChildren<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        float timeRemaining = GameManager.time.Value;

        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
        }

        timerText.text = timeRemaining.ToString("0.0");
    }
}
