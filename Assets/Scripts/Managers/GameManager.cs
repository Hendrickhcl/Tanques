﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int m_NumRoundsToWin = 5;
    public float m_StartDelay = 3f;
    public float m_EndDelay = 3f;
    public float m_MaxGameTime = 30; // ⏳ Tiempo límite de 4 minutos
    public Text m_TimerText; // Referencia al texto del temporizador
    public CameraControl m_CameraControl;
    public Text m_MessageText;
    public GameObject m_TankPrefab;
    public TankManager[] m_Tanks;

    private int m_RoundNumber;
    private WaitForSeconds m_StartWait;
    private WaitForSeconds m_EndWait;
    private TankManager m_RoundWinner;
    private TankManager m_GameWinner;

    private float m_GameStartTime;
    private float m_GameEndTime;

    private void Start()
    {
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);

        SpawnAllTanks();
        SetCameraTargets();

        m_GameStartTime = Time.time; 

        StartCoroutine(GameLoop());
    }

    private void SpawnAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].m_Instance =
                Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
            m_Tanks[i].m_PlayerNumber = i + 1;
            m_Tanks[i].Setup();
        }
    }

    private void SetCameraTargets()
    {
        Transform[] targets = new Transform[m_Tanks.Length];
        for (int i = 0; i < targets.Length; i++)
        {
            targets[i] = m_Tanks[i].m_Instance.transform;
        }
        m_CameraControl.m_Targets = targets;
    }

    private IEnumerator GameLoop()
    {
        yield return StartCoroutine(RoundStarting());
        yield return StartCoroutine(RoundPlaying());
        yield return StartCoroutine(RoundEnding());

        if (m_GameWinner != null)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            StartCoroutine(GameLoop());
        }
    }

    private IEnumerator RoundStarting()
    {
        ResetAllTanks();
        DisableTankControl();
        m_CameraControl.SetStartPositionAndSize();

        m_RoundNumber++;
        m_MessageText.text = "ROUND " + m_RoundNumber;

        yield return m_StartWait;
    }

    private IEnumerator RoundPlaying()
    {
        EnableTankControl();
        m_MessageText.text = string.Empty;

        while (!OneTankLeft())
        {
            float elapsed = Time.time - m_GameStartTime;
            float remaining = m_MaxGameTime - elapsed;

            int minutes = (int)(remaining / 60);
            int seconds = (int)(remaining % 60);

            m_TimerText.text = "TIEMPO RESTANTE: " + minutes.ToString("00") + ":" + seconds.ToString("00");

            if (elapsed >= m_MaxGameTime)
            {
                break; // ⏳ Tiempo agotado
            }

            yield return null;
        }
    }

    private IEnumerator RoundEnding()
    {
        DisableTankControl();
        m_RoundWinner = null;
        m_RoundWinner = GetRoundWinner();

        if (m_RoundWinner != null)
            m_RoundWinner.m_Wins++;

        m_GameWinner = GetGameWinner();

        // ⏳ Si se agotó el tiempo, ambos pierden
        if (Time.time - m_GameStartTime >= m_MaxGameTime && m_GameWinner == null)
        {
            m_MessageText.text = "¡TIEMPO AGOTADO!\nAMBOS JUGADORES PIERDEN";
            yield return m_EndWait;
            SceneManager.LoadScene(0);
            yield break;
        }

        // ⏱️ Guardar tiempo si alguien ganó
        if (m_GameWinner != null)
        {
            m_GameEndTime = Time.time;
        }

        string message = EndMessage();
        m_MessageText.text = message;
        m_TimerText.text = "";
        yield return m_EndWait;
    }

    private bool OneTankLeft()
    {
        int numTanksLeft = 0;
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf)
                numTanksLeft++;
        }
        return numTanksLeft <= 1;
    }

    private TankManager GetRoundWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf)
                return m_Tanks[i];
        }
        return null;
    }

    private TankManager GetGameWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                return m_Tanks[i];
        }
        return null;
    }

    private string EndMessage()
    {
        string message = "EMPATE!";
        if (m_RoundWinner != null)
            message = m_RoundWinner.m_ColoredPlayerText + " GANA LA RONDA!";

        message += "\n\n\n\n";

        for (int i = 0; i < m_Tanks.Length; i++)
        {
            message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " GANA\n";
        }

        if (m_GameWinner != null)
        {
            float totalTime = m_GameEndTime - m_GameStartTime;
            int minutes = (int)(totalTime / 60);
            int seconds = (int)(totalTime % 60);
            message = m_GameWinner.m_ColoredPlayerText + " GANA EL JUEGO!\n";
            message += "TIEMPO TOTAL: " + minutes + "m " + seconds + "s\n";
        }

        return message;
    }

    private void ResetAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].Reset();
        }
    }

    private void EnableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].EnableControl();
        }
    }

    private void DisableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].DisableControl();
        }
    }

}