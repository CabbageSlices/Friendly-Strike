﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//keeps track of the scores of each team, and indicates when a team has reached the required score to win
public static class TeamScoreManager {

    public delegate void ScoreChangeEvent(TeamProperties.Teams team, int newScore);
    public static event ScoreChangeEvent onTeamScoreChange;

    //score a team needs to win the game
    public static int requiredScoreToWin;

    //scores for each team
    [System.NonSerialized]
    private static Dictionary<TeamProperties.Teams, int> scoresForEachTeam = new Dictionary<TeamProperties.Teams, int>() {

        {TeamProperties.Teams.red, 0},
        {TeamProperties.Teams.green, 0},
        {TeamProperties.Teams.blue, 0},
        {TeamProperties.Teams.yellow, 0},
        {TeamProperties.Teams.black, 0},
        {TeamProperties.Teams.white, 0},
        {TeamProperties.Teams.cyan, 0},
        {TeamProperties.Teams.magenta, 0}
    };

    public static void resetScores() {

        foreach(TeamProperties.Teams team in scoresForEachTeam.Keys) {

            scoresForEachTeam[team] = 0;
        }
    }

    //increase the scor eof the given team
    public static void increaseScore(TeamProperties.Teams team, int amount = 1) {

        TeamScoreManager.scoresForEachTeam[team] += amount;

        if(onTeamScoreChange != null)
            onTeamScoreChange(team, TeamScoreManager.scoresForEachTeam[team]);
    }
}
