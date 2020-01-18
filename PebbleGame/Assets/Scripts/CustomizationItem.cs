using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomizationItem : MonoBehaviour
{

    public bool isSelected = false;
    public bool isUnlocked;
    public int unlockRequirement;
    public string displayName;
    public int ID;
    public Image image;
    public Text itemText;
    public Image activeIcon;
    public Sprite itemImage;
    public Sprite lockImage;
    // Start is called before the first frame update
    void Start()
    {
        lockImage = image.sprite;
        activeIcon.enabled = false;

        if (PlayerPrefs.GetInt("CUSTOMISATION") == ID)
            isSelected = true;

        Refresh();
    }

    public void OnSelected()
    {
        if (!isUnlocked)
            return;

        GameManager.Instance.SelectNewItem(this);
        isSelected = true;

    }
    public void Unselect()
    {
        isSelected = false;
        Refresh();
    }

    public void Refresh()
    {
        if(PlayerPrefs.GetInt("HIGHSCORE") >= unlockRequirement)
        {
            isUnlocked = true;
            image.sprite = itemImage;
            itemText.text = displayName;
            activeIcon.enabled = isSelected;

        }
        else
        {
            isUnlocked = false;
            image.sprite = lockImage;
            itemText.text = unlockRequirement.ToString();
            activeIcon.enabled = false;


        }
    }
}
