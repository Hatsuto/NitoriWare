﻿using UnityEngine;

public class MilkPourState : MonoBehaviour
{
    [SerializeField]
    private float _fillRate = 100;

    [SerializeField]
    private bool _failOnEarlyRelease;

    [SerializeField]
    private MilkPourCup _cup;
    private gameState _state;

    private enum gameState
    {
        Start,
        Filling,
        Idle,
        Stopped
    }
        
    void Start ()
    {
        _state = gameState.Start;
    }

    void Update ()
    {
        switch (_state)
        {
            case gameState.Stopped:
                break;
            case gameState.Start:
                _state = Input.GetKey (KeyCode.Space) ? gameState.Filling : gameState.Start;
                if (_state == gameState.Filling)
                    OnFill ();
                break;
            case gameState.Filling:
            case gameState.Idle:
                _state = Input.GetKey (KeyCode.Space) ? gameState.Filling : gameState.Idle;
                if (_state == gameState.Filling)
                    OnFill ();
                else
                    OnIdle ();
                break;
        }
    }

    void OnFill ()
    {
        _cup.AddFill (_fillRate * Time.deltaTime);
        if (_cup.IsFillMaxed ())
            Fail ();
    }

    void OnIdle ()
    {
        if (_cup.IsFillReqMet ())
            Win ();
        else if (_cup.IsOverfilled ())
            Fail ();
        else if (_failOnEarlyRelease)
            Fail ();
    }

    void Win ()
    {
        _cup.Stop ();
        MicrogameController.instance.setVictory(true, true);
        _state = gameState.Stopped;
    }

    void Fail ()
    {
        _cup.Stop ();
        MicrogameController.instance.setVictory(false, true);
        _state = gameState.Stopped;
    }
}
