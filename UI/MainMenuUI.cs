using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HSVPicker;

public class MainMenuUI : MonoBehaviour
{
    private Button button;
    private Toggle toggle;
    private Dropdown dropdown;
    private Slider slider;
    private ColorPicker colorPicker;
    public BookPro book;

    [Header("References")]

    public SuperManager superManager;

    [Header("Any Menu to Switch On/Off?")]

    public GameObject[] menusToSwitchOff;

    public GameObject[] menusToToggle;
    public Vector3[] initialPositions = new Vector3[2];

    [Header("Generic Booleans")]

    public bool NewGame;
    public bool quitGame;
    public int turnPages; // 1 -> right, 2 -> left

    [Header("Is it a location on the World Map?")]

    public bool isLocation;
    public int mapIndex;
    public GameObject newGameButton;
    public Image locImage;
    public Text locationName, locDesc, regInfo, histInfo, geoInfo, ctryName;

    public Sprite locationImage;
    public string nameOfPlace, locationDescription, regionInfo, historyInfo, geographyInfo, countryName;

    [Header("Is it a tutorial campaign scenario?")]

    public bool isTutMap;
    public int tutmapIndex;

    [Header("Game Options")]

    public bool numOfPlayersDropdown;

    [Header("Graphics Settings")]

    public bool shadows;
    public GameObject shadowSlider;
    public bool bloom;
    public GameObject bloomSlider;
    public bool fogOfWar;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < menusToToggle.Length; i++)
        {
            initialPositions[i] = menusToToggle[i].transform.position;
        }

        if (GetComponent<Button>())
        {
            button = GetComponent<Button>();
        }
        else if (GetComponent<Toggle>())
        {
            toggle = GetComponent<Toggle>();
        }
        else if (GetComponent<Dropdown>())
        {
            dropdown = GetComponent<Dropdown>();
        }
        else if (GetComponent<ColorPicker>())
        {
            colorPicker = GetComponent<ColorPicker>();
        }
        else if (GetComponent<Slider>())
        {
            slider = GetComponent<Slider>();
        }

        if (menusToToggle.Length > 0)
        {
            button.onClick.AddListener(delegate { ToggleMenus(); });
        }

        if (menusToSwitchOff.Length > 0)
        {
            button.onClick.AddListener(delegate { SwitchOffMenus(); });
        }

        if (isLocation)
        {
            button.onClick.AddListener(delegate { SwitchLoc(); });
        }

        if (NewGame)
        {
            if (superManager)
            {
                button.onClick.AddListener(delegate { superManager.isTutorialScene = false; superManager.loadingScreen.SetActive(true); });
            }
        }

        if (isTutMap)
        {
            if (superManager)
            {
                button.onClick.AddListener(delegate { superManager.map_index = tutmapIndex; superManager.isTutorialScene = true; superManager.loadingScreen.SetActive(true); });
            }
        }

        if (quitGame)
        {
            button.onClick.AddListener(delegate { Application.Quit(); });
        }

        if (shadows)
        {
            if (toggle)
            {
                toggle.onValueChanged.AddListener(delegate { superManager.SetShadows(); shadowSlider.SetActive(toggle.isOn); });
            }
            else if (slider)
            {
                slider.value = superManager.mainPlayer.globalLight.shadowStrength;

                slider.onValueChanged.AddListener(delegate { superManager.mainPlayer.globalLight.shadowStrength = slider.value; });
            }
        }

        if (bloom)
        {
            if (toggle)
            {
                toggle.onValueChanged.AddListener(delegate { superManager.SetBloom(); bloomSlider.SetActive(toggle.isOn); });
            }
            else if (slider)
            {
                slider.value = superManager.AssignBloom();

                slider.onValueChanged.AddListener(delegate { superManager.AdjustBloom(slider.value); });
            }
        }

        if (fogOfWar)
        {
            toggle.onValueChanged.AddListener(delegate { superManager.SetFog(); });
        }

        if (numOfPlayersDropdown)
        {
            dropdown.onValueChanged.AddListener(delegate { superManager.numOfAIs = dropdown.value + 1; AssignColours(); });
        }

        if (colorPicker)
        {
            AssignColours();

            colorPicker.onValueChanged.AddListener(delegate { AssignColours(); });
        }

        if(turnPages > 0)
        {
            if (book)
            {
                button.onClick.AddListener(delegate { TurnPages(); });
            }
        }
    }

    private void ToggleMenus()
    {
        for (int i = 0; i < menusToToggle.Length; i++)
        {
            if (menusToToggle[i].activeSelf)
            {
                if (!isLocation)
                {
                    menusToToggle[i].SetActive(false);
                }
            }
            else
            {
                menusToToggle[i].transform.position = initialPositions[i];

                menusToToggle[i].SetActive(true);
            }
        }
    }

    private void SwitchOffMenus()
    {
        for(int i = 0; i < menusToSwitchOff.Length; i++)
        {
            if (menusToSwitchOff[i])
            {
                if (menusToSwitchOff[i].activeSelf)
                {
                    menusToSwitchOff[i].SetActive(false);
                }
            }
        }
    }

    private void SwitchLoc()
    {
        if (locationImage)
        {
            locImage.sprite = locationImage;
        }
        else
        {
            locImage.sprite = null;
        }

        locationName.text = nameOfPlace;
        locDesc.text = locationDescription;
        regInfo.text = regionInfo;
        histInfo.text = historyInfo;
        geoInfo.text = geographyInfo;
        ctryName.text = countryName;

        if(superManager.maps.Length > mapIndex)
        {
            if (superManager.maps[mapIndex])
            {
                superManager.map_index = mapIndex;

                if (!newGameButton.activeSelf)
                {
                    newGameButton.SetActive(true);
                }
            }
            else
            {
                if (newGameButton.activeSelf)
                {
                    newGameButton.SetActive(false);
                }
            }
        }
        else
        {
            if (newGameButton.activeSelf)
            {
                newGameButton.SetActive(false);
            }
        }
    }

    private void AssignColours()
    {
        if (!colorPicker)
        {
            colorPicker = FindObjectOfType<ColorPicker>();
        }

        Color col = colorPicker.CurrentColor;

        superManager.playerColours[0] = col;

        for(int i=1; i <= superManager.numOfAIs; i++)
        {
            superManager.playerColours[i] = new Color(col.r + Random.Range(-1f, 1f), col.g + Random.Range(-1f, 1f), col.b + Random.Range(-1f, 1f), col.a);
        }
    }

    private void TurnPages()
    {
        if(turnPages == 1)
        {
            if(book.CurrentPaper < book.papers.Length)
            {
                book.OnMouseDragRightPage();
                book.TweenForward();
            }
        }
        else if(turnPages == 2)
        {
            if(book.CurrentPaper != 0)
            {
                book.OnMouseDragLeftPage();
                book.TweenForward();
            }
        }
    }
}
