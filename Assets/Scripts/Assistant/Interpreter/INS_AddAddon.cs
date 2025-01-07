using SimpleJSON;
using System.Text;

public partial class Interpreter
{
    public static bool INS_AddAddon(JSONObject jsonObj, string worldName, string objectName)
    {
        string addonType = jsonObj.GetValueOrDefault(Defs.k_GameDesc_AddonTypeKey, string.Empty);
        if(string.IsNullOrEmpty(addonType))
        {
            return false;
        }
        StringBuilder sb = new StringBuilder($"ADDON -WORLD \"{worldName}\" -OBJECT \"{objectName}\" -TYPE \"{addonType}\"");
        if (addonType.CompareTo("Perspective Camera") == 0)
        {
            var fieldOfView = jsonObj.GetValueOrDefault("FieldOfView", string.Empty).ToString().Trim('"');
            var nearClipPlane = jsonObj.GetValueOrDefault("NearClipPlane", string.Empty).ToString().Trim('"');
            var farClipPlane = jsonObj.GetValueOrDefault("FarClipPlane", string.Empty).ToString().Trim('"');
            var viewportRect = jsonObj.GetValueOrDefault("ViewportRect", string.Empty).ToString().Trim('"');

            if (!string.IsNullOrEmpty(fieldOfView))
            {
                sb.Append($" -FOV {fieldOfView}");
            }
            if (!string.IsNullOrEmpty(nearClipPlane))
            {
                sb.Append($" -NEAR {nearClipPlane}");
            }
            if (!string.IsNullOrEmpty(farClipPlane))
            {
                sb.Append($" -FAR {farClipPlane}");
            }
            if (!string.IsNullOrEmpty(viewportRect))
            {
                sb.Append($" -VIEWPORT_RECT {viewportRect}");
            }
        }
        else if (addonType.CompareTo("Script") == 0)
        {
            var script = jsonObj.GetValueOrDefault("Script", string.Empty).ToString().Trim('"');
            var dependencies = jsonObj.GetValueOrDefault("Dependencies", string.Empty).ToString().Trim('"');
            if(!string.IsNullOrEmpty(dependencies))
            {
                sb.Append($" -DEPENDENCIES \"{dependencies}\"");
            }

            if (script.CompareTo("ThirdPersonCameraController.cs") == 0 ||
                script.CompareTo("TopDownCameraController.cs") == 0)
            {
                var offset = jsonObj.GetValueOrDefault("ViewOffset", string.Empty).ToString().Trim('"');
                var target = jsonObj.GetValueOrDefault("TargetName", string.Empty).ToString().Trim('"');

                if (!string.IsNullOrEmpty(script))
                {
                    sb.Append($" -SCRIPT \"{script}\"");
                }

                string settings = string.Empty;
                if (!string.IsNullOrEmpty(offset))
                {
                    settings = $"ViewOffset={offset}";
                }
                if (!string.IsNullOrEmpty(target))
                {
                    if (string.IsNullOrEmpty(settings))
                    {
                        settings = $"TargetName={target}";
                    }
                    else
                    {
                        settings += $";TargetName={target}";
                    }
                }

                if (!string.IsNullOrEmpty(settings))
                {
                    sb.Append($" -SETTINGS \"{settings}\"");
                }
            }
            else if (script.CompareTo("FirstPersonCameraController.cs") == 0)
            {
                var viewOffset = jsonObj.GetValueOrDefault("ViewOffset", "[0, 0, 0]").ToString().Trim('"');
                if (!string.IsNullOrEmpty(viewOffset))
                {
                    sb.Append($" -VIEW_OFFSET {viewOffset}");
                }
                if (!string.IsNullOrEmpty(script))
                {
                    sb.Append($" -SCRIPT {script}");
                }
            }
        }
        else if (addonType.CompareTo("Directional Light") == 0)
        {
            var color = jsonObj.GetValueOrDefault("Color", "[1, 1, 1]").ToString().Trim('"');
            var intensity = jsonObj.GetValueOrDefault("Intensity", "1").ToString().Trim('"');
            sb.Append($" -COLOR {color}");
            sb.Append($" -INTENSITY {intensity}");
        }
        else if (addonType.CompareTo("Primitive") == 0)
        {
            var primitiveType = jsonObj.GetValueOrDefault("PrimitiveType", "sphere").ToString();
            var sizeScale = jsonObj.GetValueOrDefault("SizeScale", "1").ToString().Trim('"');
            sb.Append($" -PRIMITIVE_TYPE {primitiveType}");
            sb.Append($" -SIZE_SCALE {sizeScale}");
        }

        _instructions.Add(sb.ToString());
        return true;            
    }
}
