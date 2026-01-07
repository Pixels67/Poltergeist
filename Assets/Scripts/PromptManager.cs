using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PromptManager : MonoBehaviour
{
    public bool FinishedShift { get; private set; }

    [SerializeField] private PromptList[] promptLists;
    [SerializeField] private TMP_Text uiText;
    [SerializeField] private float charDelaySeconds = 0.05f;

    private int _shiftCounter;
    private int _promptCounter;

    private Coroutine _displayPromptCoroutine;
    private Coroutine _displayTextCoroutine;

    private AudioSource _audioSource;

    public void AdvanceShift()
    {
        if (_shiftCounter + 1 < promptLists.Length)
        {
            _shiftCounter++;
            _promptCounter = 0;
            FinishedShift = false;
        }
    }

    private void OnEnable()
    {
        Button.OnButtonPressed += ButtonCallback;
    }

    private void OnDisable()
    {
        Button.OnButtonPressed -= ButtonCallback;
    }

    private void Awake()
    {
        _displayPromptCoroutine = StartCoroutine(DisplayNextPrompt());
        _audioSource = GetComponent<AudioSource>();
    }

    private void ButtonCallback(ButtonType type)
    {
        if (_displayPromptCoroutine != null)
        {
            return;
        }

        _displayPromptCoroutine = StartCoroutine(DisplayNextPrompt());
    }

    private IEnumerator DisplayNextPrompt()
    {
        var prompts = promptLists[_shiftCounter].prompts;

        if (_promptCounter >= prompts.Count)
        {
            yield break;
        }

        var prompt = prompts[_promptCounter];

        if (_displayTextCoroutine != null)
        {
            StopCoroutine(_displayTextCoroutine);
        }

        uiText.text = string.Empty;
        yield return new WaitForSeconds(prompt.delaySeconds);

        prompt.promptEvent.Invoke();
        _displayTextCoroutine = StartCoroutine(DisplayPromptText(prompt, _promptCounter != prompts.Count - 1 && prompt.hint));
        _promptCounter++;

        yield return new WaitForSeconds(prompt.pauseSeconds);

        if (_promptCounter >= prompts.Count)
        {
            FinishedShift = true;
        }

        var conditionsComplete = false;
        while (!conditionsComplete)
        {
            conditionsComplete = !prompt.conditions.Exists(condition => !condition.IsComplete());
            yield return null;
        }

        while (_displayTextCoroutine != null)
        {
            yield return null; // Just wait
        }

        _displayPromptCoroutine = null;
    }

    private IEnumerator DisplayPromptText(Prompt prompt, bool hint = true)
    {
        string buffer = prompt.text;

        if (hint)
        {
            buffer += prompt.type switch
            {
                PromptType.Continue => "\n \n[Press any button to continue]",
                PromptType.YesNo => "\n \n[Y/N]",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        while (buffer.Length != 0)
        {
            if (buffer[0] == ' ')
            {
                yield return new WaitForSeconds(charDelaySeconds);
            }
            else if (buffer[0] != '\\')
            {
                yield return new WaitForSeconds(charDelaySeconds);
                _audioSource?.Play();
            }
            else if (buffer[1] == 'd')
            {
                buffer = buffer[2..];
                yield return new WaitForSeconds(1);
                continue;
            }

            uiText.text += buffer[0];
            buffer = buffer[1..];
        }

        _displayTextCoroutine = null;
    }
}

public enum ConditionType
{
    ObjectIsActive,
    ObjectIsInactive
}

public enum PromptType
{
    YesNo,
    Continue
}

[System.Serializable]
internal struct PromptList
{
    public List<Prompt> prompts;
}

[System.Serializable]
internal struct Condition
{
    public ConditionType type;
    public GameObject gameObject;

    public bool IsComplete()
    {
        return type switch
        {
            ConditionType.ObjectIsActive => gameObject.activeSelf,
            ConditionType.ObjectIsInactive => !gameObject.activeSelf,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

[System.Serializable]
internal struct Prompt
{
    public PromptType type;
    [TextArea(3, 20)] public string text;
    public bool hint;
    public UnityEvent promptEvent;
    public List<Condition> conditions;
    public float delaySeconds;
    public float pauseSeconds;
}