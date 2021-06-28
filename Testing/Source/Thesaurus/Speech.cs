using System;  
using System.Speech.Recognition;
using MonoGameHtml;

namespace Testing {
	public static class Speech {
		public static void Init() {  

			// Create an in-process speech recognizer for the en-US locale.  
			SpeechRecognitionEngine recognizer =  
				new SpeechRecognitionEngine(  
					new System.Globalization.CultureInfo("en-US"));


			// Create and load a dictation grammar.  
			recognizer.LoadGrammar(new DictationGrammar());

			// Add a handler for the speech recognized event.  
			recognizer.SpeechRecognized +=   
				new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);
			// Configure input to the speech recognizer.  
			recognizer.SetInputToDefaultAudioDevice();  

			// Start asynchronous, continuous speech recognition.  
			recognizer.RecognizeAsync(RecognizeMode.Multiple);
		}  

		// Handle the SpeechRecognized event.  
		static void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)  
		{  
			Console.WriteLine("Recognized text: " + e.Result.Text);
			foreach (var wordVar in e.Result.Words) {
				string word = wordVar.Text;
				//Console.WriteLine($"Word: {word}");

				Thesaurus.data.Add(Thesaurus.func(word, Scraper.GetSynonyms(word)));
				if (Thesaurus.data.Count > 7) Thesaurus.data.RemoveAt(0);
				Thesaurus.pack.SetVar("update", ++Thesaurus.update);
			}
		}
	}
}