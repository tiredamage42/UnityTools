

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UnityTools.DevConsole 
{
    public static class Parser
    {
        public static async Task<object> Run(string input)
        {
            //if input starts with id flag
            //remove the id flag and store it separately
            string id = null;
            if (StartsWithInstanceKey(input))
            {
                id = input.Substring(0, input.IndexOf(' '));                
                input = input.Replace(id + " ", "");
            }

            foreach (Command command in Command.allCommands)
            {
                //check
                if (input.StartsWith(command.Name))
                {
                    List<string> parameters = GetParameters(input.Replace(command.Name, ""));

                    if (command.Matches(parameters, out object[] converted))
                    {
                        bool useLinked;
                        
                        object instance = FindInstance(command, id, out useLinked);
                        if (instance == null && !command.isStatic)
                        {
                            //this was an instance method that didnt have an id
                            return new NullReferenceException("Couldn't find instance with ID " + id);
                        }

                        //try to exec
                        try {
                            object result = command.Invoke(instance, useLinked, converted);
                            if (result is Task)
                            {
                                Task task = result as Task;
                                await task.ConfigureAwait(false);
                                return task.GetType().GetProperty("Result").GetValue(task);
                            }
                            else
                                return result;
                        }
                        catch (Exception exception) {
                            UnityEngine.Debug.LogError(exception);
                            return null;//exception;
                        }
                    }
                }
            }
            return "Command not found '" + input + "'";
        }

        public static bool StartsWithInstanceKey (string input) {
            return input.StartsWith("!") || input.StartsWith("@") || input.StartsWith("#");
        }

        public static object FindInstance(Command command, string id, out bool useLinked)
        {
            useLinked = false;

            if (id == null) return null;
            if (command.isStatic) return null;

            // get object by name (first returned)
            if (id.StartsWith("!") ) {
                UnityEngine.Object[] objs = UnityEngine.Object.FindObjectsOfType(command.declaringType);
                id = id.Substring(1);
                for (int i = 0; i < objs.Length; i++) {
                    if (objs[i].name == id) {
                        return objs[i];
                    }
                }
            }
            
            // get dynamic object by alias / key
            else if (id.StartsWith("@") || id.StartsWith("#")) {
                object obj;
                ObjectLoadState state; 

                state = DynamicObjectManager.GetObjectFromKey(id, out obj);
                
                switch (state) {
                    case ObjectLoadState.Loaded:
                        if (command.declaringType == typeof(DynamicObject)) 
                            return obj;
                        else
                            return ((DynamicObject)obj).GetComponent(command.declaringType);
                        
                    case ObjectLoadState.Unloaded:
                        useLinked = true;
                        if (command.linkedType == typeof(ObjectState)) 
                            return obj;
                        else
                            return ((ObjectState)obj).GetComponent(command.linkedType);
                        
                }
            }    
            return null;
        }

        static List<string> GetParameters(string input)
        {
            List<string> parameters = Regex.Matches(input, @"[\""].+?[\""]|[^ ]+").Cast<Match>().Select(x => x.Value).ToList();
            for (int i = 0; i < parameters.Count; i++)
            {
                if (parameters[i].StartsWith("\"") && parameters[i].EndsWith("\""))
                    parameters[i] = parameters[i].TrimStart('\"').TrimEnd('\"');
            }
            return parameters;
        }
    }
}
