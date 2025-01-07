using UnityEngine;
using SimpleJSON;
using System.Collections.Generic;
using System;
using System.IO;
using T2G.UnityAdapter;

public class SampleGameDescLibrary
{

    public static string[] SampleGameDescNames =
    {
        "Load Sample ...",
        "Swat Hero",
        "Marine Swat "
    };

    public static bool GetSampleGameDesc(int sampleIndex, ref GameDesc gameDesc)
    {
        bool result = false;
        switch (sampleIndex)
        {
            case 1:
                result = GetSampleGameDesc1(ref gameDesc);
                break;
            case 2:
                result = GetSampleGameDesc2(ref gameDesc);
                break;
            default:
                break;
        }
        return result;
    }

    static bool GetSampleGameDesc1(ref GameDesc gameDesc)
    {
        gameDesc.Name = "Swat Hero";
        gameDesc.Title = "Swat Hero";
        gameDesc.Genre = "Third Person Shooter";
        gameDesc.ArtStyle = "Realistic";
        gameDesc.Developer = "George";
        gameDesc.GameStory = "Create a third-person shooter game. The player controls a marine swat to fight at a millitary zone on a secret island. " +
            "The player character carries an AR15 to attack and kill all enemies on the way to destroy the enemy's base building. This mission requires " +
            "the player to acomplish the task within 10 minutes.";

#region Project settings
        gameDesc.Project = new GameProject();
        gameDesc.Project.Path = "C:/MyGames";
        gameDesc.Project.Name = "MarineShooter";
        gameDesc.Project.Engine = "Unity";
#endregion Project settings

#region Game worlds
        gameDesc.GameWorlds = new GameWorld[1];

#region GameWorld 1
        gameDesc.GameWorlds[0] = new GameWorld();
        gameDesc.GameWorlds[0].Name = "IlandBattleField";
        gameDesc.GameWorlds[0].Objects = new WorldObject[7];

        //Camera
        gameDesc.GameWorlds[0].Objects[0] = new WorldObject();
        gameDesc.GameWorlds[0].Objects[0].Name = "MainCamera";
        gameDesc.GameWorlds[0].Objects[0].Position = new float[3] { 0.0f, 2.0f, 0.0f };
        gameDesc.GameWorlds[0].Objects[0].Addons = new Addon[2];
        PerspectiveCamera camera = new PerspectiveCamera();
        camera.NearClipPlane = 1.0f;
        camera.FarClipPlane = 10000.0f;
        camera.FieldOfView = 50.0f;
        gameDesc.GameWorlds[0].Objects[0].Addons[0] = camera;
        ThirdPersonCameraController cameraController = new ThirdPersonCameraController();
        cameraController.TargetName = "Player";
        gameDesc.GameWorlds[0].Objects[0].Addons[1] = cameraController;
        //light
        gameDesc.GameWorlds[0].Objects[1] = new WorldObject();
        gameDesc.GameWorlds[0].Objects[1].Rotation = new float[3] { 50.0f, -30.0f, 0.0f };
        gameDesc.GameWorlds[0].Objects[1].Name = "SunLight";
        gameDesc.GameWorlds[0].Objects[1].Addons = new Addon[1];
        DirectionalLight sunLight = new DirectionalLight();
        gameDesc.GameWorlds[0].Objects[1].Addons[0] = sunLight;
        //Ground
        gameDesc.GameWorlds[0].Objects[2] = new WorldObject();
        gameDesc.GameWorlds[0].Objects[2].Name = "Ground";
        gameDesc.GameWorlds[0].Objects[2].Addons = new Addon[1];
        Primitive ground = new Primitive();
        ground.PrimitiveType = "plane";
        ground.SizeScale = 30.0f;
        gameDesc.GameWorlds[0].Objects[2].Addons[0] = ground;
        //Play character
        gameDesc.GameWorlds[0].Objects[3] = new WorldObject();
        gameDesc.GameWorlds[0].Objects[3].Name = "Player";
        gameDesc.GameWorlds[0].Objects[3].Prefab = "PlayerSwat";
        //gameDesc.GameWorlds[0].Objects[3].Prefab = "PlayerSoldierToon";
        gameDesc.GameWorlds[0].Objects[3].Position = new float[3] { 0.0f, 0.0f, 10.0f };
        gameDesc.GameWorlds[0].Objects[3].Rotation = new float[3] { 0.0f, 0.0f, 0.0f };
        //Guns to pick up
        gameDesc.GameWorlds[0].Objects[4] = new WorldObject();
        gameDesc.GameWorlds[0].Objects[4].Name = "Gun_AK";
        gameDesc.GameWorlds[0].Objects[4].Prefab = "Gun_AK";
        gameDesc.GameWorlds[0].Objects[4].Position = new float[3] { -5.0f, 1.0f, 20.0f };
        gameDesc.GameWorlds[0].Objects[4].Rotation = new float[3] { 180.0f, 0.0f, 0.0f };
        gameDesc.GameWorlds[0].Objects[5] = new WorldObject();
        gameDesc.GameWorlds[0].Objects[5].Name = "Gun_G36";
        gameDesc.GameWorlds[0].Objects[5].Prefab = "Gun_G36";
        gameDesc.GameWorlds[0].Objects[5].Position = new float[3] { 5.0f, 1.0f, 20.0f };
        gameDesc.GameWorlds[0].Objects[5].Rotation = new float[3] { 180.0f, 0.0f, 0.0f };
        //UI
        gameDesc.GameWorlds[0].Objects[6] = new WorldObject();
        gameDesc.GameWorlds[0].Objects[6].Name = "SimpleUI";
        gameDesc.GameWorlds[0].Objects[6].Prefab = "SimpleUI";

#endregion Gameworld 1
#endregion Game worlds

        return true;
    }

