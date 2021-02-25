using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using BepInEx.Logging;
using UnityEngine;
using System.Reflection.Emit;
using System.Reflection;
using BepInEx.Configuration;

namespace InfiniteFarming
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    [BepInProcess("Littlewood.exe")]
    public class InfiniteFarming : BaseUnityPlugin
    {
        public const string pluginGuid = "egnite.littlewood.infinitefarming";
        public const string pluginName = "Infinite Farming";
        public const string pluginVersion = "1.0";

        public static ManualLogSource LogSource;

        public static int maxSpawnedCrops;
        public static int maxSpawnedFruits;
        public static int maxSpawnedTrees;

        public void Awake()
        {
            LogSource = BepInEx.Logging.Logger.CreateLogSource("InfFarming");
            ConfigEntry<int> configMaxCrops = Config.Bind("InfiniteFarming",
                "MaxCrops",
                36,
                "Maximum number of crops that can be planted");
            ConfigEntry<int> configMaxFruitTrees = Config.Bind("InfiniteFarming",
                "MaxFruitTrees",
                36,
                "Maximum number of fruit trees that can be planted");
            ConfigEntry<int> configMaxTrees = Config.Bind("InfiniteFarming",
                "MaxTrees",
                30,
                "Maximum number of wood trees that can be planted");

            maxSpawnedCrops = configMaxCrops.Value;
            maxSpawnedFruits = configMaxFruitTrees.Value;
            maxSpawnedTrees = configMaxTrees.Value;

            LogSource.LogInfo("Plugin loaded");

            Harmony.CreateAndPatchAll(typeof(InfiniteFarming));
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(GameScript), "LoadMap")]
        public static IEnumerable<CodeInstruction> LoadMapTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            Type myType = typeof(GameScript);
            // Get the type and fields of FieldInfoClass.
            FieldInfo myFieldInfo = myType.GetField("spawnedCropObj", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo fruitFieldInfo = myType.GetField("spawnedFruitTreeObj", BindingFlags.NonPublic | BindingFlags.Instance);

            IEnumerable<CodeInstruction> inst = new CodeMatcher(instructions)
                .Start()
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, fruitFieldInfo))
                .Advance(1)
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, fruitFieldInfo))
                .MatchForward(false, new CodeMatch(OpCodes.Ldarg_0))
                .Advance(1)
                .SetOperandAndAdvance(maxSpawnedFruits)
                .Start()
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, myFieldInfo))
                .Advance(1)
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, myFieldInfo))
                .MatchForward(false, new CodeMatch(OpCodes.Ldarg_0))
                .Advance(1)
                .SetOperandAndAdvance(maxSpawnedCrops)
                .InstructionEnumeration();

            /*
            CodeMatcher codeMatcher = new CodeMatcher(inst).Start();
            while (codeMatcher.Pos < codeMatcher.Length)
            {
                LogSource.LogInfo($"{codeMatcher.Pos} : {codeMatcher.Instruction}");
                codeMatcher.Advance(1);
            }
            */
            return inst;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(GameScript), "SpawnObject")]
        public static IEnumerable<CodeInstruction> SpawnObjectTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            Type myType = typeof(GameScript);
            MethodInfo myMethod = myType.GetMethod("GetTotalTrees");

            IEnumerable<CodeInstruction> inst = new CodeMatcher(instructions)
                .Start()
                .MatchForward(false, new CodeMatch(OpCodes.Call, myMethod))
                .MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S))
                .SetOperandAndAdvance(maxSpawnedTrees)
                .InstructionEnumeration();


            return inst;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(GameScript), "GrowTrees")]
        public static IEnumerable<CodeInstruction> GrowTreesTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            Type myType = typeof(GameScript);
            MethodInfo myMethod = AccessTools.Method(typeof(GameScript), "GetTotalTrees", new Type[] { });

            IEnumerable<CodeInstruction> inst = new CodeMatcher(instructions)
                .End()
                .MatchBack(false, new CodeMatch(OpCodes.Ldc_I4_S))
                .SetOperandAndAdvance(maxSpawnedTrees)
                .InstructionEnumeration();


            return inst;
        }


        [HarmonyTranspiler]
        [HarmonyPatch(typeof(GameScript), "RefreshBuildCost")]
        public static IEnumerable<CodeInstruction> RefreshBuildCostTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            Type myType = typeof(GameScript);
            MethodInfo myMethod = AccessTools.Method(typeof(GameScript), "GetTotalTrees", new Type[] { });
            
            IEnumerable<CodeInstruction> inst = new CodeMatcher(instructions)
                .Start()
                .MatchForward(false, new CodeMatch(OpCodes.Call, myMethod))
                .MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S))
                .SetOperandAndAdvance(maxSpawnedTrees)
                .InstructionEnumeration();
            /*
            CodeMatcher codeMatcher = new CodeMatcher(inst).Start();
            while (codeMatcher.Pos < codeMatcher.Length)
            {
                LogSource.LogInfo($"{codeMatcher.Pos} : {codeMatcher.Instruction}");
                codeMatcher.Advance(1);
            }
            */
            return inst;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(GameScript), "GetTotalCrops")]
        public static IEnumerable<CodeInstruction> GetTotalCropsTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            Type myType = typeof(GameScript);
            FieldInfo myFieldInfo = myType.GetField("spawnedCropObj", BindingFlags.NonPublic | BindingFlags.Instance);
            IEnumerable<CodeInstruction> inst = new CodeMatcher(instructions)
                .Start()
                .End()
                .MatchBack(false, new CodeMatch(OpCodes.Ldloc_2))
                .Advance(1)
                .Set(OpCodes.Ldc_I4_S, maxSpawnedCrops)
                .InstructionEnumeration();
            /*
            foreach(CodeInstruction instruction in inst)
            {
                LogSource.LogInfo(instruction);
            }
            */
            return inst;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(GameScript), "GetTotalFruitTrees")]
        public static IEnumerable<CodeInstruction> GetTotalFruitTreesTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            Type myType = typeof(GameScript);
            FieldInfo myFieldInfo = myType.GetField("spawnedFruitTreeObj", BindingFlags.NonPublic | BindingFlags.Instance);
            IEnumerable<CodeInstruction> inst = new CodeMatcher(instructions)
                .Start()
                .End()
                .MatchBack(false, new CodeMatch(OpCodes.Ldloc_2))
                .Advance(1)
                .Set(OpCodes.Ldc_I4_S, maxSpawnedFruits)
                .InstructionEnumeration();
            /*
            foreach(CodeInstruction instruction in inst)
            {
                LogSource.LogInfo(instruction);
            }
            */
            return inst;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(GameScript), "GetTotalTrees")]
        public static IEnumerable<CodeInstruction> GetTotalTreesTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            Type myType = typeof(GameScript);
            FieldInfo myFieldInfo = myType.GetField("spawnedTreeObj", BindingFlags.NonPublic | BindingFlags.Instance);
            IEnumerable<CodeInstruction> inst = new CodeMatcher(instructions)
                .Start()
                .End()
                .MatchBack(false, new CodeMatch(OpCodes.Ldloc_2))
                .Advance(1)
                .Set(OpCodes.Ldc_I4_S, maxSpawnedTrees)
                .InstructionEnumeration();
            /*
            foreach(CodeInstruction instruction in inst)
            {
                LogSource.LogInfo(instruction);
            }
            */
            return inst;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(GameScript), "InitTownWishes")]
        public static IEnumerable<CodeInstruction> InitTownWishesTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Change maximum wishes for extra crops / fruit trees
            int fruitWishesNeeded = (int) Math.Ceiling((maxSpawnedFruits - 6) / 2.0f);
            int cropWishesNeeded = (int)Math.Ceiling((maxSpawnedCrops - 6) / 2.0f);
            int wishesNeeded = Math.Max(fruitWishesNeeded, cropWishesNeeded);

            IEnumerable<CodeInstruction> inst = new CodeMatcher(instructions)
                .Start()
                .MatchForward(false, new CodeMatch(OpCodes.Call))
                .MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S))
                .SetOperandAndAdvance(wishesNeeded)
                .InstructionEnumeration();
            /*
            CodeMatcher codeMatcher = new CodeMatcher(inst).Start();
            while (codeMatcher.Pos < codeMatcher.Length)
            {
                LogSource.LogInfo($"{codeMatcher.Pos} : {codeMatcher.Instruction}");
                codeMatcher.Advance(1);
            }
            */
            return inst;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameScript), "BeginGame")]
        public static bool BeginGamePrefix(GameScript __instance)
        {
            #region Resize Crop array
            Traverse field = new Traverse(__instance).Field("spawnedCropObj");
            GameObject[] spawnedCropObj = field.GetValue<GameObject[]>();
            Array newArray = Array.CreateInstance(spawnedCropObj.GetType().GetElementType(), maxSpawnedCrops);
            Array.Copy(spawnedCropObj, newArray, spawnedCropObj.Length);
            field.SetValue(newArray);
            LogSource.LogInfo($"Crop list resized to {field.GetValue<GameObject[]>().Length}");
            #endregion

            #region Resize Fruit array
            Traverse fruitField = new Traverse(__instance).Field("spawnedFruitTreeObj");
            GameObject[] spawnedFruitObj = fruitField.GetValue<GameObject[]>();
            Array newFruitArray = Array.CreateInstance(spawnedFruitObj.GetType().GetElementType(), maxSpawnedFruits);
            Array.Copy(spawnedFruitObj, newFruitArray, spawnedFruitObj.Length);
            fruitField.SetValue(newFruitArray);
            LogSource.LogInfo($"Fruit Tree list resized to {fruitField.GetValue<GameObject[]>().Length}");
            #endregion

            #region Resize Wood Tree array
            Traverse treeField = new Traverse(__instance).Field("spawnedTreeObj");
            GameObject[] spawnedTreeObj = treeField.GetValue<GameObject[]>();
            Array newTreeArray = Array.CreateInstance(spawnedTreeObj.GetType().GetElementType(), maxSpawnedTrees);
            Array.Copy(spawnedTreeObj, newTreeArray, spawnedTreeObj.Length);
            treeField.SetValue(newTreeArray);
            LogSource.LogInfo($"Wood Tree list resized to {treeField.GetValue<GameObject[]>().Length}");
            #endregion

            return true;
        }

    }
}
