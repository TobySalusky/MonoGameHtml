using System;
using System.Threading.Tasks;

namespace MonoGameHtml {
	public static class HtmlLiveEdit {
		public static async Task<HtmlLiveEditRunner> Create(Func<Task<HtmlRunner>> generateRunner, string watchPath = null) {
			var liveEditRunner = new HtmlLiveEditRunner(generateRunner);
			Logger.Log("TEST1");
			//await liveEditRunner.GenerateTask();
			liveEditRunner.GenerateTask().Start();
			Logger.Log("TEST2");
			
			if (watchPath != null) liveEditRunner.AttachFileWatcher(watchPath);
			return liveEditRunner;
		}
	}
}