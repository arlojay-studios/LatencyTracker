using LatencyTracker;

namespace lcExceptions { 

    internal class UserInputErrors {
        public static async Task InvalidAddress() {
            Console.Clear();
            Console.WriteLine("Invalid Adress - Press any key to retry");
            Console.ReadLine();
            Console.Clear();
            await Program.Main();
        }
    }
}
