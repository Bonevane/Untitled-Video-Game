using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/DialogueObject")] 

public class DialogueObject : ScriptableObject
{
    [SerializeField][TextArea] private string[] dialogue;
    [SerializeField] private Response[] responses;
    [SerializeField] private bool isJittery = false;
    [SerializeField] private float typeWritingSpeed = 50f;


    public string[] Dialogue => dialogue;

    public bool IsJittery => isJittery;

    public float TypeWritingSpeed => typeWritingSpeed;

    public bool HasResponses => Responses != null && Responses.Length > 0;

    public Response[] Responses => responses;
}
