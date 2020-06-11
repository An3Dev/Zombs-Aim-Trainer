using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using TMPro;
using System.Linq;

public class KillFeed : MonoBehaviourPun
{

    [SerializeField] GameObject killFeedTextPrefab;

    GameObject[] spawnedTextArray;

    float lastSpawnTime;
    float lifetimeOfText = 15;

    // Start is called before the first frame update
    void Start()
    {
        spawnedTextArray = new GameObject[0];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [PunRPC]
    public void AddKill(string killer, string victim)
    {
        GameObject text = null;

        // look for already instantiated text in list
        for(int i = 0; i < spawnedTextArray.Length; i++)
        {
            if (!spawnedTextArray[i].activeInHierarchy)
            {
                text = spawnedTextArray[i];
                break;
            }
        }
        // if there are not any available text, the spawn text
        if (text == null)
        {
            text = Instantiate(killFeedTextPrefab, transform);
            spawnedTextArray.Append(text);
        }
        
        text.GetComponent<TextMeshProUGUI>().text = killer + " eliminated " + victim;
        StartCoroutine(DisableText(lifetimeOfText, text));
    }

    IEnumerator DisableText(float seconds, GameObject text)
    {
        yield return new WaitForSeconds(seconds);
        text.SetActive(false);
    }
}
