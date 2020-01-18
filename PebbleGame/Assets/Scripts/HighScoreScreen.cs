using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighScoreScreen : MonoBehaviour
{

    List<HighScoreitem> items = new List<HighScoreitem>();
    public GameObject itemPrefab;
    public Transform container;
    public Text waitText;

    public void Open()
    {
        gameObject.SetActive(true);
        RefreshScores();
    }

    public void Close()
    {
        GameManager.Instance.ShowMenu();
    }

    public void RefreshScores()
    {
        waitText.enabled = true;
        ServerManager.Instance.MakeServerRequest(OnGetHighscores, OnError, new { action = "GetScores" });
    }


    public void OnGetHighscores(SimpleJSON.JSONNode data)
    {
        var childs = container.GetComponentsInChildren<HighScoreitem>();
        for (int i = 0; i < childs.Length; i++)
        {
            Destroy(childs[i].gameObject);
        }

        items.Clear();
        Debug.LogFormat("Data Loaded {0},{1},{2}",data,data[0]["nickname"],0);

        for (int i = 0; i < 10; i++)
        {
            var item = Instantiate(itemPrefab, container).GetComponent<HighScoreitem>();
            item.SetUp(i + 1, data[i]["nickname"], data[i]["highscore"]);
        }

        waitText.enabled = false;
    }

    public void OnError(string data)
    {
        Debug.Log(data);
    }

}
