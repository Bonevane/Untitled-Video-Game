using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class TypewriterEffect : MonoBehaviour
{
    // [SerializeField] private float typewriterSpeed = 50f;    // MOVED TO INDIVIDUAL DIALOGUE OBJECT

    public bool isRunning { get; private set; }

    private readonly List<Punctuation> punctuations = new List<Punctuation>()
    {
        new Punctuation(new HashSet<char>() {'.', '!', '?'}, 0.6f),
        new Punctuation(new HashSet<char>() {',', ';', ':'}, 0.3f)
    };


    private Coroutine typingCoroutine;

    public void Run(string textToType, TMP_Text textLabel, bool isJittery, float speed)
    {
        typingCoroutine = StartCoroutine(TypeText(textToType, textLabel, isJittery, speed));
    }

    public void Stop()
    {
        StopCoroutine(typingCoroutine);
        isRunning = false;
    }

    private IEnumerator TypeText(string textToType, TMP_Text textLabel, bool isJittery, float speed)
    {
        isRunning = true;
        textLabel.text = textToType;
        textLabel.maxVisibleCharacters = 0;

        float t = 0;
        int charIndex = 0;

        JitterTextEffect jitterEffect = textLabel.GetComponent<JitterTextEffect>();

        if (isJittery) jitterEffect.EnableJitter();
        else jitterEffect.DisableJitter();

        while (charIndex < textLabel.text.Length)
        {
            int lastCharIndex = charIndex;

            t += Time.deltaTime * speed;
            charIndex = Mathf.FloorToInt(t);
            charIndex = Mathf.Clamp(charIndex, 0, textLabel.text.Length);


            for (int i = lastCharIndex; i < charIndex; i++)
            {
                bool isLast = i >= textLabel.text.Length - 1;

                textLabel.maxVisibleCharacters = i + 1;

                if (IsPunctuation(textLabel.text[i], out float waitTime) && !isLast && !IsPunctuation(textLabel.text[i + 1], out _))
                {
                    yield return new WaitForSeconds(waitTime);
                }
            }


            yield return null;
        }

        isRunning = false;
    } 


    private bool IsPunctuation(char ch, out float waitTime)
    {
        foreach(Punctuation punctuationCategory in punctuations)
        {
            if (punctuationCategory.Punctuations.Contains(ch))
            {
                waitTime = punctuationCategory.WaitTime;
                return true;
            }
        }

        waitTime = default; return false;
    }

    private readonly struct Punctuation
    {
        public readonly HashSet<char> Punctuations;
        public readonly float WaitTime;

        public Punctuation(HashSet<char> punctuations, float waitTime)
        {
            Punctuations = punctuations;
            WaitTime = waitTime;
        }
    }
}
