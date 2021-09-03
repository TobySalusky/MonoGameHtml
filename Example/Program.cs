using System;

namespace Example {
    public static class Program {
        [STAThread]
        static void Main() {
            using (var game = new ExampleGame())
                game.Run();
        }
    }
}