using System;

namespace MonoGameHtml.Exceptions {
    public class InvalidHtmlComponentException : Exception {
        public InvalidHtmlComponentException(string message) : base(message) { }
    }
}