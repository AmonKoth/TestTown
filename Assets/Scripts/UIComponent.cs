using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UIComponent : MonoBehaviour
{
    [Header("Menu")]
    [SerializeField]
    private GameObject _menu;

    private Text _timer;
    private double _time;
    private Text _collectedPoints;
    private Text _totalPoints;

    private int _points = 0;

    private bool _isEscapePressed = false;

    private PlayerController _player;

    public int GetPoints() => _points;

    public void OnEspacePressed(InputAction.CallbackContext context)
    {
        _isEscapePressed = !_isEscapePressed;
    }

    private void TextAssinger()
    {
        var texts = FindObjectsOfType<Text>();
        foreach (Text t in texts)
        {
            if (t.name == "Timer")
            {
                _timer = t;
            }
            if (t.name == "Count")
            {
                _collectedPoints = t;
            }
            if (t.name == "Total")
            {
                _totalPoints = t;
            }
        }
    }
    private void HandleTimer()
    {
        _time += Time.deltaTime;
        _timer.text = ((int)_time).ToString();
    }
    public void AddPoints()
    {
        _points++;
        _collectedPoints.text = _points.ToString();
    }

    private void HandlePause()
    {
        if (_isEscapePressed)
        {
            _menu.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _player.SetIsPause(true);
            Time.timeScale = 0;
        }
        else
        {
            _menu.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _player.SetIsPause(false);
            Time.timeScale = 1;
        }
    }
    private void Initilizer()
    {
        _points = 0;
        TextAssinger();
        _time = 0;
        _collectedPoints.text = _points.ToString();
        _player = this.GetComponent<PlayerController>();
        SpeedUp[] speedUpPowerups = FindObjectsOfType<SpeedUp>();
        CollectibleBase[] totalAmountOfPoints = FindObjectsOfType<CollectibleBase>();
        _totalPoints.text = (totalAmountOfPoints.Length - speedUpPowerups.Length).ToString();
    }
    // Start is called before the first frame update
    void Start()
    {
        Initilizer();
    }

    void Update()
    {
        HandleTimer();
        HandlePause();
    }
}
