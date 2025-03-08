using System.Collections;
using UnityEngine;
using TMPro;

public class JitterTextEffect : MonoBehaviour
{
    private TMP_Text textMesh;
    private bool isJittering = false;

    [SerializeField] private float jitterAmount = 2f; // How much it shakes
    [SerializeField] private float jitterSpeed = 25f; // Speed of shaking

    private void Awake()
    {
        textMesh = GetComponent<TMP_Text>();
    }

    public void EnableJitter()
    {
        if (!isJittering)
        {
            isJittering = true;
            StartCoroutine(JitterEffect());
        }
    }

    public void DisableJitter()
    {
        isJittering = false;
    }

    private IEnumerator JitterEffect()
    {
        while (isJittering)
        {
            textMesh.ForceMeshUpdate();
            var textInfo = textMesh.textInfo;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible) continue;

                int vertexIndex = textInfo.characterInfo[i].vertexIndex;
                var verts = textInfo.meshInfo[textInfo.characterInfo[i].materialReferenceIndex].vertices;

                float shakeX = Random.Range(-jitterAmount, jitterAmount);
                float shakeY = Random.Range(-jitterAmount, jitterAmount);

                for (int j = 0; j < 4; j++) // Each character has 4 vertices
                {
                    verts[vertexIndex + j] += new Vector3(shakeX, shakeY, 0);
                }
            }

            // Apply changes
            for (int i = 0; i < textMesh.textInfo.meshInfo.Length; i++)
            {
                textMesh.textInfo.meshInfo[i].mesh.vertices = textMesh.textInfo.meshInfo[i].vertices;
                textMesh.UpdateGeometry(textMesh.textInfo.meshInfo[i].mesh, i);
            }

            yield return new WaitForSeconds(1f / jitterSpeed);
        }
    }
}
