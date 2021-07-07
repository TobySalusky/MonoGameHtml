using System;

namespace MonoGameHtml.Exceptions {
    public class InvalidHtmlStructureException : Exception {
        public InvalidHtmlStructureException(string message) : base(message) { }
    }
}