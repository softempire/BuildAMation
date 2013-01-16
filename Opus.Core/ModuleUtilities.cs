﻿// <copyright file="ModuleUtilities.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public static class ModuleUtilities
    {
        public static IToolset GetToolsetForModule(System.Type moduleType)
        {
            System.Type type = moduleType;
            while (type != null)
            {
                // get the type of the tool associated with the module
                var t = type.GetCustomAttributes(typeof(ModuleToolAssignmentAttribute), false);
                if (0 == t.Length)
                {
                    type = type.BaseType;
                    continue;
                }

                if (t.Length > 1)
                {
                    throw new Exception("There are {0} tool assignments to the module type '{1}'. There should be only one.", t.Length, moduleType.ToString());
                }

                ModuleToolAssignmentAttribute attr = t[0] as ModuleToolAssignmentAttribute;
                System.Type toolType = attr.ToolType;
                if (null == toolType)
                {
                    // module does not require a toolchain
                    return null;
                }

                var providers = toolType.GetCustomAttributes(typeof(AssignToolsetProviderAttribute), false);
                if (0 == providers.Length)
                {
                    return null;
                }

                string toolsetName = (providers[0] as AssignToolsetProviderAttribute).ToolsetName(toolType);
                if (!Opus.Core.State.Has("Toolset", toolsetName))
                {
                    throw new Exception("Toolset '{0}' not registered", toolsetName);
                }

                IToolset toolset = Opus.Core.State.Get("Toolset", toolsetName) as IToolset;
                Opus.Core.Log.DebugMessage("Toolset for tool type '{0}' is '{1}'", toolType.ToString(), toolset.ToString());
                return toolset;
            }

            throw new Exception("Unable to locate toolchain for module '{0}'", moduleType.ToString());
        }

        private static TypeArray GetFieldsWithAttributeType<T>(IModule module, Target target) where T : class, ITargetFilters
        {
            System.Type type = module.GetType();
            System.Reflection.FieldInfo[] fieldInfoArray = type.GetFields(System.Reflection.BindingFlags.NonPublic |
                                                                          System.Reflection.BindingFlags.Public |
                                                                          System.Reflection.BindingFlags.Instance |
                                                                          System.Reflection.BindingFlags.FlattenHierarchy);
            TypeArray dependentsList = new TypeArray();
            foreach (System.Reflection.FieldInfo fieldInfo in fieldInfoArray)
            {
                T[] attributes = fieldInfo.GetCustomAttributes(typeof(T), false) as T[];
                if (attributes.Length > 0)
                {
                    if (attributes.Length > 1)
                    {
                        throw new Exception("More than one attribute not supported on field '{0}'", fieldInfo.Name);
                    }

                    bool targetFiltersMatch = TargetUtilities.MatchFilters(target, attributes[0]);
                    if (targetFiltersMatch)
                    {
                        System.Type[] values = null;
                        var fieldValue = fieldInfo.GetValue(module);
                        if (fieldValue is Array<System.Type>)
                        {
                            values = (fieldValue as Array<System.Type>).ToArray();
                        }
                        else if (fieldValue is System.Type[])
                        {
                            values = fieldValue as System.Type[];
                        }
                        else
                        {
                            throw new Exception("{0} field in {1} is of type {2} but must be of type System.Type[], Opus.Core.TypeArray or Opus.Core.Array<System.Type>", typeof(T).ToString(), module.ToString(), fieldValue.GetType());
                        }

                        dependentsList.AddRange(values);
                    }
                }
            }

            if (0 == dependentsList.Count)
            {
                return null;
            }
            else
            {
                return dependentsList;
            }
        }

        public static TypeArray GetExternalDependents(IModule module, Target target)
        {
            return GetFieldsWithAttributeType<Core.DependentModulesAttribute>(module, target);
        }

        public static TypeArray GetRequiredDependents(IModule module, Target target)
        {
            return GetFieldsWithAttributeType<Core.RequiredModulesAttribute>(module, target);
        }

        private static System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.Dictionary<Core.BaseTarget, IModule>> typeBaseTargetToModuleDictionary = new System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.Dictionary<Core.BaseTarget, IModule>>();

        public static IModule GetModule(System.Type type, Core.BaseTarget baseTarget)
        {
            if (typeBaseTargetToModuleDictionary.ContainsKey(type) &&
                typeBaseTargetToModuleDictionary[type].ContainsKey(baseTarget))
            {
                return typeBaseTargetToModuleDictionary[type][baseTarget];
            }

            DependencyGraph graph = State.Get("System", "Graph") as DependencyGraph;
            if (null == graph)
            {
                throw new Exception("Dependency graph has not yet been constructed");
            }

            foreach (DependencyNode node in graph)
            {
                System.Type moduleType = node.Module.GetType();
                bool typeMatch = (moduleType == type);
                bool baseTargetMatch = ((BaseTarget)node.Target == baseTarget);
                if (typeMatch)
                {
                    if (baseTargetMatch)
                    {
                        if (!typeBaseTargetToModuleDictionary.ContainsKey(type))
                        {
                            typeBaseTargetToModuleDictionary.Add(type, new System.Collections.Generic.Dictionary<Core.BaseTarget, IModule>());
                        }
                        if (!typeBaseTargetToModuleDictionary[type].ContainsKey(baseTarget))
                        {
                            typeBaseTargetToModuleDictionary[type][baseTarget] = node.Module;
                        }
                        return node.Module;
                    }
                }
                else
                {
                    System.Type baseType = moduleType.BaseType;
                    while ((null != baseType) && !baseType.IsInterface)
                    {
                        if ((baseType == type) && baseTargetMatch)
                        {
                            if (!typeBaseTargetToModuleDictionary.ContainsKey(type))
                            {
                                typeBaseTargetToModuleDictionary.Add(type, new System.Collections.Generic.Dictionary<Core.BaseTarget, IModule>());
                            }
                            if (!typeBaseTargetToModuleDictionary[type].ContainsKey(baseTarget))
                            {
                                typeBaseTargetToModuleDictionary[type][baseTarget] = node.Module;
                            }
                            return node.Module;
                        }

                        baseType = baseType.BaseType;
                    }
                }
            }

            return null;
        }

        private static System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.Dictionary<Core.Target, IModule>> typeTargetToModuleDictionary = new System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.Dictionary<Core.Target, IModule>>();

        public static IModule GetModule(System.Type type, Core.Target target)
        {
            if (typeTargetToModuleDictionary.ContainsKey(type) &&
                typeTargetToModuleDictionary[type].ContainsKey(target))
            {
                return typeTargetToModuleDictionary[type][target];
            }

            DependencyGraph graph = State.Get("System", "Graph") as DependencyGraph;
            if (null == graph)
            {
                throw new Exception("Dependency graph has not yet been constructed");
            }

            foreach (DependencyNode node in graph)
            {
                System.Type moduleType = node.Module.GetType();
                bool typeMatch = (moduleType == type);
                bool targetMatch = (node.Target == target);
                if (typeMatch)
                {
                    if (targetMatch)
                    {
                        if (!typeTargetToModuleDictionary.ContainsKey(type))
                        {
                            typeTargetToModuleDictionary.Add(type, new System.Collections.Generic.Dictionary<Core.Target, IModule>());
                        }
                        if (!typeTargetToModuleDictionary[type].ContainsKey(target))
                        {
                            typeTargetToModuleDictionary[type][target] = node.Module;
                        }
                        return node.Module;
                    }
                }
                else
                {
                    System.Type baseType = moduleType.BaseType;
                    while ((null != baseType) && !baseType.IsInterface)
                    {
                        if ((baseType == type) && targetMatch)
                        {
                            if (!typeTargetToModuleDictionary.ContainsKey(type))
                            {
                                typeTargetToModuleDictionary.Add(type, new System.Collections.Generic.Dictionary<Core.Target, IModule>());
                            }
                            if (!typeTargetToModuleDictionary[type].ContainsKey(target))
                            {
                                typeTargetToModuleDictionary[type][target] = node.Module;
                            }
                            return node.Module;
                        }

                        baseType = baseType.BaseType;
                    }
                }
            }

            return null;
        }

        public static IModule GetModuleNoToolchain(System.Type type, Core.Target target)
        {
            // Intentionally NOT using typeTargetToModuleDictionary

            DependencyGraph graph = State.Get("System", "Graph") as DependencyGraph;
            if (null == graph)
            {
                throw new Exception("Dependency graph has not yet been constructed");
            }

            foreach (DependencyNode node in graph)
            {
                System.Type moduleType = node.Module.GetType();
                bool typeMatch = (moduleType == type);
                bool targetMatch = (BaseTarget)node.Target == (BaseTarget)target;
                if (typeMatch)
                {
                    if (targetMatch)
                    {
                        return node.Module;
                    }
                }
                else
                {
                    System.Type baseType = moduleType.BaseType;
                    while ((null != baseType) && !baseType.IsInterface)
                    {
                        if ((baseType == type) && targetMatch)
                        {
                            return node.Module;
                        }

                        baseType = baseType.BaseType;
                    }
                }
            }

            return null;
        }
    }
}
