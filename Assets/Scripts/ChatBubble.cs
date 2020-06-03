using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ChatBubble : MonoBehaviour
{

    Transform background;
    TextMeshPro text;
    // Start is called before the first frame update
    void Awake()
    {
        background = transform.Find("Background");
        text = transform.Find("Text").GetComponent<TextMeshPro>();
        Setup("Testing testing testing testing Testing testing testing testing Testing testing testing testing Testing testing testing testing");
        Debug.Log(background.localScale);
    }

    public void Setup(string wantedText)
    {
        text.text = wantedText;
        text.ForceMeshUpdate();
        Vector2 bounds = text.GetRenderedValues();

        Vector2 padding = new Vector2(0.1f, 0.1f);
        //padding = Vector3.zero;
        background.localScale = bounds + padding * 2;
        background.transform.position = new Vector3(background.localScale.x / 2 - (padding.x / 2), background.transform.position.y / 2 + (padding.y / 2));
    }
}
