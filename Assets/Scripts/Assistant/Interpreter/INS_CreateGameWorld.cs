using SimpleJSON;

public partial class Interpreter
{
    public static bool INS_CreateGameWorld(JSONObject gameWorld, ref string worldName)
    {
        worldName = gameWorld.GetValueOrDefault(Defs.k_GameDesc_NameKey, string.Empty).ToString();
        var isBootstrap = gameWorld.GetValueOrDefault("IsBootstrap", false).AsBool;
        var gravity = gameWorld.GetValueOrDefault("Gravity", -9.8f).AsFloat;
        if (IsNotEmptyString(worldName))
        {
            _instructions.Add($"CREATE_WORLD {worldName} -BOOTSTRAP {isBootstrap} -GRAVITY {gravity}");
            worldName = worldName.Trim('"');
            return true;
        }
        return false;
    }
}