    static bool GetSampleGameDesc2(ref GameDesc gameDesc)
    {
        gameDesc.Name = "Marine Swat";
        gameDesc.Title = "Marine Swat";
        gameDesc.Genre = "Third Person Shooter";
        gameDesc.ArtStyle = "Realistic";
        gameDesc.Developer = "George";
        gameDesc.GameStory = "Create a top down shooter game. The player controls a marine swat to fight at a millitary zone on a secret island. " +
            "The player character carries an AR15 to attack and kill all enemies on the way to destroy the enemy's base building. This mission requires " +
            "the player to acomplish the task within 10 minutes.";

        #region Project settings
        gameDesc.Project = new GameProject();
        gameDesc.Project.Path = "C:/MyGames";
        gameDesc.Project.Name = "MarineShooter";
        gameDesc.Project.Engine = "Unity";
        #endregion Project settings

        #region Game worlds
        gameDesc.GameWorlds = new GameWorld[2];

        #region GameWorld 1
        gameDesc.GameWorlds[0] = new GameWorld();
        gameDesc.GameWorlds[0].Name = "IlandBattleField";
        gameDesc.GameWorlds[0].Objects = new WorldObject[9];

        //Camera
        gameDesc.GameWorlds[0].Objects[0] = new WorldObject();
        gameDesc.GameWorlds[0].Objects[0].Name = "MainCamera";
        gameDesc.GameWorlds[0].Objects[0].Position = new float[3] { 0.0f, 2.0f, 0.0f };
        gameDesc.GameWorlds[0].Objects[0].Addons = new Addon[2];
        PerspectiveCamera camera = new PerspectiveCamera();
        camera.NearClipPlane = 1.0f;
        camera.FarClipPlane = 10000.0f;
        camera.FieldOfView = 50.0f;
        gameDesc.GameWorlds[0].Objects[0].Addons[0] = camera;
        TopDownCameraController cameraController = new TopDownCameraController();
        cameraController.TargetName = "Player";
        gameDesc.GameWorlds[0].Objects[0].Addons[1] = cameraController;
        //light
        gameDesc.GameWorlds[0].Objects[1] = new WorldObject();
        gameDesc.GameWorlds[0].Objects[1].Rotation = new float[3] { 50.0f, -30.0f, 0.0f };
        gameDesc.GameWorlds[0].Objects[1].Name = "SunLight";
        gameDesc.GameWorlds[0].Objects[1].Addons = new Addon[1];
        DirectionalLight sunLight = new DirectionalLight();
        gameDesc.GameWorlds[0].Objects[1].Addons[0] = sunLight;
        //iland
        gameDesc.GameWorlds[0].Objects[2] = new WorldObject();
        gameDesc.GameWorlds[0].Objects[2].Name = "Ground";
        gameDesc.GameWorlds[0].Objects[2].Addons = new Addon[1];
        Primitive ground = new Primitive();
        ground.PrimitiveType = "plane";
        ground.SizeScale = 30.0f;
        gameDesc.GameWorlds[0].Objects[2].Addons[0] = ground;
        //Play character
        gameDesc.GameWorlds[0].Objects[3] = new WorldObject();
        gameDesc.GameWorlds[0].Objects[3].Name = "Player";
        gameDesc.GameWorlds[0].Objects[3].Prefab = "PlayerSoldierToon";
        gameDesc.GameWorlds[0].Objects[3].Position = new float[3] { 0.0f, 0.0f, 10.0f };
        gameDesc.GameWorlds[0].Objects[3].Rotation = new float[3] { 0.0f, 0.0f, 0.0f };
        //Guns to pick up
        gameDesc.GameWorlds[0].Objects[4] = new WorldObject();
        gameDesc.GameWorlds[0].Objects[4].Name = "Gun_AK";
        gameDesc.GameWorlds[0].Objects[4].Prefab = "Gun_AK";
        gameDesc.GameWorlds[0].Objects[4].Position = new float[3] { -5.0f, 1.0f, 20.0f };
        gameDesc.GameWorlds[0].Objects[4].Rotation = new float[3] { 180.0f, 0.0f, 0.0f };
        gameDesc.GameWorlds[0].Objects[5] = new WorldObject();
        gameDesc.GameWorlds[0].Objects[5].Name = "Gun_G36";
        gameDesc.GameWorlds[0].Objects[5].Prefab = "Gun_G36";
        gameDesc.GameWorlds[0].Objects[5].Position = new float[3] { 5.0f, 1.0f, 20.0f };
        gameDesc.GameWorlds[0].Objects[5].Rotation = new float[3] { 180.0f, 0.0f, 0.0f };
        //Ocean
        gameDesc.GameWorlds[0].Objects[6] = new WorldObject();
        gameDesc.GameWorlds[0].Objects[6].Name = "Ocean";
        gameDesc.GameWorlds[0].Objects[6].Prefab = "SimpleOcean";
        gameDesc.GameWorlds[0].Objects[6].Position = new float[3] { 0.0f, 0.0f, 0.0f };

        gameDesc.GameWorlds[0].Objects[7] = new WorldObject();
        gameDesc.GameWorlds[0].Objects[7].Name = "SkyDome";
        gameDesc.GameWorlds[0].Objects[7].Prefab = "TimeOfDay";
        gameDesc.GameWorlds[0].Objects[7].Position = new float[3] { 0.0f, 0.0f, 0.0f };
        gameDesc.GameWorlds[0].Objects[7].Controller = "TimeOfDayController";
        gameDesc.GameWorlds[0].Objects[7].SetControllerValues = new string[] { 
            "Latitude", "30",
            "Month", "6",
            "Clouds", "3",
            "Fog", "false"
        };
        //UI
        gameDesc.GameWorlds[0].Objects[8] = new WorldObject();
        gameDesc.GameWorlds[0].Objects[8].Name = "SimpleUI";
        gameDesc.GameWorlds[0].Objects[8].Prefab = "SimpleUI";

        //Quest

        #endregion GameWorld 1

        #region GameWorld 2

        //gameDesc.GameWorlds[1] = new GameWorld();
        //gameDesc.GameWorlds[1].Name = "Main Menu";
        //gameDesc.GameWorlds[1].IsBootstrap = true;

        ////UI

        #endregion GameWorld 2

        #endregion Game worlds

        return true;
    }

}

