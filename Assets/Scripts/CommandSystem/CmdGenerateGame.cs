
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace T2G
{

    public class CmdGenerateGame : Command
    {
        public static readonly string CommandKey = "generate";

        private string _gameDescPath;
        public override bool Execute(params string[] args)
        {
            if(args.Length == 0)
            {
                OnExecutionCompleted?.Invoke(false,ConsoleController.eSender.Assistant, "Need a game description file to generate the game!");
                return false;
            }

            _gameDescPath = args[0];

            GenerateGameSepc(1);

            if (!File.Exists(_gameDescPath))
            {
                OnExecutionCompleted?.Invoke(false, 
                    ConsoleController.eSender.Error, 
                    $"Couldn't find the game description file {_gameDescPath}!");
                return false;
            }

            string gameDescText = File.ReadAllText(_gameDescPath);
            OnExecutionCompleted?.Invoke(true,
                    ConsoleController.eSender.Assistant,
                    $"Game description text {_gameDescPath} was loaded sucessfully! \nStart game generation ...");
            
            GameGenerationManager.Instance.StartGeneratingGameFromGameDesc(gameDescText);

            return true;
        }

        public override string GetKey()
        {
            return CommandKey.ToLower();
        }

        public override string[] GetArguments()
        {
            return new string[] { _gameDescPath };
        }

        void GenerateGameSepc(int index)
        {
            GameDescLite gameDesc = new GameDescLite();
            gameDesc.Engine = "Unity";

            switch (index)
            {
                case 0:
                    {
                        gameDesc.ProjectName = "SwatShooter";
                        gameDesc.ProjectPath = "C:/MyGames";
                        gameDesc.Title = "SwatShooter";
                        gameDesc.Genre = "Third-person shooter";
                        gameDesc.Spaces = new SpaceDescLite[1];
                        gameDesc.Spaces[0] = new SpaceDescLite();
                        gameDesc.Spaces[0].Name = "TrainingGround";
                        gameDesc.Spaces[0].Objects = new SpaceObject[6];
                        gameDesc.Spaces[0].Objects[0] = new SpaceObject();
                        gameDesc.Spaces[0].Objects[0].Desc = "SunLight";
                        gameDesc.Spaces[0].Objects[0].Name = "Sun";
                        gameDesc.Spaces[0].Objects[0].Properties = new string[1];
                        gameDesc.Spaces[0].Objects[0].Properties[0] = "rotation=(120, 0, 0)";
                        gameDesc.Spaces[0].Objects[1] = new SpaceObject();
                        gameDesc.Spaces[0].Objects[1].Desc = "Plain ground";
                        gameDesc.Spaces[0].Objects[1].Name = "Ground";
                        gameDesc.Spaces[0].Objects[2] = new SpaceObject();
                        gameDesc.Spaces[0].Objects[2].Desc = "Player swat shooter";
                        gameDesc.Spaces[0].Objects[2].Name = "Player";
                        gameDesc.Spaces[0].Objects[2].Properties = new string[2];
                        gameDesc.Spaces[0].Objects[2].Properties[0] = "position=(0, 0, 0)";
                        gameDesc.Spaces[0].Objects[2].Properties[1] = "rotation=(0, -60, 0)";
                        gameDesc.Spaces[0].Objects[3] = new SpaceObject();
                        gameDesc.Spaces[0].Objects[3].Desc = "third-person camera";
                        gameDesc.Spaces[0].Objects[3].Name = "Player Camera";
                        gameDesc.Spaces[0].Objects[3].Properties = new string[2];
                        gameDesc.Spaces[0].Objects[3].Properties[0] = "position=(0, 2, -3)";
                        gameDesc.Spaces[0].Objects[3].Properties[1] = "rotation=(10, 0, 0)";
                        gameDesc.Spaces[0].Objects[4] = new SpaceObject();
                        gameDesc.Spaces[0].Objects[4].Desc = "M4 rifle";
                        gameDesc.Spaces[0].Objects[4].Name = "M4-Rifle";
                        gameDesc.Spaces[0].Objects[4].Properties = new string[2];
                        gameDesc.Spaces[0].Objects[4].Properties[0] = "position=(-2, 1, 3)";
                        gameDesc.Spaces[0].Objects[4].Properties[1] = "rotation=(180, 0, 0)";
                        gameDesc.Spaces[0].Objects[5] = new SpaceObject();
                        gameDesc.Spaces[0].Objects[5].Desc = "G36 gun";
                        gameDesc.Spaces[0].Objects[5].Properties = new string[1];
                        gameDesc.Spaces[0].Objects[5].Properties[0] = "position=(2, 1, 3)";
                        string json = JsonUtility.ToJson(gameDesc, true);
                        File.WriteAllText(_gameDescPath, json);
                    }
                    break;
                case 1:
                    {
                        gameDesc.ProjectName = "SwatHero";
                        gameDesc.ProjectPath = "C:/MyGames";
                        gameDesc.Title = "SwatHero";
                        gameDesc.Genre = "Third-person shooter";
                        gameDesc.Spaces = new SpaceDescLite[1];
                        gameDesc.Spaces[0] = new SpaceDescLite();
                        gameDesc.Spaces[0].Name = "Millitary Island";
                        gameDesc.Spaces[0].Objects = new SpaceObject[8];
                        gameDesc.Spaces[0].Objects[0] = new SpaceObject();
                        gameDesc.Spaces[0].Objects[0].Desc = "Sky";
                        gameDesc.Spaces[0].Objects[0].Name = "Sky";
                        gameDesc.Spaces[0].Objects[0].SetValues = new SetValuePair[1];
                        gameDesc.Spaces[0].Objects[0].SetValues[0] = new SetValuePair() { Field = "CloudDensity", Values = "0.8" };
                        gameDesc.Spaces[0].Objects[1] = new SpaceObject();
                        gameDesc.Spaces[0].Objects[1].Desc = "millitary based island";
                        gameDesc.Spaces[0].Objects[1].Name = "Island Base";
                        gameDesc.Spaces[0].Objects[2] = new SpaceObject();
                        gameDesc.Spaces[0].Objects[2].Desc = "water plane";
                        gameDesc.Spaces[0].Objects[2].Name = "Ocean";
                        gameDesc.Spaces[0].Objects[3] = new SpaceObject();
                        gameDesc.Spaces[0].Objects[3].Desc = "Player swat shooter";
                        gameDesc.Spaces[0].Objects[3].Name = "Player";
                        gameDesc.Spaces[0].Objects[3].Properties = new string[1];
                        gameDesc.Spaces[0].Objects[3].Properties[0] = "spawnpoint=SpawnPoint1,SpawnPoint2";
                        gameDesc.Spaces[0].Objects[4] = new SpaceObject();
                        gameDesc.Spaces[0].Objects[4].Desc = "third-person camera";
                        gameDesc.Spaces[0].Objects[4].Name = "Player Camera";
                        gameDesc.Spaces[0].Objects[4].Properties = new string[2];
                        gameDesc.Spaces[0].Objects[4].Properties[0] = "position=(0, 2, -3)";
                        gameDesc.Spaces[0].Objects[4].Properties[1] = "rotation=(10, 0, 0)";
                        gameDesc.Spaces[0].Objects[4].SetValues = new SetValuePair[1];
                        gameDesc.Spaces[0].Objects[4].SetValues[0] = new SetValuePair() { Field = "TargetName", Values = "Player" };
                        gameDesc.Spaces[0].Objects[5] = new SpaceObject();
                        gameDesc.Spaces[0].Objects[5].Desc = "M4 rifle";
                        gameDesc.Spaces[0].Objects[5].Name = "M4-Rifle";
                        gameDesc.Spaces[0].Objects[5].Properties = new string[2];
                        gameDesc.Spaces[0].Objects[5].Properties[0] = "position=(-1.88, 1.5, 55)";
                        gameDesc.Spaces[0].Objects[5].Properties[1] = "rotation=(180, 0, 0)";
                        gameDesc.Spaces[0].Objects[6] = new SpaceObject();
                        gameDesc.Spaces[0].Objects[6].Desc = "G36 rifle";
                        gameDesc.Spaces[0].Objects[6].Name = "G36-Rifle";
                        gameDesc.Spaces[0].Objects[6].Properties = new string[2];
                        gameDesc.Spaces[0].Objects[6].Properties[0] = "position=(-27, 1.5, -105)";
                        gameDesc.Spaces[0].Objects[6].Properties[1] = "rotation=(180, 0, 0)";
                        gameDesc.Spaces[0].Objects[7] = new SpaceObject();
                        gameDesc.Spaces[0].Objects[7].Desc = "Simple UI";
                        gameDesc.Spaces[0].Objects[7].Name = "MainMenu";
                        string json = JsonUtility.ToJson(gameDesc, true);
                        File.WriteAllText(_gameDescPath, json);
                    }
                    break;
                case 2:
                    break;
                default:
                    break;
            }
        }
    }
}
