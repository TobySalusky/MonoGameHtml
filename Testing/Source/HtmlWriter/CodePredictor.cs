using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using MonoGameHtml;

namespace Testing {
    public static class CodePredictor {

        private static AdhocWorkspace workspace;
        private static Project scriptProject;
        
        static CodePredictor() {
            
        }

        private static async Task<List<string>> PredictCS(string searchFor, string code, int index) { // TODO: overhaul
            try {
                var host = MefHostServices.Create(MefHostServices.DefaultAssemblies);
                Logger.log("HOST",host);
                workspace = new AdhocWorkspace(host);
                
                var compilationOptions = new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    usings: new[] { "System" });
                   
                var scriptProjectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "Script", "Script", LanguageNames.CSharp, isSubmission: true)
                    .WithMetadataReferences(new[] 
                    { 
                        MetadataReference.CreateFromFile(typeof(object).Assembly.Location) 
                    })
                    .WithCompilationOptions(compilationOptions);

                scriptProject = workspace.AddProject(scriptProjectInfo);

                var scriptDocumentInfo = DocumentInfo.Create(
                    DocumentId.CreateNewId(scriptProject.Id), "Script",
                    sourceCodeKind: SourceCodeKind.Script,
                    loader: TextLoader.From(TextAndVersion.Create(SourceText.From(code), VersionStamp.Create())));
                var scriptDocument = workspace.AddDocument(scriptDocumentInfo);
                
                var completionService = CompletionService.GetService(scriptDocument);
                var results = await completionService.GetCompletionsAsync(scriptDocument, index);

                return results.Items.Select(item => item.DisplayText).FilterOrderBy(searchFor);

            } catch (Exception e) {
                Logger.log(e.StackTrace);
                Logger.log(e.Message);
            }

            return null;
        }

        private static List<string> PredictHtmlAttribute(string searchFor, string code, int index) {
            var htmlPairs = Parser.FindHtmlPairs(code);
            
            foreach (HtmlPair pair in htmlPairs) {
                string header = pair.headerContents(code);
                if (!pair.indexIsInside(index)) continue;
                if (index > pair.openIndex + header.Length + 1) continue;
                
                int headerIndex = index - pair.openIndex - 2;
                var dict = DelimPair.searchAll(header, DelimPair.CurlyBrackets, DelimPair.SingleQuotes);
                if (!DelimPair.allNestOf(0, header.nestAmountsLen(headerIndex, 0, dict))) {
                    return null;
                }

                // TODO: make generally used function to get prop names and values from prop string
                // TODO: don't predict things that are already there!!!

                return HtmlNode.KnownPropNames.FilterOrderBy(searchFor);
            }

            return null;
        }

        private static List<string> FilterOrderBy(this IEnumerable<string> collection, string searchFor) {
            return collection.
                Where(str => str.Contains(searchFor) && str != searchFor).
                OrderBy(str => str.indexOf(searchFor)).
                ToList();
        }

        public static string FindSearchFor(string code, int index) {
            for (int i = index - 1; i >= 0; i--) {
                if (code[i].IsValidReferenceNameCharacter()) continue;
                return code[(i + 1)..(index)];
            }

            return "";
        }

        public static async Task<List<string>> Predict(string searchFor, string code, int index) {

            if (searchFor == "") return null;
            
            var htmlPredictions = PredictHtmlAttribute(searchFor, code, index);

            if (htmlPredictions != null) return htmlPredictions;

            return await PredictCS(searchFor, code, index);
        }
    }
}