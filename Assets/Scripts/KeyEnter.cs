using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class KeyEnter : MonoBehaviour
{
    Button buttonMe;

    void Start()
    {
        buttonMe = GetComponent<Button>();

        // Ensure the first button is selected when the UI appears
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

    void Update()
    {
        // Get currently selected button
        GameObject selected = EventSystem.current.currentSelectedGameObject;

        // Move Down
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveSelection(1);
        }

        // Move Up
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveSelection(-1);
        }

        // Press Enter or Space to Click
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            if (selected != null)
            {
                selected.GetComponent<Button>()?.onClick.Invoke();
            }
        }
    }

    void MoveSelection(int direction)
    {
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null) return;

        Selectable current = selected.GetComponent<Selectable>();

        // Find next selectable button
        Selectable next = (direction > 0) ? current.FindSelectableOnDown() : current.FindSelectableOnUp();

        if (next != null)
        {
            EventSystem.current.SetSelectedGameObject(next.gameObject);
        }
    }
}
