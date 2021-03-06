﻿using UnityEngine;
using System.Collections;

/// <summary>
/// Script for the basketball, is a subclass of the Ball class.
/// </summary>
public class BasketBall : Ball
{
    /// <summary>
    /// Unity build in Start method
    /// </summary>
    protected override void Start()
    {
        gameObject.GetComponent<Renderer>().material = Resources.Load<Material>("Materials/balldimpled");

        base.Start(); //Calls the superclass version of the Start() method.
    }

    /// <summary>
    /// Unity build in Update method
    /// </summary>
    protected override void Update()
    {
        base.Update();
    }

    /// <summary>
    /// Method that runs whenever the object collides.
    /// </summary>
    /// <param name="_collision">Object that the object collides with.</param>
    private void OnCollisionEnter(Collision _collision)
    {
        AudioManager.Instance.PlayEffect("BallBounce");
    }
}
