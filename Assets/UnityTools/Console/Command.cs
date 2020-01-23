

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityTools.DevConsole 
{
    internal static class Extensions
    {
        public static CommandAttribute GetCommand(this MemberInfo member)
        {
            try
            {
                object[] attributes = member.GetCustomAttributes(typeof(CommandAttribute), false);
                for (int a = 0; a < attributes.Length; a++) {
                    if (attributes[a].GetType() == typeof(CommandAttribute))
                        return attributes[a] as CommandAttribute;
                }
            }
            catch
            {
                //this could happen due to a TypeLoadException in builds
            }

            return null;
        }
    }
    [Serializable] public class CommandAttribute : Attribute {
        public string name = "";
        public string description = "";
        public string category = "";
        public bool inGameOnly;
        public Type linkedType;
        public CommandAttribute(string name, string description, string category, bool inGameOnly, Type linkedType=null) {
            this.name = name;
            this.description = description;
            this.category = category;
            this.inGameOnly = inGameOnly;
            this.linkedType = linkedType;
        }
    }

    class _MemberInfo {
        public MethodInfo method;
        public FieldInfo field;
        public PropertyInfo property;
        public MethodInfo get, set;
        
        public _MemberInfo (MethodInfo method, FieldInfo field, PropertyInfo property) {
            this.method = method;
            this.field = field;
            this.property = property;

            if (property != null)
            {
                get = property.GetGetMethod(true);
                set = property.GetSetMethod(true);
            }
        }
    }
    class ParamInfo {
        public object defaultValue;
        public bool isOptional;
        public string name;
        public Type type;

        public ParamInfo (string name, Type type, bool isOptional, object defaultValue) {
            this.name = name;
            this.type = type;
            this.isOptional = isOptional;
            this.defaultValue = defaultValue;
        }
    }

    [Serializable] public class Command
    {
        static List<Command> _allCommands;
        public static List<Command> allCommands {
            get {
                if (_allCommands == null)
                    InitializeLibrary();
                return _allCommands;
            }
        }
        static Dictionary<string, List<Command>> _commandsLibrary = null;
        public static Dictionary<string, List<Command>> commandsLibrary {
            get {
                if (_commandsLibrary == null)
                    InitializeLibrary();
                return _commandsLibrary;
            }
        }

        static void AddCommand (Command c) {
            if (c == null)
                return;

            string category = c.Category;
            if (string.IsNullOrEmpty(category))
                category = "Misc.";
            
            if (_commandsLibrary.ContainsKey(category))
                _commandsLibrary[category].Add(c);
            else
                _commandsLibrary[category] = new List<Command>() { c };
            
            _allCommands.Add(c);
        }

        static void InitializeLibrary () {
            _commandsLibrary = new Dictionary<string, List<Command>>();
            _allCommands = new List<Command>();

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes()) {
                foreach (var m in type.GetMethods(flags))       AddCommand (Command.Create(m));
                foreach (var p in type.GetProperties(flags))    AddCommand (Command.Create(p));   
                foreach (var f in type.GetFields(flags))        AddCommand (Command.Create(f));
            }
        }

        _MemberInfo mainInfo, linkedInfo;
        CommandAttribute attribute;
        public bool hasLinkedType { get { return linkedType != null; } }
        public Type linkedType { get { return attribute.linkedType; } }
        public string Name { get { return attribute.name; } }
        public string Category { get { return attribute.category; } }
        public string Description { get { return attribute.description; } }
        public bool inGameOnly { get { return attribute.inGameOnly; } }
        
        List<ParamInfo> parameters = new List<ParamInfo>();
        public Type declaringType;
        public bool isStatic;
           

        public string GetHint () {
            string text = Name;
            foreach (var parameter in parameters) 
                text += " <" + parameter.name + " (" + parameter.type.Name + ")>";
            if (Description != "")
                text += " :: " + Description;
            if (!isStatic)
                text = "[I] " + text;
            return text;
        }

        
        Command(MethodInfo method, CommandAttribute attribute) {
            Initialize(attribute, method, method, null, null);
        }
        Command(FieldInfo field, CommandAttribute attribute) {
            Initialize(attribute, field, null, field, null);
        }
        Command(PropertyInfo property, CommandAttribute attribute) {
            Initialize(attribute, property, null, null, property);
        }

        private void Initialize(CommandAttribute attribute, MemberInfo info, MethodInfo method, FieldInfo field, PropertyInfo property)
        {
            this.declaringType = info.DeclaringType;
            this.attribute = attribute;

            mainInfo = new _MemberInfo (method, field, property);
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            if (linkedType != null) {
                if (method != null) 
                    linkedInfo = new _MemberInfo (linkedType.GetMethod(method.Name, flags), null, null);
                else if (field != null) 
                    linkedInfo = new _MemberInfo (null, linkedType.GetField(field.Name, flags), null);
                else if (property != null) 
                    linkedInfo = new _MemberInfo (null, null, linkedType.GetProperty(property.Name, flags));
            }

            if (method != null) isStatic = method.IsStatic;
            if (field != null) isStatic = field.IsStatic;
            if (property != null) isStatic = property.GetAccessors(true)[0].IsStatic;

            if (method != null) {
                //set parameters
                ParameterInfo[] ps = method.GetParameters();
                for (int i = 0; i < ps.Length; i++)
                    parameters.Add (new ParamInfo( ps[i].Name, ps[i].ParameterType, ps[i].IsOptional, ps[i].DefaultValue ));
            }
            else if (property != null) {
                if (mainInfo.set != null) 
                    parameters.Add (new ParamInfo( "value", property.PropertyType, false, null ));
            }
            else if (field != null) {
                parameters.Add (new ParamInfo( "value", field.FieldType, false, null ));
            }
        }

        public object Invoke(object owner, bool useLinked, params object[] parameters) {
            bool isInMainMenuScene = GameManager.isInMainMenuScene;
            if (inGameOnly && isInMainMenuScene) 
                return "Command: '" + Name + "' is only available during gameplay!";
            
            _MemberInfo info = !useLinked ? mainInfo : linkedInfo;
            if (info.method != null) {
                return info.method.Invoke(owner, parameters);
            }
            else if (info.property != null) {
                if (parameters == null || parameters.Length == 0) {
                    //if no parameters were passed, then get
                    if (info.get != null) 
                        return info.get.Invoke(owner, parameters);
                }
                else if (parameters != null && parameters.Length == 1) {
                    //if 1 parameter was passed, then set
                    if (info.set != null)
                        info.property.SetValue(owner, parameters[0]);
                }
            }
            else if (info.field != null) {
                if (parameters == null || parameters.Length == 0)
                    return info.field.GetValue(owner);
                else if (parameters != null && parameters.Length == 1)
                    info.field.SetValue(owner, parameters[0]);
            }
            return null;
        }

        public static Command Create(MethodInfo method) {
            CommandAttribute attribute = method.GetCommand();
            if (attribute == null) return null;
            return new Command(method, attribute);
        }
        public static Command Create(PropertyInfo property) {
            CommandAttribute attribute = property.GetCommand();
            if (attribute == null) return null;
            return new Command(property, attribute);
        }
        public static Command Create(FieldInfo field) {
            CommandAttribute attribute = field.GetCommand();
            if (attribute == null) return null;
            return new Command(field, attribute);
        }

        public bool Matches(List<string> parametersGiven, out object[] convertedParameters)
        {
            convertedParameters = null;

            //parameter amount mismatch
            if (mainInfo.method != null)
            {
                //get the total amount of params required
                int paramsRequired = 0;
                int optionalParams = 0;
                for (int i = 0; i < parameters.Count; i++)
                {
                    if (parameters[i] is ParamInfo param)
                    {
                        if (!param.isOptional)
                            paramsRequired++;
                        else
                            optionalParams++;
                    }
                }
                if (parametersGiven.Count < paramsRequired || parametersGiven.Count > paramsRequired + optionalParams)
                    return false;
                
                //try to infer the type from input parameters
                convertedParameters = new object[parameters.Count];
                for (int i = 0; i < parameters.Count; i++)
                {
                    ParamInfo param = parameters[i];
                    
                    object propValue = null;
                    if (i >= parametersGiven.Count)
                    {
                        propValue = param.defaultValue;
                    }
                    else
                    {
                        SystemTools.TryParse (parametersGiven[i], param.type, out propValue);
                        //couldnt get a value
                        if (propValue == null)
                            throw new FailedToConvertException("Failed to convert " + parametersGiven[i] + " to type " + param.type.Name);
                    }

                    convertedParameters[i] = propValue;
                }
            }
            else {

                //get the value
                if (parametersGiven.Count == 0)
                    return true;
                
                // trying to set a property without "Set"...
                if (parameters.Count == 0) 
                    return false;
                
                //try to infer the type from input parameters
                SystemTools.TryParse (parametersGiven[0], parameters[0].type, out object propValue);
                
                //couldnt get a value
                if (propValue == null)
                    throw new FailedToConvertException("Failed to convert " + parametersGiven[0] + " to type " + parameters[0].type.Name);

                convertedParameters = new object[] { propValue };
            }
            return true;
        }
    }
    public class FailedToConvertException : Exception {
        public FailedToConvertException(string message) : base(message) { }
    }
}
