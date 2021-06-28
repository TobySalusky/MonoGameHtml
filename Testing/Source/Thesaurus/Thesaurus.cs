using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Microsoft.Xna.Framework;
using MonoGameHtml;

namespace Testing {
	public static class Thesaurus {

        public static List<(Color, string[])> data = new List<(Color, string[])>();
        public static int update = 0;

        public static StatePack pack;
        
        public static Func<string, string[], (Color, string[])> func = (str, synonyms) => {
            var arr = new string[4];
            float val = Util.sin01(update / 7F * Maths.twoPI);
            Color col = new Color(val, 0, 1-val, 1F);
            arr[0] = str;

            string pop(int i) {
                string returnStr = synonyms[i];
                var list = synonyms.ToList();
                list.RemoveAt(i);
                synonyms = list.ToArray();

                return returnStr;
            }

            for (int i = 1; i < arr.Length; i++) {

                arr[i] = (synonyms.Length == 0) ? "?" : pop(Util.randInt(synonyms.Length));
            }

            return (col, arr);
        };
        
		public static async void Init(GameMain gameMain) {
            
            const string components = @"
const App = () => {

    int [update, setUpdate] = useState(0);

    return (
        <span onTick={()=^{
            if (update != $update) {
                setUpdate($update);
            }
        }}>
            <div>
                {$list.map(tuple =^
                <span>
                    <div class='Cont'>
                        <div class='In' backgroundColor={tuple.Item1}>{tuple.Item2[0]}</div>
                    </div>
                </span>
                )}
            </div>
            
            <div>
                {$list.map(tuple =^
                <span>
                    {tuple.Item2.map((str, i) =^
                        (i == 0) ? null :
                        <div class='Cont'>
                            <div class='In' backgroundColor={tuple.Item1}>{str}</div>
                        </div>
                    )}
                </span>
                )}
            </div>
        </span>
    );
}
";
            
            const string html = @"
<body>
    <App/>
</body>
";
            
            pack = StatePack.Create(
                "list", data,
                "update", 0
            );

            gameMain.htmlInstance = await HtmlProcessor.GenerateRunner(html, pack, 
                components: HtmlComponents.Create(components));
            
            Speech.Init();
        }
	}
}