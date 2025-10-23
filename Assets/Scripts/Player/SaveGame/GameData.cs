using System;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    // === INVENTARIO ===
    // guardo el inventario como una lista de strings (ej: "manzana", "pera")
    public List<string> inventory = new List<string>();

    // === CHECKPOINT ===
    // guardo nivel, subnivel y coordenadas exactas
    public int currentLevel;
    public int currentSubLevel;
    public Vector3 checkpointPos;

    // === VIDA ===
    public int maxHealth;
    public int currentHealth;

    // === HABILIDADES ===
    // toggles para habilidades desbloqueadas
    public bool hasDoubleJump;
    public bool hasSlowFall;
    public bool hasWallClimb;

    // === ESTADÍSTICAS DE PARTIDA ===
    public float playTime;         // tiempo total jugado en segundos
    public int enemiesDefeated;    // enemigos eliminados
    public int hitsTaken;          // cuántos golpes recibió el player
    public int deaths;             // número de veces que murió
    public int collectedItems;     // objetos recolectados
    public int lastScene;   // última escena cargada

    // === CONSTRUCTOR POR DEFECTO ===
    public GameData()
    {
        currentLevel = 1;
        currentSubLevel = 1;
        checkpointPos = new Vector3(480f, 500f, 0f);

        maxHealth = 100;
        currentHealth = 10;

        hasDoubleJump = false;
        hasSlowFall = false;
        hasWallClimb = false;

        playTime = 0f;
        enemiesDefeated = 0;
        hitsTaken = 0;
        deaths = 0;
        collectedItems = 0;
        lastScene = 1;
    }
}
