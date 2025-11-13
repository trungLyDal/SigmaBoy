using UnityEngine;
using TMPro;

public class MonologueTrigger : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialoguePanel;   
    public TMP_Text dialogueText;      

    [Header("Monologue Lines")]
    [TextArea(3, 10)] 
    public string[] lines;

    private int index; 

    void Update()
    {
        if (dialoguePanel.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                NextLine();
            }
        }
    }

    // This function is already public, which is perfect.
    // Our new menu script will call this function.
    public void StartMonologue()
    {
        index = 0;
        dialoguePanel.SetActive(true);
        dialogueText.text = lines[index];
    }

    void NextLine()
    {
        index++; 
        if (index < lines.Length)
        {
            dialogueText.text = lines[index];
        }
        else
        {
            EndMonologue();
        }
    }

    void EndMonologue()
    {
        dialoguePanel.SetActive(false);
    }
}