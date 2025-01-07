using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class HelpPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _helpText;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        //load document to display here
        _helpText.text = string.Empty;
    }
}
