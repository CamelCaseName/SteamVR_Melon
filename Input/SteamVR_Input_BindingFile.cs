//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System.Collections.Generic;

using System.Linq;

namespace Valve.VR
{
    [System.Serializable]
    public class SteamVRInputBindingFile
    {
        public string app_key;
        public Dictionary<string, SteamVRInputBindingFileActionList> bindings = new();
        public string controller_type;
        public string description;
        public string name;
    }

    [System.Serializable]
    public class SteamVRInputBindingFileActionList
    {
        public List<SteamVRInputBindingFileChord> chords = new();
        public List<SteamVRInputBindingFilePose> poses = new();
        public List<SteamVRInputBindingFileHaptic> haptics = new();
        public List<SteamVRInputBindingFileSource> sources = new();
        public List<SteamVRInputBindingFileSkeleton> skeleton = new();
    }

    [System.Serializable]
    public class SteamVRInputBindingFileChord
    {
        public string output;
        public List<List<string>> inputs = new();

        public override bool Equals(object obj)
        {
            if (obj is SteamVRInputBindingFileChord chord)
            {
                if (this.output == chord.output && this.inputs != null && chord.inputs != null)
                {
                    if (this.inputs.Count == chord.inputs.Count)
                    {
                        for (int thisIndex = 0; thisIndex < this.inputs.Count; thisIndex++)
                        {
                            if (this.inputs[thisIndex] != null && chord.inputs[thisIndex] != null && this.inputs[thisIndex].Count == chord.inputs[thisIndex].Count)
                            {
                                for (int thisSubIndex = 0; thisSubIndex < this.inputs[thisIndex].Count; thisSubIndex++)
                                {
                                    if (this.inputs[thisIndex][thisSubIndex] != chord.inputs[thisIndex][thisSubIndex])
                                    {
                                        return false;
                                    }
                                }
                                return true;
                            }
                        }
                    }
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

    [System.Serializable]
    public class SteamVRInputBindingFilePose
    {
        public string output;
        public string path;

        public override bool Equals(object obj)
        {
            if (obj is SteamVRInputBindingFilePose pose)
            {
                if (pose.output == this.output && pose.path == this.path)
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

    [System.Serializable]
    public class SteamVRInputBindingFileHaptic
    {
        public string output;
        public string path;

        public override bool Equals(object obj)
        {
            if (obj is SteamVRInputBindingFileHaptic pose)
            {
                if (pose.output == this.output && pose.path == this.path)
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

    [System.Serializable]
    public class SteamVRInputBindingFileSkeleton
    {
        public string output;
        public string path;

        public override bool Equals(object obj)
        {
            if (obj is SteamVRInputBindingFileSkeleton pose)
            {
                if (pose.output == this.output && pose.path == this.path)
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

    [System.Serializable]
    public class SteamVRInputBindingFileSource
    {
        public string path;
        public string mode;
        public SteamVRInputBindingFileSourceInputStringDictionary parameters = new();
        public SteamVRInputBindingFileSourceInput inputs = new();

        protected const string outputKeyName = "output";

        public string GetOutput()
        {
            foreach (var input in inputs)
            {
                foreach (var entry in input.Value)
                {
                    if (entry.Key == outputKeyName)
                    {
                        return entry.Value;
                    }
                }
            }

            return null;
        }

        public override bool Equals(object obj)
        {
            if (obj is SteamVRInputBindingFileSource pose)
            {
                if (pose.mode == this.mode && pose.path == this.path)
                {
                    bool parametersEqual = false;
                    if (parameters != null && pose.parameters != null)
                    {
                        if (this.parameters.Equals(pose.parameters))
                        {
                            parametersEqual = true;
                        }
                    }
                    else if (parameters == null && pose.parameters == null)
                    {
                        parametersEqual = true;
                    }

                    if (parametersEqual)
                    {
                        bool inputsEqual = false;
                        if (inputs != null && pose.inputs != null)
                        {
                            if (this.inputs.Equals(pose.inputs))
                            {
                                inputsEqual = true;
                            }
                        }
                        else if (inputs == null && pose.inputs == null)
                        {
                            inputsEqual = true;
                        }

                        return inputsEqual;
                    }
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

    [System.Serializable]
    public class SteamVRInputBindingFileSourceInput : Dictionary<string, SteamVRInputBindingFileSourceInputStringDictionary>
    {
        public override bool Equals(object obj)
        {
            if (obj is SteamVRInputBindingFileSourceInput sourceInput)
            {
                if (this == sourceInput)
                {
                    return true;
                }
                else
                {
                    if (this.Count == sourceInput.Count)
                    {
                        foreach (var element in this)
                        {
                            if (sourceInput.ContainsKey(element.Key) == false)
                            {
                                return false;
                            }

                            if (this[element.Key].Equals(sourceInput[element.Key]) == false)
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                }
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [System.Serializable]
    public class SteamVRInputBindingFileSourceInputStringDictionary : Dictionary<string, string>
    {
        public override bool Equals(object obj)
        {
            if (obj is SteamVRInputBindingFileSourceInputStringDictionary stringDictionary)
            {
                if (this == stringDictionary)
                {
                    return true;
                }

                return (this.Count == stringDictionary.Count && !this.Except(stringDictionary).Any());
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}