using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ConnectionInputUI : MonoBehaviour
{
    public GameObject panel;
    public TMP_InputField fluxInput;
    public TMP_InputField capacityInput;
    public Button confirmButton;
    public Button cancelButton;

    private Action<int, int> onSubmit;

    private void Awake()
    {
        confirmButton.onClick.AddListener(Submit);
        cancelButton.onClick.AddListener(Cancel);
    }

    public void Show(Action<int, int> callback)
    {
        panel.SetActive(true);
        onSubmit = callback;
    }

    private void Submit()
    {
        int flux = int.TryParse(fluxInput.text, out int f) ? f : 0;
        int capacity = int.TryParse(capacityInput.text, out int c) ? c : 0;
        if (flux > capacity)
        {
            Debug.LogWarning("Flux cannot be greater than Capacity!");
            return;
        }
        if(flux<0 || capacity<0)
        {
            Debug.LogWarning("Flux or capacity cannot be negative!");
            return;
        }
        fluxInput.text = "";
        capacityInput.text = "";

        panel.SetActive(false);

        onSubmit?.Invoke(flux, capacity);
    }

    private void Cancel()
    {
        panel.SetActive(false);
    }
}