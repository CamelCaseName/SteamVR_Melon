//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Valve.VR
{
    [System.Serializable]
    public class SteamVRInputActionFile
    {
        public List<SteamVRInputActionFileAction> actions = new();
        public List<SteamVRInputActionFileActionSet> action_sets = new();
        public List<SteamVRInputActionFileDefaultBinding> default_bindings = new();
        public List<Dictionary<string, string>> localization = new();

        public string filePath;

        public List<SteamVRInputActionFileLocalizationItem> localizationHelperList = new();

        public void InitializeHelperLists()
        {
            foreach (var actionset in action_sets)
            {
                actionset.actionsInList = new List<SteamVRInputActionFileAction>(actions.Where(action => action.path.StartsWith(actionset.name) && SteamVRInputActionFileActionTypes.listIn.Contains(action.type)));
                actionset.actionsOutList = new List<SteamVRInputActionFileAction>(actions.Where(action => action.path.StartsWith(actionset.name) && SteamVRInputActionFileActionTypes.listOut.Contains(action.type)));
                actionset.actionsList = new List<SteamVRInputActionFileAction>(actions.Where(action => action.path.StartsWith(actionset.name)));
            }

            foreach (var item in localization)
            {
                localizationHelperList.Add(new SteamVRInputActionFileLocalizationItem(item));
            }
        }

        public void SaveHelperLists()
        {
            //fix actions list
            foreach (var actionset in action_sets)
            {
                actionset.actionsList.Clear();
                actionset.actionsList.AddRange(actionset.actionsInList);
                actionset.actionsList.AddRange(actionset.actionsOutList);
            }

            actions.Clear();

            foreach (var actionset in action_sets)
            {
                actions.AddRange(actionset.actionsInList);
                actions.AddRange(actionset.actionsOutList);
            }

            localization.Clear();
            foreach (var item in localizationHelperList)
            {
                Dictionary<string, string> localizationItem = new();
                localizationItem.Add(SteamVRInputActionFileLocalizationItem.languageTagKeyName, item.language);

                foreach (var itemItem in item.items)
                {
                    localizationItem.Add(itemItem.Key, itemItem.Value);
                }

                localization.Add(localizationItem);
            }
        }

        public static string GetShortName(string name)
        {
            string fullName = name;
            int lastSlash = fullName.LastIndexOf('/');
            if (lastSlash != -1)
            {
                if (lastSlash == fullName.Length - 1)
                {
                    fullName = fullName.Remove(lastSlash);
                    lastSlash = fullName.LastIndexOf('/');
                    if (lastSlash == -1)
                    {
                        return GetCodeFriendlyName(fullName);
                    }
                }
                return GetCodeFriendlyName(fullName.Substring(lastSlash + 1));
            }

            return GetCodeFriendlyName(fullName);
        }

        public static string GetCodeFriendlyName(string name)
        {
            name = name.Replace('/', '_').Replace(' ', '_');

            if (char.IsLetter(name[0]) == false)
            {
                name = "_" + name;
            }

            for (int charIndex = 0; charIndex < name.Length; charIndex++)
            {
                if (char.IsLetterOrDigit(name[charIndex]) == false && name[charIndex] != '_')
                {
                    name = name.Remove(charIndex, 1);
                    name = name.Insert(charIndex, "_");
                }
            }

            return name;
        }

        public string[] GetFilesToCopy(bool throwErrors = false)
        {
            List<string> files = new();

            FileInfo actionFileInfo = new(this.filePath);
            string path = actionFileInfo.Directory.FullName;

            files.Add(this.filePath);

            foreach (var binding in default_bindings)
            {
                string bindingPath = Path.Combine(path, binding.binding_url);

                if (File.Exists(bindingPath))
                {
                    files.Add(bindingPath);
                }
                else
                {
                    if (throwErrors)
                    {
                        MelonLoader.MelonLogger.Error("[HPVR] Could not bind binding file specified by the actions.json manifest: " + bindingPath);
                    }
                }
            }

            return files.ToArray();
        }

        public void CopyFilesToPath(string toPath, bool overwrite)
        {
            string[] files = SteamVRInput.actionFile.GetFilesToCopy();

            foreach (string file in files)
            {
                FileInfo bindingInfo = new(file);
                string newFilePath = Path.Combine(toPath, bindingInfo.Name);

                bool exists = false;
                if (File.Exists(newFilePath))
                {
                    exists = true;
                }

                if (exists)
                {
                    if (overwrite)
                    {
                        FileInfo existingFile = new(newFilePath)
                        {
                            IsReadOnly = false
                        };
                        existingFile.Delete();

                        File.Copy(file, newFilePath);

                        RemoveAppKey(newFilePath);

                        MelonLoader.MelonLogger.Msg("[HPVR] Copied (overwrote) SteamVR Input file at path: " + newFilePath);
                    }
                    else
                    {
                        MelonLoader.MelonLogger.Msg("[HPVR] Skipped writing existing file at path: " + newFilePath);
                    }
                }
                else
                {
                    File.Copy(file, newFilePath);

                    RemoveAppKey(newFilePath);

                    MelonLoader.MelonLogger.Msg("[HPVR] Copied SteamVR Input file to folder: " + newFilePath);
                }

            }
        }

        private const string findString_appKeyStart = "\"app_key\"";
        private const string findString_appKeyEnd = "\",";
        private static void RemoveAppKey(string newFilePath)
        {
            if (File.Exists(newFilePath))
            {
                string jsonText = System.IO.File.ReadAllText(newFilePath);

                string findString = "\"app_key\"";
                int stringStart = jsonText.IndexOf(findString);

                if (stringStart == -1)
                {
                    return; //no app key
                }

                int stringEnd = jsonText.IndexOf("\",", stringStart);

                if (stringEnd == -1)
                {
                    return; //no end?
                }

                stringEnd += findString_appKeyEnd.Length;

                int stringLength = stringEnd - stringStart;

                string newJsonText = jsonText.Remove(stringStart, stringLength);

                FileInfo file = new(newFilePath)
                {
                    IsReadOnly = false
                };

                File.WriteAllText(newFilePath, newJsonText);
            }
        }
        public static SteamVRInputActionFile Open(string path)
        {
            if (File.Exists(path))
            {
                string jsonText = File.ReadAllText(path);

                SteamVRInputActionFile actionFile = JsonConvert.DeserializeObject<SteamVRInputActionFile>(jsonText);
                actionFile.filePath = path;
                actionFile.InitializeHelperLists();

                return actionFile;
            }

            return null;
        }

        public void Save(string path)
        {
            FileInfo existingActionsFile = new(path);
            if (existingActionsFile.Exists)
            {
                existingActionsFile.IsReadOnly = false;
            }

            //SanitizeActionFile(); //todo: shouldn't we be doing this?

            string json = JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            File.WriteAllText(path, json);
        }
    }

    public enum SteamVRInputActionFileDefaultBindingControllerTypes
    {
        vive, //hmd
        vivePro, //hmd
        viveController,
        generic,
        holographicController,
        oculusTouch,
        gamepad,
        knuckles,
        indexHmd, //hmd
        viveCosmosController,
        rift, //hmd
        viveTrackerCamera,
        viveTracker,
    }

    [System.Serializable]
    public class SteamVRInputActionFileDefaultBinding
    {
        public string controller_type;
        public string binding_url;

        public SteamVRInputActionFileDefaultBinding GetCopy()
        {
            SteamVRInputActionFileDefaultBinding newDefaultBinding = new()
            {
                controller_type = this.controller_type,
                binding_url = this.binding_url
            };
            return newDefaultBinding;
        }
    }

    [System.Serializable]
    public class SteamVRInputActionFileActionSet
    {
        [JsonIgnore]
        private const string actionSetInstancePrefix = "instance_";

        public string name;
        public string usage;

        [JsonIgnore]
        public string codeFriendlyName
        {
            get
            {
                return SteamVRInputActionFile.GetCodeFriendlyName(name);
            }
        }

        [JsonIgnore]
        public string shortName
        {
            get
            {
                int lastIndex = name.LastIndexOf('/');
                if (lastIndex == name.Length - 1)
                {
                    return string.Empty;
                }

                return SteamVRInputActionFile.GetShortName(name);
            }
        }

        public void SetNewShortName(string newShortName)
        {
            name = GetPathFromName(newShortName);
        }

        public static string CreateNewName()
        {
            return GetPathFromName("NewSet");
        }

        private const string nameTemplate = "/actions/{0}";
        public static string GetPathFromName(string name)
        {
            return string.Format(nameTemplate, name);
        }

        public static SteamVRInputActionFileActionSet CreateNew()
        {
            return new SteamVRInputActionFileActionSet() { name = CreateNewName() };
        }

        public SteamVRInputActionFileActionSet GetCopy()
        {
            SteamVRInputActionFileActionSet newSet = new()
            {
                name = this.name,
                usage = this.usage
            };
            return newSet;
        }

        public override bool Equals(object obj)
        {
            if (obj is SteamVRInputActionFileActionSet set)
            {
                if (set == this)
                {
                    return true;
                }

                if (set.name == this.name)
                {
                    return true;
                }

                return false;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [JsonIgnore]
        public List<SteamVRInputActionFileAction> actionsInList = new();

        [JsonIgnore]
        public List<SteamVRInputActionFileAction> actionsOutList = new();

        [JsonIgnore]
        public List<SteamVRInputActionFileAction> actionsList = new();
    }

    public enum SteamVRInputActionFileActionRequirements
    {
        optional,
        suggested,
        mandatory,
    }

    [System.Serializable]
    public class SteamVRInputActionFileAction
    {
        [JsonIgnore]
        private static string[] _requirementValues;
        [JsonIgnore]
        public static string[] requirementValues
        {
            get
            {
                if (_requirementValues == null)
                {
                    _requirementValues = System.Enum.GetNames(typeof(SteamVRInputActionFileActionRequirements));
                }

                return _requirementValues;
            }
        }

        public string name;
        public string type;
        public string scope;
        public string skeleton;
        public string requirement;

        public SteamVRInputActionFileAction GetCopy()
        {
            SteamVRInputActionFileAction newAction = new()
            {
                name = this.name,
                type = this.type,
                scope = this.scope,
                skeleton = this.skeleton,
                requirement = this.requirement
            };
            return newAction;
        }

        [JsonIgnore]
        public SteamVRInputActionFileActionRequirements requirementEnum
        {
            get
            {
                for (int index = 0; index < requirementValues.Length; index++)
                {
                    if (string.Equals(requirementValues[index], requirement, System.StringComparison.CurrentCultureIgnoreCase))
                    {
                        return (SteamVRInputActionFileActionRequirements)index;
                    }
                }

                return SteamVRInputActionFileActionRequirements.suggested;
            }
            set
            {
                requirement = value.ToString();
            }
        }

        [JsonIgnore]
        public string codeFriendlyName
        {
            get
            {
                return SteamVRInputActionFile.GetCodeFriendlyName(name);
            }
        }

        [JsonIgnore]
        public string shortName
        {
            get
            {
                return SteamVRInputActionFile.GetShortName(name);
            }
        }

        [JsonIgnore]
        public string path
        {
            get
            {
                int lastIndex = name.LastIndexOf('/');
                if (lastIndex != -1 && lastIndex + 1 < name.Length)
                {
                    return name.Substring(0, lastIndex + 1);
                }

                return name;
            }
        }

        private const string nameTemplate = "/actions/{0}/{1}/{2}";
        public static string CreateNewName(string actionSet, string direction)
        {
            return string.Format(nameTemplate, actionSet, direction, "NewAction");
        }
        public static string CreateNewName(string actionSet, SteamVRActionDirections direction, string actionName)
        {
            return string.Format(nameTemplate, actionSet, direction.ToString().ToLower(), actionName);
        }

        public static SteamVRInputActionFileAction CreateNew(string actionSet, SteamVRActionDirections direction, string actionType)
        {
            return new SteamVRInputActionFileAction() { name = CreateNewName(actionSet, direction.ToString().ToLower()), type = actionType };
        }

        [JsonIgnore]
        public SteamVRActionDirections direction
        {
            get
            {
                if (type.ToLower() == SteamVRInputActionFileActionTypes.vibration)
                {
                    return SteamVRActionDirections.Out;
                }

                return SteamVRActionDirections.In;
            }
        }

        protected const string prefix = "/actions/";

        [JsonIgnore]
        public string actionSet
        {
            get
            {
                int setEnd = name.IndexOf('/', prefix.Length);
                if (setEnd == -1)
                {
                    return string.Empty;
                }

                return name.Substring(0, setEnd);
            }
        }

        public void SetNewActionSet(string newSetName)
        {
            name = string.Format(nameTemplate, newSetName, direction.ToString().ToLower(), shortName);
        }

        public override string ToString()
        {
            return shortName;
        }

        public override bool Equals(object obj)
        {
            if (obj is SteamVRInputActionFileAction action)
            {
                if (this == obj)
                {
                    return true;
                }

                if (this.name == action.name && this.type == action.type && this.skeleton == action.skeleton && this.requirement == action.requirement)
                {
                    return true;
                }

                return false;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class SteamVRInputActionFileLocalizationItem
    {
        public const string languageTagKeyName = "language_tag";

        public string language;
        public Dictionary<string, string> items = new();

        public SteamVRInputActionFileLocalizationItem(string newLanguage)
        {
            language = newLanguage;
        }

        public SteamVRInputActionFileLocalizationItem(Dictionary<string, string> dictionary)
        {
            if (dictionary == null)
            {
                return;
            }

            if (dictionary.ContainsKey(languageTagKeyName))
            {
                language = (string)dictionary[languageTagKeyName];
            }
            else
            {
                MelonLoader.MelonLogger.Msg("[HPVR] Input: Error in actions file, no language_tag in localization array item.");
            }

            foreach (KeyValuePair<string, string> item in dictionary)
            {
                if (item.Key != languageTagKeyName)
                {
                    items.Add(item.Key, (string)item.Value);
                }
            }
        }
    }

    public class SteamVRInputManifestFile
    {
        public string source;
        public List<SteamVRInputManifestFileApplication> applications;
    }

    public class SteamVRInputManifestFileApplication
    {
        public string app_key;
        public string launch_type;
        public string url;
        public string binary_path_windows;
        public string binary_path_linux;
        public string binary_path_osx;
        public string action_manifest_path;
        //public List<SteamVR_Input_ManifestFile_Application_Binding> bindings = new List<SteamVR_Input_ManifestFile_Application_Binding>();
        public string image_path;
        public Dictionary<string, SteamVRInputManifestFileApplicationString> strings = new();
    }

    public class SteamVRInputUnityAssemblyFileDefinition
    {
        public string name = "SteamVR_Actions";
        public string[] references = new string[] { "SteamVR" };
        public string[] optionalUnityReferences = new string[0];
        public string[] includePlatforms = new string[0];
        public string[] excludePlatforms = new string[] { "Android" };
        public bool allowUnsafeCode = false;
        public bool overrideReferences = false;
        public string[] precompiledReferences = new string[0];
        public bool autoReferenced = false;
        public string[] defineConstraints = new string[0];
    }

    public class SteamVRInputManifestFileApplicationString
    {
        public string name;
    }

    public class SteamVRInputManifestFileApplicationBinding
    {
        public string controller_type;
        public string binding_url;
    }

    public class SteamVRInputManifestFileApplicationBindingControllerTypes
    {
        public static string oculus_touch = "oculus_touch";
        public static string vive_controller = "vive_controller";
        public static string knuckles = "knuckles";
        public static string holographic_controller = "holographic_controller";
        public static string vive = "vive";
        public static string vive_pro = "vive_pro";
        public static string holographic_hmd = "holographic_hmd";
        public static string rift = "rift";
        public static string vive_tracker_camera = "vive_tracker_camera";
        public static string vive_cosmos = "vive_cosmos";
        public static string vive_cosmos_controller = "vive_cosmos_controller";
        public static string index_hmd = "index_hmd";
    }

    static public class SteamVRInputActionFileActionTypes
    {
        public static string boolean = "boolean";
        public static string vector1 = "vector1";
        public static string vector2 = "vector2";
        public static string vector3 = "vector3";
        public static string vibration = "vibration";
        public static string pose = "pose";
        public static string skeleton = "skeleton";

        public static string skeletonLeftPath = "\\skeleton\\hand\\left";
        public static string skeletonRightPath = "\\skeleton\\hand\\right";

        public static string[] listAll = new string[] { boolean, vector1, vector2, vector3, vibration, pose, skeleton };
        public static string[] listIn = new string[] { boolean, vector1, vector2, vector3, pose, skeleton };
        public static string[] listOut = new string[] { vibration };
        public static string[] listSkeletons = new string[] { skeletonLeftPath, skeletonRightPath };
    }

    static public class SteamVRInputActionFileActionSetUsages
    {
        public static string leftright = "leftright";
        public static string single = "single";
        public static string hidden = "hidden";

        public static string leftrightDescription = "per hand";
        public static string singleDescription = "mirrored";
        public static string hiddenDescription = "hidden";

        public static string[] listValues = new string[] { leftright, single, hidden };
        public static string[] listDescriptions = new string[] { leftrightDescription, singleDescription, hiddenDescription };
    }
}