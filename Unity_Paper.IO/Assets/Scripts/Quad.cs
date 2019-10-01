using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Quad : MonoBehaviour
{
    [HideInInspector]
    public enum Status { None, Claimed, Owned, Checked }
    public enum Player { None = 0, Player1 = 1, Player2 = 2, Player3 = 3 }
    public Status enumStatus;
    public Player enumPlayer;
    private Color statusColor;

    public void SetStatus(Status _status, Player _player)
    {
        enumStatus = _status;
        enumPlayer = _player;

        switch (enumStatus)
        {
            case Status.None:
                {
                    
                    statusColor = Color.white;
                    break;
                }
            case Status.Claimed:
                {
                    if (enumPlayer == Player.Player1)
                        statusColor = new Color(1f, 0.8f, 0.8f);
                    else
                        statusColor = new Color(0.8f, 0.8f, 1f);

                    break;
                }
            case Status.Owned:
                {
                    if (enumPlayer == Player.Player1)
                        statusColor = new Color(0.4f, 0, 0);
                    else
                        statusColor = new Color(0, 0, 0.4f);
                    break;
                }
            case Status.Checked:
                {
                    statusColor = Color.cyan;
                    break;
                }
        }
        GetComponent<Renderer>().material.color = statusColor;
    }
    
    public void SetToRed(float waitTime)
    {
        if(this != null)
           StartCoroutine(MarkAsCurrent(waitTime));
    }

    IEnumerator MarkAsCurrent(float waitTime)
    {
        if (enumPlayer == Player.Player1)
        {
            GetComponent<Renderer>().material.color = Color.red;
        }
        else
        {
            GetComponent<Renderer>().material.color = Color.blue;
        }
        
        yield return new WaitForSeconds(waitTime);
        GetComponent<Renderer>().material.color = statusColor;
    }

    public void ResetQuad()
    {
        enumStatus = Status.None;
        enumPlayer = Player.None;
        GetComponent<Renderer>().material.color = Color.white;
    }
}
