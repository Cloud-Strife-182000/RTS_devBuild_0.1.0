using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RTSButton : MonoBehaviour
{
    private Player_Unit PU;
    public Player_Structure PS;
    private UnitCanvas UC;

    private ActivePlayer player;

    public Attributes attr;

    public Button button;
    public Toggle toggle;

    public int index;
    public int militaryButtonType;
    public int AgeToActivate;

    public ProgressBarPro researchBar, spawnBar;
    public float spawnCountdown, spawnRate, spawnIncrement;
    public float researchCountdown, researchRate, researchIncrement;

    public bool isMenuButton, isTechTreeButton, isAdvanceAgeButton, isBuildEconomic, isBuildMilitary, isFrontPanel, isResearchButton, isMinimapButton, isMainMenuButton, isReleaseGarrisonButton, isUpgradeButton, isObjectivesButton;
    private bool paused;
    public bool researchStarted;

    public GameObject objectivesBox;
    public Canvas techTree;
    public GameObject minimap;
    public Image buttonImage;

    public Sprite nextUpgradeSprite;

    public int queueIndex;

    public RTSButton nextTech;

    public bool buildMiningVault;

    // Start is called before the first frame update
    void Start()
    {
        if (GetComponentInParent<UnitCanvas>())
        {
            UC = GetComponentInParent<UnitCanvas>();
        }

        if (GetComponentInParent<Player_Unit>())
        {
            PU = GetComponentInParent<Player_Unit>();
        }

        if (GetComponentInParent<Player_Structure>())
        {
            PS = GetComponentInParent<Player_Structure>();
        }

        if (GetComponent<Button>())
        {
            button = GetComponent<Button>();
            buttonImage = GetComponent<Image>();
        }
        else if (GetComponent<Toggle>())
        {
            toggle = GetComponent<Toggle>();
        }

        if (button)
        {
            if (isMenuButton)
            {
                player = FindObjectOfType<ActivePlayer>();
                button.onClick.AddListener(delegate { StopTimeAndShowMenu(); });

                if (isTechTreeButton)
                {
                    button.onClick.AddListener(delegate { ToggleTechTree(); });
                }

                if (isObjectivesButton)
                {
                    button.onClick.AddListener(delegate { ToggleObjectivesBox(); });
                }
            }
            else if (isMainMenuButton)
            {
                player = FindObjectOfType<ActivePlayer>();
                button.onClick.AddListener(delegate { player.superManager.ExitToMainMenu(); });
            }
            else if (PS)
            {
                if(index >= 0)
                {
                    button.onClick.AddListener(delegate { InitiateSpawn(); });
                    spawnBar = GetComponentInChildren<ProgressBarPro>();
                    spawnBar.gameObject.SetActive(false);

                    spawnIncrement = PS.spawnables[index].spawnIncrement;
                    spawnRate = PS.spawnables[index].spawnRate;
                }

                if (isAdvanceAgeButton)
                {
                    button.onClick.AddListener(delegate { AdvanceAge(); });
                    attr = GetComponent<Attributes>();
                    researchBar = GetComponentInChildren<ProgressBarPro>();
                    researchBar.gameObject.SetActive(false);
                }

                if (isResearchButton)
                {
                    button.onClick.AddListener(delegate { InitiateResearch(); });
                    attr = GetComponent<Attributes>();
                    researchBar = GetComponentInChildren<ProgressBarPro>();
                    researchBar.gameObject.SetActive(false);
                }

                if (isReleaseGarrisonButton)
                {
                    button.onClick.AddListener(delegate { PS.ReleaseGarrison(); });
                }
            }
            else if (PU)
            {
                if(PU.unitType == 1)
                {
                    if (index >= 0)
                    {
                        button.onClick.AddListener(delegate { PU.CallBuildFunction(index); });
                    }

                    if (isBuildEconomic)
                    {
                        button.onClick.AddListener(delegate { UC.ActivateEconomicBuild(); });
                    }
                    else if (isBuildMilitary)
                    {
                        button.onClick.AddListener(delegate { UC.ActivateMilitaryBuild(); });
                    }
                    else if (isFrontPanel)
                    {
                        button.onClick.AddListener(delegate { UC.ActivateFrontPanel(); });
                    }
                }

                if(militaryButtonType == 2)
                {
                    button.onClick.AddListener(delegate { PU.AttackOnLeftClick(); });
                }
                else if(militaryButtonType == 3)
                {
                    button.onClick.AddListener(delegate { PU.Die(); });
                }
                else if(militaryButtonType == 4)
                {
                    button.onClick.AddListener(delegate { PU.DropOffRemainingUnits(); });
                }
            }
            else if (isMinimapButton)
            {
                player = FindObjectOfType<ActivePlayer>();
                button.onClick.AddListener(delegate { ToggleMinimap(); });
            }
        }
        else if (toggle)
        {
            toggle.isOn = true;

            if(militaryButtonType == 1)
            {
                toggle.onValueChanged.AddListener(delegate { PU.ToggleRetaliation(); });
            }
        }
    }

    private void StopTimeAndShowMenu()
    {
        if (!player.paused)
        {
            Time.timeScale = 0;
            player.paused = true;
        }
        else
        {
            Time.timeScale = 1;
            player.paused = false;
        }
        
    }

    private void ToggleTechTree()
    {
        if (player.paused)
        {
            if (player.existingCanvasOpen)
            {
                player.existingCanvasOpen.gameObject.SetActive(false);
            }

            techTree.gameObject.SetActive(true);
            player.existingCanvasOpen = techTree;
        }
        else
        {
            techTree.gameObject.SetActive(false);
            player.existingCanvasOpen = null;
        }
    }

    private void ToggleObjectivesBox()
    {
        if (player.paused)
        {
            objectivesBox.SetActive(true);
        }
        else
        {
            objectivesBox.SetActive(false);
        }
    }

    private void ToggleMinimap()
    {
        if (!minimap.activeSelf)
        {
            player.minimapCam.SetActive(true);
            player.minimapViewportCam.SetActive(true);
            minimap.gameObject.SetActive(true);
        }
        else
        {
            minimap.gameObject.SetActive(false);
            player.minimapViewportCam.SetActive(false);
            player.minimapCam.SetActive(false);
        }
    }

    private void AdvanceAge()
    {
        InitiateResearch();
    }

    private void InitiateResearch()
    {
        if (PS.CheckResearchCosts(attr) && !researchStarted && PS.queueCounter < PS.queueImages.Count)
        {
            PS.AccountForResearchCosts(attr);

            PS.taskQueue.Add(this);
            PS.queueCounter++;
            queueIndex = PS.queueCounter-1;

            if (PS.queueImages[queueIndex].sprite != null)
            {
                foreach (Image qimg in PS.queueImages)
                {
                    if (qimg.sprite == null)
                    {
                        qimg.sprite = buttonImage.sprite;
                        queueIndex = PS.queueImages.IndexOf(qimg);
                    }
                }
            }
            else
            {
                PS.queueImages[queueIndex].sprite = buttonImage.sprite;
            }

            PS.queueIndexes.Add(queueIndex);
            PS.queueImages[queueIndex].gameObject.SetActive(true);
            researchStarted = true;
        }
    }

    private void InitiateSpawn()
    {
        if (PS.CheckUnitCosts(PS.spawnables[index]) && PS.queueCounter < PS.queueImages.Count)
        {
            PS.AccountForUnitCosts(PS.spawnables[index]);

            PS.taskQueue.Add(this);
            PS.queueCounter++;
            queueIndex = PS.queueCounter-1;

            if(PS.queueImages[queueIndex].sprite != null)
            {
                foreach(Image qimg in PS.queueImages)
                {
                    if(qimg.sprite == null)
                    {
                        qimg.sprite = buttonImage.sprite;
                        queueIndex = PS.queueImages.IndexOf(qimg);
                    }
                }
            }
            else
            {
                PS.queueImages[queueIndex].sprite = buttonImage.sprite;
            }

            PS.queueIndexes.Add(queueIndex);
            PS.queueImages[queueIndex].gameObject.SetActive(true);
        }
    }
}
