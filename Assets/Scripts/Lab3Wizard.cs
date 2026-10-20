using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Lab3Wizard : MonoBehaviour
{
    [Header("Pages: operator, check, question, result")]
    [SerializeField] private GameObject[] pages;

    [Header("Form")]
    [SerializeField] private TMP_InputField operatorInput;
    [SerializeField] private Toggle inspectedToggle;
    [SerializeField] private Slider temperatureSlider;

    [Header("Labels")]
    [SerializeField] private TMP_Text stepText;
    [SerializeField] private TMP_Text temperatureText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text resultText;

    [Header("Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button saveButton;

    private int step;
    private int errors;
    private bool answerCorrect;
    private bool started;
    private bool completed;

    private float startedAt;
    private float finishedSeconds;

    [Serializable]
    private class Record
    {
        public string operatorName;
        public bool inspected;
        public float temperature;
        public bool answerCorrect;
        public bool completed;
        public int errors;
        public float durationSeconds;
        public string savedAtUtc;
    }

    private void Start()
    {
        temperatureSlider.onValueChanged.AddListener(
            UpdateTemperature);

        ResetAll();
    }

    private void OnDestroy()
    {
        temperatureSlider.onValueChanged.RemoveListener(
            UpdateTemperature);
    }

    private void BeginIfNeeded()
    {
        if (started)
            return;

        started = true;
        startedAt = Time.realtimeSinceStartup;
    }

    private float Elapsed()
    {
        if (!started)
            return 0f;

        return completed
            ? finishedSeconds
            : Time.realtimeSinceStartup - startedAt;
    }

    private void UpdateTemperature(float value)
    {
        temperatureText.text =
            $"Temperature, °C: {value:0} °C";
    }

    private void ShowStep()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == step);
        }

        stepText.text = $"Step {step + 1}/{pages.Length}";

        backButton.interactable = step > 0 && !completed;
        nextButton.gameObject.SetActive(!completed);
        saveButton.gameObject.SetActive(!completed);
    }

    private void Reject(string message)
    {
        errors++;
        messageText.text = message;
    }

    public void Next()
    {
        if (completed)
            return;

        BeginIfNeeded();

        if (step == 0 &&
            string.IsNullOrWhiteSpace(operatorInput.text))
        {
            Reject("Enter operator name.");
            return;
        }

        if (step == 1)
        {
            if (!inspectedToggle.isOn)
            {
                Reject("Confirm equipment inspection.");
                return;
            }

            float temperature = temperatureSlider.value;

            if (temperature < 18f || temperature > 27f)
            {
                Reject("Choose temp between 18–27 °C for this scenario.");
                return;
            }
        }

        if (step == 2 && !answerCorrect)
        {
            Reject("First choose the right answer.");
            return;
        }

        messageText.text = "";

        if (step == 2)
        {
            Finish();
            return;
        }

        step++;
        ShowStep();
    }

    public void Back()
    {
        if (completed || step == 0)
            return;

        step--;
        messageText.text = "";
        ShowStep();
    }

    public void Answer(bool correct)
    {
        if (step != 2 || completed)
            return;

        BeginIfNeeded();
        answerCorrect = correct;

        if (correct)
        {
            messageText.text = "Correct";
        }
        else
        {
            Reject("Incorrect. Try again.");
        }
    }

    public void SaveForm()
    {
        if (completed)
            return;

        if (WriteRecord("lab3-form.json", out string path))
        {
            messageText.text = "Form saved.";
            Debug.Log($"Form saved to {path}");
        }
    }

    private void Finish()
    {
        finishedSeconds =
            Time.realtimeSinceStartup - startedAt;

        completed = true;

        if (!WriteRecord("lab3-result.json", out string path))
        {
            completed = false;
            return;
        }

        step = 3;

        resultText.text =
            $"Finished inspection\n\n" +
            $"Operator: {operatorInput.text.Trim()}\n" +
            $"Temperature: {temperatureSlider.value:0} °C\n" +
            $"Errors: {errors}\n" +
            $"Time: {finishedSeconds:0.0} s\n\n" +
            "Result saved.";

        messageText.text = "";
        Debug.Log($"Result saved to {path}");

        ShowStep();
    }

    private bool WriteRecord(string fileName, out string path)
    {
        path = Path.Combine(
            Application.persistentDataPath, fileName);

        var record = new Record
        {
            operatorName = operatorInput.text.Trim(),
            inspected = inspectedToggle.isOn,
            temperature = temperatureSlider.value,
            answerCorrect = answerCorrect,
            completed = completed,
            errors = errors,
            durationSeconds = Elapsed(),
            savedAtUtc = DateTime.UtcNow.ToString("O")
        };

        try
        {
            Directory.CreateDirectory(
                Application.persistentDataPath);

            File.WriteAllText(
                path, JsonUtility.ToJson(record, true));

            return true;
        }
        catch (Exception exception)
        {
            messageText.text = "Failed saving file.";
            Debug.LogException(exception);
            return false;
        }
    }

    public void ResetAll()
    {
        step = 0;
        errors = 0;
        answerCorrect = false;
        started = false;
        completed = false;
        finishedSeconds = 0f;

        operatorInput.SetTextWithoutNotify("");
        inspectedToggle.SetIsOnWithoutNotify(false);
        temperatureSlider.SetValueWithoutNotify(22f);

        UpdateTemperature(22f);

        messageText.text = "";
        resultText.text = "";

        ShowStep();
    }
}