using NUnit.Framework;
using System;
using UnityEngine;


/// <summary>
/// 계정에관한 모든 정보 
[Serializable]
public class GameData
{
    public int UserLevel = 1;
    public string UserName = "Player";

    public int Gold = 0;
    public int Dia = 0;


}

[Serializable]
public class ContinueData
{

}

public class GameManager
{
    public GameData _gameData = new GameData();
    


    public void Init()
    {

    }
}