public class GameDesc : System.Object
{
    public string Category = Defs.k_GameDescCategory;
    public string Name;
    public string Title = string.Empty;
    public string Genre = string.Empty;
    public string ArtStyle = string.Empty;
    public string Developer = string.Empty;
    public GameProject Project = new GameProject();
    public string GameStory = string.Empty;
    public string[] Packages = { "TextMeshProEssential.unitypackage" };
    public GameWorld[] GameWorlds = new GameWorld[1];

    public GameDesc()
    {
        GameWorlds[0] = new GameWorld();
    }

    public string GetProjectPathName()
    {
        return Path.Combine(Project.Path, Project.Name);
    }
}

public class GameProject
{
    public string Engine = "Unity";
    public string Path = "c:/MyGames";
    public string Name = "MyGame";
}

public class GameWorld
{
    public string Category = Defs.k_GameWorldCategory;
    public string Name;
    public WorldObject[] Objects;
    public bool IsBootstrap = false;
    public float Gravity = -9.8f;
}

public class WorldObject
{
    public string Category = Defs.k_WorldObjectCategory;
    public string Name = string.Empty;
    public float[] Position = new float[3] { 0.0f, 0.0f, 0.0f };
    public float[] Rotation = new float[3] { 0.0f, 0.0f, 0.0f };     //Euler angles
    public float[] Scale = new float[3] { 1.0f, 1.0f, 1.0f };
    public Addon[] Addons = null;
    public string Prefab = string.Empty;
    public string Controller = string.Empty;        //e.g., "PlayerController"
    public string[] SetControllerValues = null;     //e.g., "Health", 100, "Name", "Hero" 

