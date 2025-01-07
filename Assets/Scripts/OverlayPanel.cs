using UnityEngine;
using TMPro;
using System.Collections;

public class OverlayPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _Inprogress;
    int _index;

    string[] _inProgressStrings = {
        "In progress .",
        "In progress ..",
        "In progress ...",
        "In progress ....",
        "In progress .....",
        "In progress ......",
    };

    WaitForSeconds _wait = new WaitForSeconds(0.5f);

    private void OnEnable()
    {
        _index = _inProgressStrings.Length - 1;
        _Inprogress.text = _inProgressStrings[_index];
        StartCoroutine(AnimateInProgress());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator AnimateInProgress()
    {
        while(true)
        {
            yield return _wait;

            ++_index;
            if(_index == _inProgressStrings.Length)
            {
                _index = 0;
            }
            _Inprogress.text = _inProgressStrings[_index];
        }
    }
}
