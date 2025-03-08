using System;
using UnityEngine;

public class DialogueActivator : MonoBehaviour, IInteractable
{
    [SerializeField] private DialogueObject dialogueObject;
    private Player playerInRange;


    public void UpdateDialogueObject (DialogueObject dialogueObject)
    {
        this.dialogueObject = dialogueObject;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered");
        if(other.CompareTag("Player") && other.TryGetComponent(out Player player))
        {
            player.Interactable = this;
            playerInRange = player;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player") && other.TryGetComponent(out Player player))
        {
            if (player.Interactable is DialogueActivator dialogueActivator && dialogueActivator == this ) {
                player.Interactable = null;
            }
        }
    }

    private void OnDisable()
    {
        if (playerInRange == null) return;

        if (playerInRange.Interactable is DialogueActivator dialogueActivator && dialogueActivator == this)
        {
            playerInRange.Interactable = null;
        }
    }

    public void Interact(Player player)
    {
        foreach(DialogueResponseEvent responseEvents in GetComponents<DialogueResponseEvent>())
        {
            if (responseEvents.DialogueObject == dialogueObject)
            {
                player.DialogueUI.AddResponseEvents(responseEvents.Events);
                break;
            }
        }

        player.DialogueUI.ShowDialogue(dialogueObject);
    }
}
