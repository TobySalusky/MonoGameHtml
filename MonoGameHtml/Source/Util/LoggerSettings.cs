using System.Collections.Generic;
using System.Linq;

namespace MonoGameHtml {
    public class LoggerSettings {
        public bool logOutput, allowColor, colorOutputCS, formatColoredCS; // TODO: add format option to uncolored CS

        public LoggerSettings() {
            logOutput = false;
            allowColor = true;
            colorOutputCS = false;
            formatColoredCS = true;
        }
    }
}