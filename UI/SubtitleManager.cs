using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubtitleManager : MonoBehaviour
{
    public ActivePlayer player;

    public Text subtitleText;

    public int subtitleListIndex;

    public List<Subtitles> subtitleList;

    public int currentSubtitleIndex;

    public Button button;

    void Start()
    {
        button = GetComponent<Button>();

        if (button)
        {
            button.onClick.AddListener(delegate { GoToNextSubtitle(); });
        }
    }

    public void GoToNextSubtitle()
    {
        if(currentSubtitleIndex < subtitleList[subtitleListIndex].strings.Count - 1)
        {
            currentSubtitleIndex++;

            subtitleText.text = subtitleList[subtitleListIndex].strings[currentSubtitleIndex];
        }
        else
        {
            player.ToggleOffSubtitles();

            currentSubtitleIndex = 0;
        }
    }

    public void GoToNextInstruction()
    {
        if (currentSubtitleIndex < subtitleList[subtitleListIndex].strings.Count - 1)
        {
            currentSubtitleIndex++;

            subtitleText.text = subtitleList[subtitleListIndex].strings[currentSubtitleIndex];
        }
        else
        {
            player.instructions.SetActive(false);

            currentSubtitleIndex = 0;
        }
    }
}
