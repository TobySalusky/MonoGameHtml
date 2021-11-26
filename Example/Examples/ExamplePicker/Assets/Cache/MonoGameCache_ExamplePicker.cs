

using System;
using System.Collections.Generic;
using System.Linq;
using MonoGameHtml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System.Reflection;
using Example;

namespace MonoGameHtmlGeneratedCode.ExamplePicker {
	public class Cache_ExamplePicker : StatePack {
	public Cache_ExamplePicker(params object[] initialVariableNamesAndObjects) : base(initialVariableNamesAndObjects) {}
		protected override string[] cachedInput() {
			return new string[]{ @"<App></App>", @"const App = () => {

    string[] gameList;

    {
        Assembly assembly = typeof(Example.Program).Assembly;
        gameList = assembly.GetTypes().map(type => type.Name).Where(name => name.EndsWith('Game') && name != 'ExamplePickerGame')
            .map(name => name.Substring(0, name.IndexOf('Game')));
    };
    
    return (
        <body>
            {gameList.map(name => <Choice name={name} ></Choice>)}
        </body>
    );
}", @"

const Choice = (string name) => {

    float hoverTime = 0F;
    const float maxHoverTime = 0.175F;

    var onHover = () => {
        hoverTime += 2 * deltaTime;
    }; 

    var onTick = () => {
        hoverTime -= deltaTime;
        hoverTime = Math.Clamp(hoverTime, 0, maxHoverTime);
    };

    var onPress = () => {
        Assembly assembly = typeof(Example.Program).Assembly;
        Game newGame = (Game) assembly.CreateInstance($'Example.{name}Game');

        if (newGame == null) {
            Logger.log('No such game exists!');
            return;
        }

        Program.ChangeGame(newGame);
    };

    return (
        <span onHover={onHover} onTick={onTick} onPress={onPress} class='GameRow'>
            <div -width={int~: sin(hoverTime / maxHoverTime) * 20 + 30} ></div>
            <h5 class='GameChoice'>
                {name}
            </h5>
        </span>
    );
}" };
		}

		protected override HtmlNode cachedNode() {
			/*IMPORTS_DONE*/
HtmlNode CreateApp(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null) {
	
	HtmlNode ___node = null;
		

	
string[] gameList;
{
        Assembly assembly = typeof(Example.Program).Assembly;
        gameList = assembly.GetTypes().Select(type => type.Name).ToArray().Where(name => name.EndsWith("Game") && name != "ExamplePickerGame")
            .Select(name => name.Substring(0, name.IndexOf("Game"))).ToArray();
    };
;
	___node = newNode("body", props: new Dictionary<string, object> {}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr((gameList.Select(name => CreateChoice("Choice", props: new Dictionary<string, object> {["name"]=name}, children: null, childrenFunc: null, textContent: "", name: name)).ToArray()))));
	
	return ___node;
}

HtmlNode CreateChoice(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null, string? name = null) {
	
	HtmlNode ___node = null;
		

	
float hoverTime = 0F;
const float maxHoverTime = 0.175F;
var onHover = (Action)(()=>{
        hoverTime += 2 * deltaTime;
    });
var onTick = (Action)(()=>{
        hoverTime -= deltaTime;
        hoverTime = Math.Clamp(hoverTime, 0, maxHoverTime);
    });
var onPress = (Action)(()=>{
        Assembly assembly = typeof(Example.Program).Assembly;
        Game newGame = (Game) assembly.CreateInstance($"Example.{name}Game");

        if (newGame == null) {
            Logger.log("No such game exists!");
            return;
        }

        Program.ChangeGame(newGame);
    });
;
	___node = newNode("span", props: new Dictionary<string, object> {["onHover"]=onHover, ["onTick"]=onTick, ["onPress"]=onPress, ["class"]="GameRow"}, children: nodeArr(newNode("div", props: new Dictionary<string, object> {["-width"]=(Func<int>)(() => (int)(sin(hoverTime / maxHoverTime) * 20 + 30))}, textContent: ""), newNode("h5", props: new Dictionary<string, object> {["class"]="GameChoice"}, textContent: (Func<string>)(()=> ""+(name)+""))));
	
	return ___node;
}
HtmlNode node = CreateApp("App", props: new Dictionary<string, object> {}, children: null, childrenFunc: null, textContent: "");
setupNode(node);
return node;
		}
	}
}
