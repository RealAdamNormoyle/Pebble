using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighScoreitem : MonoBehaviour
{
    public Color evenColor;
    public Color oddColor;
    public Text id;
    public Text score;
    public Text nickname;
    public Image image;
    public void SetUp(int i,string n,string s)
    {
        id.text = i.ToString();
        score.text = s;
        nickname.text = n;

        float r = i % 2;
        image.color = (r == 0) ? evenColor : oddColor;
    }

}
