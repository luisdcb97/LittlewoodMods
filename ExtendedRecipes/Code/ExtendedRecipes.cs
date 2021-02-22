using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BepInEx;
using HarmonyLib;
using BepInEx.Logging;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace LittlewoodMod
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    [BepInProcess("Littlewood.exe")]
    public class ExtendedRecipes : BaseUnityPlugin
    {
        public const string pluginGuid = "egnite.littlewood.extendedrecipes";
        public const string pluginName = "Extended Recipes";
        public const string pluginVersion = "1.0";

        public static ManualLogSource LogSource;

        static RecipeData[] customRecipes;
        static bool useCustomSprites;
        static bool useCustomNames;
        static bool useCustomIngredients;
        static bool useCustomUnlock;

        public void Awake()
        {
            LogSource = BepInEx.Logging.Logger.CreateLogSource("ExtRecipes");

            loadJSON();

            Harmony.CreateAndPatchAll(typeof(ExtendedRecipes));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameScript), "PopulateValidRecipes")]
        public static void PopulateValidRecipesPostfix(GameScript __instance) {
            if (useCustomUnlock)
            {
                unlockRecipes(__instance);
            }
            if (useCustomSprites)
            {
                replaceSprites(ref __instance);
            }
            if (useCustomIngredients)
            {
                replaceIngredients(__instance);
            }
        }

        private static void replaceIngredients(GameScript __instance)
        {
            List<Recipe> bbrecipes = new Traverse(__instance).Field("bubblePotRecipe").GetValue<List<Recipe>>();
            List<Recipe> szrecipes = new Traverse(__instance).Field("sizzlePanRecipe").GetValue<List<Recipe>>();
            List<Recipe> cbrecipes = new Traverse(__instance).Field("chopBoardRecipe").GetValue<List<Recipe>>();

            int id = 0;
            int size = bbrecipes.Count;
            for (; id < size; id++)
            {
                bbrecipes[id].ingredientId[0] = customRecipes[id].ingredients[0];
                bbrecipes[id].ingredientId[1] = customRecipes[id].ingredients[1];
            }
            size += szrecipes.Count;
            for (; id < size; id++)
            {
                szrecipes[id - 40].ingredientId[0] = customRecipes[id].ingredients[0];
                szrecipes[id - 40].ingredientId[1] = customRecipes[id].ingredients[1];
            }

            size += cbrecipes.Count;
            for (; id < size; id++)
            {
                cbrecipes[id - 80].ingredientId[0] = customRecipes[id].ingredients[0];
                cbrecipes[id - 80].ingredientId[1] = customRecipes[id].ingredients[1];
            }
            
            
            new Traverse(__instance).Field("bubblePotRecipe").SetValue(bbrecipes);
            new Traverse(__instance).Field("sizzlePanRecipe").SetValue(szrecipes);
            new Traverse(__instance).Field("chopBoardRecipe").SetValue(cbrecipes);
            LogSource.LogInfo("Recipe ingredients replaced");
        }

        private static void replaceSprites(ref GameScript __instance)
        {
            Sprite[] recipeSprites = new Traverse(__instance).Field("recipeSprite").GetValue<Sprite[]>();

            for (int i = 0; i < recipeSprites.Length; i++)
            {
                if (customRecipes[i].sprite != null)
                {
                    recipeSprites[i] = customRecipes[i].sprite;
                }
                else {
                    recipeSprites[i] = recipeSprites[i];
                }
            }

            new Traverse(__instance).Field("recipeSprite").SetValue(recipeSprites);
            LogSource.LogInfo("Recipe sprites replaced");
        }

        private static void unlockRecipes(GameScript __instance)
        {
            int[] bbrecipes = new Traverse(__instance).Field("bubblePotRecipeUnlocked").GetValue<int[]>();
            int[] szrecipes = new Traverse(__instance).Field("sizzlePanRecipeUnlocked").GetValue<int[]>();
            int[] cbrecipes = new Traverse(__instance).Field("chopBoardRecipeUnlocked").GetValue<int[]>();

            int id = 0;
            int size = bbrecipes.Length;
            for(; id < size; id++)
            {
                if (customRecipes[id].unlocked)
                {
                    bbrecipes[id] = 1;
                }
                else {
                    bbrecipes[id] = 0;
                }
            }
            size += szrecipes.Length;
            for (; id < size; id++)
            {
                if (customRecipes[id].unlocked)
                {
                    szrecipes[id - 40] = 1;
                }
                else {
                    szrecipes[id - 40] = 0;
                }
            }
            
            size += cbrecipes.Length;
            for (; id < size; id++)
            {
                if (customRecipes[id].unlocked)
                {
                    cbrecipes[id - 80] = 1;
                }
                else {
                    cbrecipes[id - 80] = 0;
                }
            }

            new Traverse(__instance).Field("bubblePotRecipeUnlocked").SetValue(bbrecipes);
            new Traverse(__instance).Field("sizzlePanRecipeUnlocked").SetValue(szrecipes);
            new Traverse(__instance).Field("chopBoardRecipeUnlocked").SetValue(cbrecipes);
            LogSource.LogInfo("Recipes unlocked");
        }

        private static void loadJSON()
        {
            string jsonPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(ExtendedRecipes)).Location) + "/customRecipes.json";
            var jsonObj = JObject.Parse(File.ReadAllText(jsonPath));
            useCustomSprites = (bool)jsonObj["useCustomSprites"];
            useCustomNames = (bool)jsonObj["useCustomNames"];
            useCustomIngredients = (bool)jsonObj["useCustomIngredients"];
            useCustomUnlock = (bool)jsonObj["useCustomUnlock"];
            var BPRecipes = (JArray)jsonObj["BubblePot"];
            var SZRecipes = (JArray)jsonObj["SizzlePan"];
            var CPRecipes = (JArray)jsonObj["ChopBoard"];
            int numRecipes = BPRecipes.Count + SZRecipes.Count + CPRecipes.Count;
            customRecipes = new RecipeData[numRecipes];
            foreach (JObject recipeObj in BPRecipes)
            {
                RecipeData rd = RecipeData.fromJSONObject(recipeObj);
                customRecipes[rd.id] = rd;
            }
            foreach (JObject recipeObj in SZRecipes)
            {
                RecipeData rd = RecipeData.fromJSONObject(recipeObj);
                customRecipes[rd.id] = rd;
            }
            foreach (JObject recipeObj in CPRecipes)
            {
                RecipeData rd = RecipeData.fromJSONObject(recipeObj);
                customRecipes[rd.id] = rd;
            }
            
            LogSource.LogInfo("Data loaded from file");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameScript), "GetRecipeName")]
        public static void GetRecipeNamePostfix(int a, ref string __result)
        {
            //Replace recipe name displayed when cooking
            if (useCustomNames)
            {
                __result = customRecipes[a].name;
            }
        }
    }

    class RecipeData
    {
        public int id;
        public Sprite sprite;
        public string name;
        public string imageName;
        public bool unlocked;
        public int[] ingredients;

        public static string assetPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(ExtendedRecipes)).Location) + "/ItemImages/";

        public static RecipeData fromJSONObject(JObject obj)
        {
            int id = (int)obj["Id"];
            int ing1 = (int)obj["Ingredient1"];
            int ing2 = (int)obj["Ingredient2"];
            string name = (string)obj["Name"];
            string imageName = (string)obj["ImageName"];
            bool isUnlocked = (bool)obj["IsUnlocked"];
            return new RecipeData(id, name, imageName, isUnlocked, ing1, ing2);

        }

        public RecipeData(int id, string name, string imageName, bool unlocked, int ing1, int ing2)
        {
            this.id = id;
            this.name = name;
            this.imageName = imageName;
            this.unlocked = unlocked;
            this.ingredients = new[] { ing1, ing2 };
            this.sprite = this.loadSprite();
        }

        private Sprite loadSprite()
        {
            try
            {
                return this.createSpriteFromPNG();
            }
            catch (Exception e)
            {
                ExtendedRecipes.LogSource.LogWarning("Failed to load sprite: " + e.Message);
                ExtendedRecipes.LogSource.LogWarning(e.StackTrace);
            }
            return null;
            
        }

        private string getSpriteName()
        {
            return this.imageName;
        }

        private Sprite createSpriteFromPNG()
        {
            string imagePath = RecipeData.assetPath + this.getSpriteName();
            
            var tex = new Texture2D(360, 108, TextureFormat.RGBA32, false);
            tex.LoadImage(File.ReadAllBytes(imagePath));
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Point;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1, 1);
        }
        
        public override string ToString()
        {
            string a = "";
            if (sprite != null)
            {
                a = $"Sprite-{sprite.name}";
            }
            else
            {
                a = "NULL";
            }
            return $"{{ \"Id\": {this.id}, \"Name\": {this.name}, \"Ingredients\": [{this.ingredients[0]},{this.ingredients[1]}], \"IsUnlocked\": {this.unlocked}, Sprite: {a}}}";
        }

    }
}
