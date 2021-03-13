using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    public SuperManager SM;

    IEnumerator LoadScene()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            if (SM.isTutorialScene)
            {
                SM.LoadTutorialScene();
            }
            else
            {
                SM.LoadScene();
            }

            gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        StartCoroutine(LoadScene());
    }
}
