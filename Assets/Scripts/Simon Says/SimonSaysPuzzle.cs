using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SimonSaysPuzzle : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    public List<GameObject> statues;
    public List<int> sequence;
    public int playerStep = 0;
    public bool isDisplaying = false;

    // The colors we will use
    public Color normalColor = Color.white;
    public Color activeColor = Color.gray;

    void Start()
    {
        ResetAllStatueColors();
        StartNewGame();
    }

    void StartNewGame()
    {
        sequence.Clear();
        AddStep();
    }

    void AddStep()
    {
        sequence.Add(Random.Range(0, statues.Count));
        StartCoroutine(DisplaySequence());
    }

    // 1. When displaying, one by one become grey
    IEnumerator DisplaySequence()
    {
        isDisplaying = true;
        playerStep = 0;
        ResetAllStatueColors();
        yield return new WaitForSeconds(1f);

        foreach (int index in sequence)
        {
            statues[index].GetComponent<SpriteRenderer>().color = activeColor;
            yield return new WaitForSeconds(0.6f);
            statues[index].GetComponent<SpriteRenderer>().color = normalColor;
            yield return new WaitForSeconds(0.2f);
        }

        // 2. When done displaying, all colors reset
        scoreText.text = $"Score: {sequence.Count}";
        ResetAllStatueColors();
        isDisplaying = false;
    }

    public void PlayerClicked(GameObject clickedStatue)
    {
        if (isDisplaying) return;

        int clickedIndex = statues.IndexOf(clickedStatue);

        if (clickedIndex == sequence[playerStep])
        {
            // 3. When a player clicks, it turns grey briefly
            StartCoroutine(FlashSingleStatue(clickedStatue));
            
            playerStep++;
            if (playerStep >= sequence.Count)
            {
                // 5. Once completed, all blink twice
                StartCoroutine(CorrectSequenceFeedback());
            }
        }
        else
        {
            // 4. If wrong, all blink grey and reset
            StartCoroutine(WrongSequenceFeedback());
        }
    }

    // Helper to reset everything to normal color
    void ResetAllStatueColors()
    {
        foreach (GameObject obj in statues)
        {
            obj.GetComponent<SpriteRenderer>().color = normalColor;
        }
    }

    // Feedback for clicking a single statue
    IEnumerator FlashSingleStatue(GameObject statue)
    {
        statue.GetComponent<SpriteRenderer>().color = activeColor;
        yield return new WaitForSeconds(0.2f);
        statue.GetComponent<SpriteRenderer>().color = normalColor;
    }

    // Feedback for completing a round (Blink twice)
    IEnumerator CorrectSequenceFeedback()
    {
        isDisplaying = true; // Prevent clicking during animation
        yield return new WaitForSeconds(0.3f);

        for (int i = 0; i < 2; i++)
        {
            SetAllColors(activeColor);
            yield return new WaitForSeconds(0.2f);
            SetAllColors(normalColor);
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(0.5f);
        AddStep();
    }

    // Feedback for failing (Blink once and reset)
    IEnumerator WrongSequenceFeedback()
    {
        isDisplaying = true;
        SetAllColors(activeColor);
        yield return new WaitForSeconds(0.5f);
        SetAllColors(normalColor);
        yield return new WaitForSeconds(0.5f);
        
        StartNewGame();
    }

    void SetAllColors(Color newColor)
    {
        foreach (GameObject obj in statues)
        {
            obj.GetComponent<SpriteRenderer>().color = newColor;
        }
    }
}