    public string Packages = string.Empty;
}

public class Addon
{
    public string Category = Defs.k_ObjectAddonCategory;
    public string AddonType = Defs.k_GameDesc_AddonTypeKey;
}

public class ScriptAddon : Addon
{
    public string Script = string.Empty;
    public string Dependencies = string.Empty;
    public ScriptAddon() 
    {
        AddonType = "Script";
    }
}

public class ThirdPersonCameraController : ScriptAddon
{
    public float[] ViewOffset = new float[3] { 0.0f, 3.0f, 5.0f }; 
    public string TargetName = string.Empty;

    public ThirdPersonCameraController() : base()
    {
        Script = "ThirdPersonCameraController.cs";
        Dependencies = "PlayerCameraController.cs,ObjectController.cs";
    }
}

public class FirstPersonCameraController : ScriptAddon
{
    public float[] ViewOffset = new float[3] { 0.0f, 1.8f, 0.0f };

    public FirstPersonCameraController() : base()
    {
        Script = "FirstPersonCameraController.cs";
        Dependencies = "PlayerCameraController.cs,ObjectController.cs";
    }
}

public class TopDownCameraController : ScriptAddon
{
    public float[] ViewOffset = new float[3] { 0.0f, 1.8f, 0.0f };
    public string TargetName;

    public TopDownCameraController() : base()
    {
        Script = "TopDownCameraController.cs";
        Dependencies = "PlayerCameraController.cs,ObjectController.cs";
    }
}

public class PerspectiveCamera : Addon
{
    public string FOVAxis = "Vertical";
    public float FieldOfView = 60.0f;
    public float NearClipPlane = 0.1f;
    public float FarClipPlane = 1000.0f;
    public float[] ViewportRect = new float[4] { 0.0f, 0.0f, 1.0f, 1.0f };
    public PerspectiveCamera()
    {
        AddonType = "Perspective Camera";
    }
}

public class DirectionalLight : Addon
{
    public float[] Color = new float[3] { 0.9f, 0.9f, 0.9f };
    public float Intensity = 1.0f;
    public DirectionalLight()
    {
        AddonType = "Directional Light";
    }
}

public class Primitive : Addon
{
    public string PrimitiveType = "cube";  //cube, sphere, plane, quad
    public float SizeScale = 1.0f;
    public Primitive()
    {
        AddonType = "Primitive";
    }
}

public class UI
{
    public class HUD
    {

    }

    public class Menu
    {
        public string Name;
        public string[] MenuItems;
    }

    public class MenuItem
    {
        public string Name;
        public string Action;

    }
}

public class Quest
{
    public float Duration;
    public string Goal;
    public string SubGoals;
    public int Collected;
    public int Kills;
    public int Scores;
    public int Destroies;
    public string Destroied;
    public string WinCondition;
    public string LoseCondition;
}