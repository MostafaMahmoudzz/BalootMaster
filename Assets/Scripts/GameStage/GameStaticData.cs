using System;
using System.Collections.Generic;
using UnityEngine;
using Pebble;

//----------------------------------------------
// GameStaticData
//----------------------------------------------
// Purpose:
//   Centralized singleton for global static data shared by the game.
//   Currently empty, but acts as an extension point to expose shared
//   references (materials, audio, configs) at runtime.
//----------------------------------------------
public class GameStaticData : Singleton<GameStaticData>
{

}