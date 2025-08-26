using UnityEngine;
using UnityEditor;

//-------------------------------------------------------
// PlayerTeam and PlayerPosition enums
//-------------------------------------------------------
// Purpose:
//   Enumerations describing team affiliation and seating positions
//   around the table. Used across `GameStage` and `Player` logic.
//-------------------------------------------------------
public enum PlayerTeam
{
   Team1,
   Team2
}

public enum PlayerPosition
{
   South,
   West,
   North,
   East
}
