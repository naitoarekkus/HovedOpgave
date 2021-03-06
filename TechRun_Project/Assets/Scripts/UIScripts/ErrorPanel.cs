﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Enumeration depicting the type of error that can occur
/// </summary>
public enum ErrorType {
    NONE,
    COULD_NOT_FIND_ROUTE,
    COULD_NOT_LOAD_MAP,
    COULD_NOT_LOAD_ROUTE,
    GPS_TIMED_OUT,
    GPS_INITIALIZATION_FAILED,
    GPS_INACTIVE}

/// <summary>
/// Class handling the error window show to the user when using the application
/// </summary>
public class ErrorPanel : MonoBehaviour {

    private static ErrorPanel instance;    
    private Text[] errorPanelTexts;
    private ErrorType error;

    /// <summary>
    /// Static instance of the ErrorPanel GameObject
    /// </summary>
    public static ErrorPanel Instance
    {
        get
        { 
            return instance;
        }
    }

    /// <summary>
    /// Initializes the errorpanel
    /// </summary>
    public void Initialize()
    {
        instance = this;
        errorPanelTexts = GetComponentsInChildren<Text>();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Show the Error Panel
    /// </summary>
    /// <param name="_subTitle">The subtitle for the error</param>
    /// <param name="_info">The information to the user</param>
    /// <param name="_type">The type of error</param>
    public void ShowError(string _subTitle, string _info, ErrorType _type)
    {
        UIController.Instance.ShowLoading(false); //Hides the loading panel if it's active
        errorPanelTexts[1].text = "Error - " + _subTitle;
        errorPanelTexts[2].text = _info;
        error = _type;

        gameObject.SetActive(true);
    }

    /// <summary>
    /// Hides the Error Panel
    /// </summary>
    private void HideError()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// OnClick Method for the Ok button
    /// </summary>
    public void OnOkClick()
    {
        //TODO: Handle error whatever it may be
        HandleError();
        ResetErrorText();
        error = ErrorType.NONE;
        HideError();
    }

    /// <summary>
    /// Resets the error tekst to standard
    /// </summary>
    private void ResetErrorText()
    {
        errorPanelTexts[0].text = "Sorry... something went wrong";
        errorPanelTexts[1].text = "Error - ";
        errorPanelTexts[2].text = "Error";
    }

    /// <summary>
    /// What happens when you click "OK" button on error panel
    /// Based on the error
    /// </summary>
    private void HandleError()
    {
        switch (error)
        {
            case ErrorType.COULD_NOT_FIND_ROUTE:
                UIController.Instance.pnlRouteButtons.gameObject.SetActive(true);
                break;
            case ErrorType.COULD_NOT_LOAD_MAP: 
                UIController.Instance.UnloadActiveScene();
                UIController.Instance.LoadScene("Main"); //reload main scene
                break;
            case ErrorType.COULD_NOT_LOAD_ROUTE:
                UIController.Instance.pnlRouteButtons.gameObject.SetActive(true);
                break;
            case ErrorType.GPS_INACTIVE:
                //Do nothing
                UIController.Instance.gameObject.GetComponent<InitGPS>().ReinitLocationService();
                break;
            case ErrorType.GPS_INITIALIZATION_FAILED:
                //TODO: To restarts to GSP, call whatever method is made so the GPS inits before map is loaded.
                UIController.Instance.gameObject.GetComponent<InitGPS>().ReinitLocationService();
                break;
            case ErrorType.GPS_TIMED_OUT:
                //TODO: To restarts to GSP, call whatever method is made so the GPS inits before map is loaded.
                UIController.Instance.gameObject.GetComponent<InitGPS>().ReinitLocationService();
                break;
            case ErrorType.NONE:
                //do nothing
                break;
        }
    }
}