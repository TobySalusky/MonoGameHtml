using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoGameHtml {
	public sealed class HtmlLiveEditRunner : HtmlRunner {

		private FileSystemWatcher fileWatcher;
		private HtmlRunner currentInstance;
		private readonly Func<Task<HtmlRunner>> generateRunner;
		
		public HtmlLiveEditRunner(Func<Task<HtmlRunner>> generateRunner) {
			this.generateRunner = generateRunner;
		}

		public void AttachFileWatcher(string path) {
			fileWatcher = new FileSystemWatcher(path) {
				NotifyFilter = NotifyFilters.Attributes
				               | NotifyFilters.CreationTime
				               | NotifyFilters.DirectoryName
				               | NotifyFilters.FileName
				               | NotifyFilters.LastAccess
				               | NotifyFilters.LastWrite
				               | NotifyFilters.Security
				               | NotifyFilters.Size
			};
			
			fileWatcher.Changed += OnChanged;
			//fileWatcher.Created += OnCreated;
			//fileWatcher.Deleted += OnDeleted;

			fileWatcher.Filter = "*.*";
			fileWatcher.IncludeSubdirectories = true;
			fileWatcher.EnableRaisingEvents = true;
		}

		private void OnChanged(object sender, FileSystemEventArgs e) {
			if (!FileIsReady(e.FullPath)) return; //first notification the file is arriving
			StartGenerateTask();
		}
		
		private bool FileIsReady(string path)
		{
			//One exception per file rather than several like in the polling pattern
			try
			{
				//If we can't open the file, it's still copying
				using (var file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					return true;
				}
			}
			catch (Exception)
			{
				return false;
			}
		}

		
		private void OnCreated(object sender, FileSystemEventArgs e) {
			StartGenerateTask();
		}

		private void OnDeleted(object sender, FileSystemEventArgs e) {
			StartGenerateTask();
		}

		private void StartGenerateTask() {
			Logger.log("unique", Util.randInt(100000));
			GenerateTask().Start();
		}

		public Task GenerateTask() {
			return new Task(() => {
				try {
					generateRunner.Invoke().ContinueWith(task => { currentInstance = task.Result; });
				} catch (Exception e) { 
					Logger.log(e);
				}
			});
		}

		public override void Update(GameTime gameTime, MouseState mouseState, KeyboardState keyState) {
			KeyInfo keys = new KeyInfo(keyState, lastKeyState);
			if (keys.down(Keys.LeftControl) && keys.pressed(Keys.R)) {
				StartGenerateTask();
			}
			currentInstance?.Update(gameTime, mouseState, keyState);
			lastKeyState = keyState;
		}

		public override void Render(SpriteBatch spriteBatch) {
			currentInstance?.Render(spriteBatch);
		}
	}
}