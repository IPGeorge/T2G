
using System.IO;
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

            //GameDescLite gameDesc = new GameDescLite();
            //gameDesc.Engine = "Unity";
            //gameDesc.ProjectName = "SwatShooter";
            //gameDesc.ProjectPath = "C:/MyGames";
            //gameDesc.Title = "SwatShooter";
            //gameDesc.Genre = "Third-person shooter";
            //gameDesc.Spaces = new SpaceDescLite[1];
            //gameDesc.Spaces[0] = new SpaceDescLite();
            //gameDesc.Spaces[0].SpaceName = "TrainingGround";
            //gameDesc.Spaces[0].Objects = new SpaceObject[5];
            //gameDesc.Spaces[0].Objects[0] = new SpaceObject();
            //gameDesc.Spaces[0].Objects[0].Desc = "Sunlight";
            //gameDesc.Spaces[0].Objects[1] = new SpaceObject();
            //gameDesc.Spaces[0].Objects[1].Desc = "Plain ground";
            //gameDesc.Spaces[0].Objects[2] = new SpaceObject();
            //gameDesc.Spaces[0].Objects[2].Desc = "third-person camera";
            //gameDesc.Spaces[0].Objects[3] = new SpaceObject();
            //gameDesc.Spaces[0].Objects[3].Desc = "Player swat shooter";
            //gameDesc.Spaces[0].Objects[4] = new SpaceObject();
            //gameDesc.Spaces[0].Objects[4].Desc = "M4 rifle";
            //string json = JsonUtility.ToJson(gameDesc, true);
            //File.WriteAllText(_gameDescPath, json);
            //Debug.LogError($"---> Game Desc wrote to {_gameDescPath}");

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

    }
}
