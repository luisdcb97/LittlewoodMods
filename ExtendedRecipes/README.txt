Extended Recipes V1.0

Requirements:
    - Requires BepInEx to be installed (can be found at https://github.com/BepInEx/BepInEx/releases 
        , a windows installer is added)

Installing BepInEx:
    - Locate your LittleWood.exe installation folder (Steam can help with this)
    - Unzip BepInEx to that folder, and replace items if asked
    - Run game once and it should be installed :)

Adding the ER plugin:
    - Copy the Plugin folder to <installation folder>/BepInEx/plugins/
    - The mod should be loaded :)

Editing gamedata:
    - Changes require restarting the game to take effect
    - [Recipe Names]:
        Change the "Name" value for the item you want to change and set the "useCustomNames" to true
    - [Ingredient Changes]:
        Change "Ingredient1" and/or "Ingredient2" value to the items you want and set the "useCustomIngredients" to true
        ItemList.json has a list of items grouped by the in-game categories
        Extend Recipes V1 only supports recipes having exactly 2 ingredients and a total of 120 recipes like the default game
    - [Image Change]:
        Change "ImageName" to the path of the image file starting at ItemImages folder and set the "useCustomSprites" to true
        Please keep to the same image sizes and format as the examples given, mod behaviour is undetermined otherwise
    - [Unlock Change]:
        Change "IsUnlocked" for the chosen recipe to true and set the "useCustomUnlock" to true
        Recipe will be craftable but no achievements for discovering them will be earned (unless you unlock the last one manually ;) )

