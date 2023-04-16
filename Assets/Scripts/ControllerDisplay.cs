using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControllerDisplay : MonoBehaviour
{
    public Image leftImage;
    public Image rightImage;
    public Image upImage;
    public Image downImage;

    // Start is called before the first frame update
    void Start()
    {
        m_gm = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        Color idleColor = Color.white;
        Color highlightColor = Color.red;

        leftImage.color = idleColor;
        rightImage.color = idleColor;
        upImage.color = idleColor;
        downImage.color = idleColor;


        if (m_gm.networkInput.xAxis.Value < 0.0f)
        {
            leftImage.color = highlightColor;
        }
        else if (m_gm.networkInput.xAxis.Value > 0.0f)
        {
            rightImage.color = highlightColor;
        }
        if (m_gm.networkInput.yAxis.Value < 0.0f)
        {
            downImage.color = highlightColor;
        }
        else if (m_gm.networkInput.yAxis.Value > 0.0f)
        {
            upImage.color = highlightColor;
        }
    }

    GameManager m_gm;
}
