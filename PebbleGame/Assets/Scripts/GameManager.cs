using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.UI;
using System.Linq;
using SimpleJSON;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] Transform playerObject;
    [SerializeField] Transform shadowObject;
    [SerializeField] Transform shoreline;

    [SerializeField] MeshRenderer waterMaterial;
    [SerializeField] GameObject[] ripples;
    [SerializeField] Text scoreText;
    [SerializeField] Text countDownText;
    [SerializeField] HighScoreScreen scoresScreen;
    [SerializeField] GameObject startScreen;
    [SerializeField] GameObject endGameScreen;
    [SerializeField] GameObject customizationScreen;
    [SerializeField] GameObject firstTimePopup;

    [SerializeField] Button nextPage;
    [SerializeField] Button prevPage;
    [SerializeField] ScrollRect scrollRect;

    [SerializeField] Text mainScreenHighScore;
    [SerializeField] Text mainScreenLastScore;

    [SerializeField] Text endGameScore;
    [SerializeField] Text endGameTitle;
    [SerializeField] Text endGameHighScore;
    [SerializeField] Text nicknameInput;

    [SerializeField] Image playButtonImage;

    [SerializeField] Image soundToggleIcon;
    [SerializeField] Image vibrationToggleIcon;

    [SerializeField] Sprite soundOnSprite;
    [SerializeField] Sprite soundOffSprite;
    [SerializeField] Sprite vibrationOnSprite;
    [SerializeField] Sprite vibrationOffSprite;


    [SerializeField] GameObject customizationNotificationIcon;
    [SerializeField] Text customizationNotificationtext;
    [SerializeField] GameObject endgameNotificationIcon;
    [SerializeField] Text endgameNotificationtext;

    [SerializeField] Animator shareWindow;
    Animator rockAnim;

    List<CustomizationItem> customizationItems = new List<CustomizationItem>();
    public List<GameObject> playerObjects;

    public int totalCustomizationPages;
    int currentPage;

    float speed = 1f;
    float maxHeight = 1.5f;
    float currentHeight;
    float fallSpeed = 0.5f;
    float jumpProgress = 0f;
    float randomModifier = 1f;
    int skips;
    bool isPlaying;
    bool isInitialized = false;
    bool isBusy = false;

    bool isShareWindowOpen = false;

    private void Awake()
    {
        PlayerPrefs.DeleteAll();
        if (isInitialized)
            return;

        Instance = this;
        ResetState();

        if (!PlayerPrefs.HasKey("LASTSCORE"))
        {
            PlayerPrefs.SetInt("LASTSCORE", 0);
        }

        if (!PlayerPrefs.HasKey("HIGHSCORE"))
        {
            PlayerPrefs.SetInt("HIGHSCORE", 0);
        }

        if (!PlayerPrefs.HasKey("SOUND"))
        {
            PlayerPrefs.SetInt("SOUND", 1);
        }

        if (!PlayerPrefs.HasKey("VIBRATION"))
        {
            PlayerPrefs.SetInt("VIBRATION", 1);
        }

        if (!PlayerPrefs.HasKey("CUSTOMISATION"))
        {
            PlayerPrefs.SetInt("CUSTOMISATION", 0);
        }

        if (!PlayerPrefs.HasKey("CUSTOMISATION_NOTIFICATION"))
        {
            PlayerPrefs.SetInt("CUSTOMISATION_NOTIFICATION", 0);
            ClearNotificationItem();

        }
        else if(PlayerPrefs.GetInt("CUSTOMISATION_NOTIFICATION") >0)
        {
            SetNotificationItem(PlayerPrefs.GetInt("CUSTOMISATION_NOTIFICATION"));
        }
        else
        {
            ClearNotificationItem();
        }

        foreach (var item in playerObjects)
        {
            item.SetActive(false);
        }

        playerObjects[PlayerPrefs.GetInt("CUSTOMISATION")].SetActive(true);
        var i = scrollRect.content.GetComponentsInChildren<CustomizationItem>();
        foreach (var item in i)
        {
            customizationItems.Add(item);
            item.Refresh();
        }

        
        
        if (!PlayerPrefs.HasKey("NICKNAME"))
        {
            ShowFirstTimePopup();
        }
        else if (string.IsNullOrWhiteSpace(PlayerPrefs.GetString("NICKNAME")) | string.IsNullOrWhiteSpace(PlayerPrefs.GetString("NICKNAME")))
        {
            ShowFirstTimePopup();

        }
        else
        {
            ShowMenu();
        }



        mainScreenLastScore.text = PlayerPrefs.GetInt("LASTSCORE").ToString();
        mainScreenHighScore.text = PlayerPrefs.GetInt("HIGHSCORE").ToString();

        countDownText.text = "";
        scoreText.text = "";
        rockAnim = playerObject.GetComponent<Animator>();
        isInitialized = true;
    }

    public void Vibrate()
    {
        if (PlayerPrefs.GetInt("VIBRATION") == 0)
            return;

        Handheld.Vibrate();
    }

    public void ButtonPressed()
    {
        SoundManager.Instance.PlaySound(SOUNDS.Button);
        Vibrate();
    }

    public void ShowMenu()
    {
        startScreen.SetActive(true);
        firstTimePopup.SetActive(false);
        endGameScreen.SetActive(false);
        customizationScreen.SetActive(false);
        scoresScreen.gameObject.SetActive(false);

        vibrationToggleIcon.sprite = (PlayerPrefs.GetInt("VIBRATION") == 1) ? vibrationOnSprite : vibrationOffSprite;
        soundToggleIcon.sprite = (PlayerPrefs.GetInt("SOUND") == 1) ? soundOnSprite : soundOffSprite;

        playButtonImage.sprite = customizationItems[PlayerPrefs.GetInt("CUSTOMISATION")].itemImage;
        ResetState();
    }

    public void ToggleSound()
    {
        PlayerPrefs.SetInt("SOUND", Mathf.Abs(PlayerPrefs.GetInt("SOUND") - 1));
        soundToggleIcon.sprite = (PlayerPrefs.GetInt("SOUND") == 1) ? soundOnSprite : soundOffSprite;

    }

    public void ToggleVibration()
    {
        PlayerPrefs.SetInt("VIBRATION", Mathf.Abs(PlayerPrefs.GetInt("VIBRATION") - 1));
        vibrationToggleIcon.sprite = (PlayerPrefs.GetInt("VIBRATION") == 1) ? vibrationOnSprite : vibrationOffSprite;
    }

    public void CheckIfBusy()
    {
        isBusy = false;

        if (isShareWindowOpen)
            isBusy = true;


    }

    public void ShowHighScores()
    {
        if (isBusy)
            return;

        firstTimePopup.SetActive(false);
        startScreen.SetActive(false);
        customizationScreen.SetActive(false);
        endGameScreen.SetActive(false);
        scoresScreen.Open();
    }

    internal void SelectNewItem(CustomizationItem customizationItem)
    {
        PlayerPrefs.SetInt("CUSTOMISATION", customizationItem.ID);
        foreach (var item in playerObjects)
        {
            item.SetActive(false);
        }

        foreach (var item in customizationItems)
        {
            item.Refresh();
        }

        playerObjects[PlayerPrefs.GetInt("CUSTOMISATION")].SetActive(true);
    }

    public void CloseCustomizationScreen()
    {
        if (isBusy)
            return;

        ShowMenu();

        customizationScreen.SetActive(false);
    }

    public void OpenCustomizationScreen()
    {
        if (isBusy)
            return;

        startScreen.SetActive(false);
        customizationScreen.SetActive(true);
        ClearNotificationItem();
        UpdateCustomizationPage();

    }

    public void ShowFirstTimePopup()
    {
        firstTimePopup.SetActive(true);
        startScreen.SetActive(false);
        customizationScreen.SetActive(false);
        endGameScreen.SetActive(false);
        scoresScreen.gameObject.SetActive(false);

    }

    public void CloseFirstTimePopup()
    {
        ShowMenu();
        firstTimePopup.SetActive(false);
    }

    public void SkipFirstTimeSetup()
    {
        PlayerPrefs.SetString("NICKNAME", "Player");
        PlayerPrefs.SetInt("POSTHIGHSCORES", 0);
        CloseFirstTimePopup();

    }

    public void AcceptFirstTimePopup()
    {
        if(string.IsNullOrEmpty(nicknameInput.text) | string.IsNullOrWhiteSpace(nicknameInput.text))
        {
            return;
        }

        CloseFirstTimePopup();

        RegisterNewPlayer();
    }

    public void OnStartPressed()
    {
        if (isBusy)
            return;

        startScreen.SetActive(false);
        StartCoroutine(StartCountdown());
    }

    public IEnumerator StartCountdown()
    {
        endGameScreen.SetActive(false);
        ResetState();

        countDownText.text = "3";
        yield return new WaitForSecondsRealtime(1);
        countDownText.text = "2";
        yield return new WaitForSecondsRealtime(1);
        countDownText.text = "1";
        yield return new WaitForSecondsRealtime(1);
        countDownText.text = "Ready!";
        yield return new WaitForSecondsRealtime(1);
        countDownText.text = "";
        StartGame();
    }

    public void ResetState()
    {
        jumpProgress = 0.1f;
        currentHeight = (((maxHeight * randomModifier) * Mathf.Sin(Mathf.PI * jumpProgress)) - 0.1f);
        playerObject.position = new Vector3(0, currentHeight, 0);
        jumpProgress = 0.1f;
        speed = -0.2f;
        scoreText.text = "";
        fallSpeed = 0.5f;
        shoreline.transform.position = new Vector3(0, 0, 0);
        waterMaterial.material.SetVector("_SurfaceNoiseScroll", new Vector4(0, speed, 0, 0));
        skips = 0;

    }
    
    public void StartGame()
    {      
        isPlaying = true;
        scoreText.text = "0";
        rockAnim.SetBool("Active",true);
    }

    public void Retry()
    {
        if (isBusy)
            return;

        StartCoroutine(StartCountdown());
    }

    public void EndGame()
    {
        endGameScreen.SetActive(true);
        endGameScore.text = skips.ToString();
        rockAnim.SetBool("Active",false);

        if (skips > PlayerPrefs.GetInt("HIGHSCORE"))
        {
            PlayerPrefs.SetInt("HIGHSCORE", skips);
            endGameTitle.text = "New Highscore!";
            PostHighscore();

        }
        else
        {
            endGameTitle.text = "Too Bad!";
        }

        endGameHighScore.text = PlayerPrefs.GetInt("HIGHSCORE").ToString();
        PlayerPrefs.SetInt("LASTSCORE", skips);
        mainScreenLastScore.text = PlayerPrefs.GetInt("LASTSCORE").ToString();
        mainScreenHighScore.text = PlayerPrefs.GetInt("HIGHSCORE").ToString();

        UpdateCustomizationPage();
        //Hide Main UI
        scoreText.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        CheckIfBusy();


        if (!isPlaying)
            return;
       
        //Position update
        jumpProgress += Time.deltaTime * (fallSpeed* randomModifier);
        currentHeight = (((maxHeight * randomModifier) * Mathf.Sin(Mathf.PI * jumpProgress)) - 0.1f);
        playerObject.position = new Vector3(0, currentHeight, 0);
        shadowObject.transform.localScale = new Vector3(1 - currentHeight, 1 - currentHeight, 1 - currentHeight);

        if (shoreline.transform.position.z > -15)
            shoreline.position -= (Vector3.forward * (Mathf.Abs(speed)*10)) * Time.deltaTime;


        if (currentHeight < -0.15f)
        {
            //GameOver
            isPlaying = false;
            StartCoroutine(ShowRipple(2));
            EndGame();
            return;
        }

        //Input
        if (Input.GetMouseButtonDown(0) | Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
    }

    public IEnumerator ShowRipple(int v)
    {
        ripples[v].SetActive(true);
        yield return new WaitForSecondsRealtime(1);
        ripples[v].SetActive(false);
    }

    public void Jump()
    {
        if(currentHeight <= 0.06f && jumpProgress>0.5f)
        {
            //correct jump
            if(currentHeight <= 0.02f)
            {
                StartCoroutine(ShowRipple(1));
                Vibrate();
            }
            else
            {
                StartCoroutine(ShowRipple(0));
                Vibrate();
                Vibrate();
            }

            SoundManager.Instance.PlaySound(SOUNDS.Button);
            speed -= (1 / 100);
            waterMaterial.material.SetVector("_SurfaceNoiseScroll", new Vector4(0, speed, 0, 0));

            jumpProgress = 1 - jumpProgress;
            skips++;
            scoreText.text = skips.ToString();
            randomModifier = UnityEngine.Random.Range(0.8f, 1f);
            fallSpeed += jumpProgress;
            rockAnim.Play("Flip");

        }
        else if(jumpProgress > 0.7f)
        {
            fallSpeed += 0.05f;
        }

    }

    //Customization

    public void SetNotificationItem(int items)
    {
        PlayerPrefs.SetInt("CUSTOMISATION_NOTIFICATION", items);

        customizationNotificationtext.text = items.ToString();
        customizationNotificationIcon.SetActive(true);

        endgameNotificationtext.text = items.ToString();
        endgameNotificationIcon.SetActive(true);
    }

    public void ClearNotificationItem()
    {
        PlayerPrefs.SetInt("CUSTOMISATION_NOTIFICATION", 0);

        customizationNotificationIcon.SetActive(false);
        endgameNotificationIcon.SetActive(false);     
    }

    public void UpdateCustomizationPage()
    {
        int i = 0;
        int t = 0;
        foreach (var item in customizationItems)
        {
            if (item.isUnlocked)
                i++;
        }

        foreach (var item in customizationItems)
        {
            item.Refresh();

            if (item.isUnlocked)
                t++;
        }

        if (t > i)
            SetNotificationItem(t - i);



        scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, 600 * totalCustomizationPages);

        if(currentPage == 0)
        {
            prevPage.interactable = false;
        }
        else
        {
            prevPage.interactable = true;

        }

        if (currentPage == totalCustomizationPages-1)
        {
            nextPage.interactable = false;
        }
        else
        {
            nextPage.interactable = true;
        }


        scrollRect.content.anchoredPosition =new Vector2(0, ((scrollRect.content.rect.height / totalCustomizationPages)*currentPage));
    }

    public void NextPage()
    {
        currentPage++;
        currentPage = Mathf.Clamp(currentPage, 0, totalCustomizationPages - 1);
        UpdateCustomizationPage();
    }

    public void PrevPage()
    {
        currentPage--;
        currentPage = Mathf.Clamp(currentPage, 0, totalCustomizationPages - 1);
        UpdateCustomizationPage();
    }

    public void ShowShareWindow()
    {
        shareWindow.Play("Show");
        isShareWindowOpen = true;
    }
    public void HideShareWindow()
    {
        shareWindow.Play("Hide");
        isShareWindowOpen = false;

    }
    //Databse
    public void RegisterNewPlayer()
    {
        PlayerPrefs.SetString("NICKNAME", nicknameInput.text);
        PlayerPrefs.SetInt("POSTHIGHSCORES", 1);

        JSONNode node = new JSONClass();
        node["nickname"] = nicknameInput.text;
        node["highscore"].AsInt = 0;
        ServerManager.Instance.MakeServerRequest(OnRegister, OnError, new { action = "register", data = node });

    }

    private void OnRegister(JSONNode obj)
    {
        PlayerPrefs.SetInt("UID", obj[0]["uid"].AsInt);
    }

    private void PostHighscore()
    {
        JSONNode node = new JSONClass();
        node["uid"].AsInt = PlayerPrefs.GetInt("UID");
        node["highscore"].AsInt = PlayerPrefs.GetInt("HIGHSCORE");
        ServerManager.Instance.MakeServerRequest(OnComplete, OnError, new { action = "update", data = node });
    }

    public void OnComplete(JSONNode obj)
    {
        Debug.Log(obj);

    }

    private void OnError(string obj)
    {
        Debug.Log(obj);
    }
}
