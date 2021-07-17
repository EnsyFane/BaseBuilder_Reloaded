using Assets.Scripts.Models;
using MoonSharp.Interpreter;
using MoonSharp.VsCodeDebugger;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Infrastructure.LUAParsing
{
    public class FurnitureActions
    {
        private static FurnitureActions _instance;

        private readonly Script luaScript;

        private static MoonSharpVsCodeDebugServer server;

        public FurnitureActions(string rawLuaCode)
        {
            // Loads all the classes marked with [MoonSharpUserData] in the LUA interpreter.
            UserData.RegisterAssembly();

            _instance = this;
            luaScript = new Script();

            // Make the base types visible.
            // TODO: Add this for jobs and inventory later.
            luaScript.Globals["World"] = typeof(World);

            // To activate the remote debugger here.
            //ActivateRemoteDebugger(luaScript, "script name");

            luaScript.DoString(rawLuaCode);
        }

        /// <summary>
        /// Executes a list of given LUA functions.
        /// </summary>
        /// <remarks>
        /// If the function with the given name is not found it is skipped.
        /// The functions are called with <paramref name="furniture"/> and <paramref name="deltaTime"/>.
        /// </remarks>
        /// <param name="functionNames">An enumeration of function names to be called.</param>
        /// <param name="furniture">The furniture object to be called with.</param>
        /// <param name="deltaTime">Time from the last update tick.</param>
        public static void CallLUAUpdateFuntions(IEnumerable<string> functionNames, Furniture furniture, float deltaTime)
        {
            foreach (var functionName in functionNames)
            {
                var function = _instance.luaScript.Globals[functionName];

                if (function == null)
                {
                    Debug.LogError($"No LUA function with name {functionName} was found.");
                    continue;
                }

                var result = _instance.luaScript.Call(function, furniture, deltaTime);

                if (result.Type == DataType.String)
                {
                    Debug.Log(result.String);
                }
            }
        }

        /// <summary>
        /// Calls a function with the given name and given arguments
        /// </summary>
        /// <param name="functionName">Function name.</param>
        /// <param name="args">Arguments to pass to the function.</param>
        /// <returns>The result of the function execution.</returns>
        public static DynValue CallFunction(string functionName, params object[] args)
        {
            var function = _instance.luaScript.Globals[functionName];

            // FIXME: Add check for existence of function with given name?

            return _instance.luaScript.Call(function, args);
        }

        [Obsolete("Only use for debugging, should not be used in live code.")]
        private void ActivateRemoteDebugger(Script script, string scriptName)
        {
            if (server == null)
            {
                server = new MoonSharpVsCodeDebugServer();
            }
            server.Start();
            server.AttachToScript(script, scriptName);
        }
    }
}