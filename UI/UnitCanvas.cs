using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnitCanvas : MonoBehaviour
{
    private Player_Structure PS;
    public ActivePlayer player;

    public CanvasRenderer ActionPanel;

    public List<RTSButton> frontPanelActions, economicBuildActions, militaryBuildActions;

    private bool actionPanelSetActive;

    public GameObject FrontButtons, EconomicBuildPanel, MilitaryBuildPanel;

    private void Awake()
    {
        if (FrontButtons)
        {
            frontPanelActions = FrontButtons.GetComponentsInChildren<RTSButton>().ToList();
        }

        if (EconomicBuildPanel)
        {
            economicBuildActions = EconomicBuildPanel.GetComponentsInChildren<RTSButton>().ToList();
        }

        if (MilitaryBuildPanel)
        {
            militaryBuildActions = MilitaryBuildPanel.GetComponentsInChildren<RTSButton>().ToList();
        }
    }

    void Start()
    {
        if (GetComponentInParent<Player_Structure>())
        {
            PS = GetComponentInParent<Player_Structure>();

            ActionPanel.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (PS)
        {
            if (!actionPanelSetActive)
            {
                if (PS.building.isBuilt)
                {
                    ActionPanel.gameObject.SetActive(true);

                    actionPanelSetActive = true;
                }
            }
        }
    }

    private void OnEnable()
    {
        if (FrontButtons && player)
        {
            ActivateFrontPanel();
        }
    }

    public void ActivateEconomicBuild()
    {
        FrontButtons.SetActive(false);
        EconomicBuildPanel.SetActive(true);
        ActivateAgeSpecificButtons(economicBuildActions);
    }

    public void ActivateMilitaryBuild()
    {
        FrontButtons.SetActive(false);
        MilitaryBuildPanel.SetActive(true);
        ActivateAgeSpecificButtons(militaryBuildActions);
    }

    public void ActivateFrontPanel()
    {
        if (EconomicBuildPanel)
        {
            EconomicBuildPanel.SetActive(false);
        }

        if (MilitaryBuildPanel)
        {
            MilitaryBuildPanel.SetActive(false);
        }
        
        FrontButtons.SetActive(true);
        ActivateAgeSpecificButtons(frontPanelActions);
    }

    public void ActivateAgeSpecificButtons(List<RTSButton> actions)
    {
        foreach (RTSButton action in actions)
        {
            if (action.AgeToActivate <= player.Age)
            {
                if(action.buildMiningVault)
                {
                    if (player.researched[4])
                    {
                        action.gameObject.SetActive(true);
                    }
                    else
                    {
                        action.gameObject.SetActive(false);
                    }
                }
                else
                {
                    action.gameObject.SetActive(true);
                }
            }
            else
            {
                action.gameObject.SetActive(false);
            }
        }

        foreach (RTSButton action in actions)
        {
            if (action.AgeToActivate <= player.Age)
            {
                if (action.nextTech)
                {
                    action.nextTech.gameObject.SetActive(false);
                }
            }
        }
    }
}
