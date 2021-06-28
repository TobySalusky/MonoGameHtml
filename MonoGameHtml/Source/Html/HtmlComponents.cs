﻿using System.Collections.Generic;
 using System.Linq;

 namespace MonoGameHtml {
	public static class HtmlComponents {
		public static string[] Create(params string[] componentFileContents) {
			List<string> componentStrings = new List<string>();
			
			// extract multiple components from single strings
			foreach (string componentFile in componentFileContents) {
				
				var bracketPairs = DelimPair.genPairs(componentFile, DelimPair.CurlyBrackets).Where(pair => pair.nestCount == 0).ToArray();

				for (int i = 0; i < bracketPairs.Length; i++) {
					string component = componentFile.sub((i == 0) ? 0 : bracketPairs[i - 1].AfterClose, bracketPairs[i].AfterClose);
					componentStrings.Add(component);
				}
			}
			return componentStrings.ToArray();
		}

		public static string Slider = @"
const Slider = (
	Action^^float^ onChange,
	object back: 'darkgray',
	object front: 'lightgray',
	object width: 100,
	object height: 30,
	float init = 0F
) => {

    HtmlNode node = null;
    float amount = init, lastAmount = amount;
    
    void toMouse() {
    	amount = Math.Clamp((@mp.X-node.PaddedX)/node.PaddedWidth, 0F, 1F);
		if (amount != lastAmount) onChange?.Invoke(amount);    	
    	lastAmount = amount;
    }

	return (
		<div ref={(HtmlNode el)=^node=el} onPress={()=^{
			toMouse();
		}} onMouseDrag={()=^{
			if (node.clicked) toMouse();
		}} width={width} height={height} backgroundColor={back}>
            <div backgroundColor={front} -width={int~: node.PaddedWidth * amount} height='100%'/>
        </div>
	);
}
", Toggle = @"
const Toggle = (
    Action^^bool^ onChange,
    object back: 'darkgray',
    object front: 'lightgray',
    object width: 100,
    object height: 50
) => {

    bool [val, setVal] = useState(false);

	return (
		<span onPress={()=^{
                setVal(!val);
                onChange?.Invoke(val);
              }} backgroundColor={back} width={width} height={height} borderRadius='50%' padding={10}>
            {val ? <div flex={1}/> : null}
            <div dimens={height} backgroundColor={front} borderRadius='50%'/>
            {val ? null : <div flex={1}/>}
        </span>
	);
}
";
	}
 }