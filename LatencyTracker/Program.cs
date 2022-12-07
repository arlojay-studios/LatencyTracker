using System.Net.NetworkInformation;

namespace R {
    public class P {

        public static void Main() {
            string d = "";
            Console.WriteLine("What domain would you like to ping?");
            Thread.Sleep(1000);
            string c = Console.ReadLine();
            if (c != "") {
                d = c;
            } else if (c == "") {
                d = "google.com";
            };
            Console.Clear();
            int y = 100;
            Console.WriteLine("How many times would you like to ping " + d + "? (5-65535) Higher values may take longer.");
            Thread.Sleep(1000);
            string z = Console.ReadLine();
            if (z != "") {
                if (Int32.TryParse(z, out y) != false) {
                    if (Int32.Parse(z) > 65535 | Int32.Parse(z) < 2) {
                        y = 100;
                    } else {
                        y = Int32.Parse(z);
                    };
                } else {
                    y = 100;
                };
            } else {
                y = 100;
            };
            Console.Clear();


            Console.WriteLine("Starting...");
            int i = 0;
            long[] r = new long[1];
            Ping p = new();
            while (true) {
                i++;
                Console.WriteLine("Pinging [" + i + "/" + y + "]");
                PingReply a = p.Send(d, 10000);
                Thread.Sleep(100);
                Array.Resize(ref r, r.Length + 1);
                Console.WriteLine("Processing [" + i + "/" + y + "]");
                r[r.Length - 1] = a.RoundtripTime;
                if (r.Length == y) {
                    Console.Clear();
                    g(r, d);
                    Console.Clear();
                };
            };
        }
        public static async void g(long[] v, string d) {
            long[] x = v.Distinct().ToArray();
            long m = v.Max();
            int j = 0;
            bool k = false;
            Array.Sort(x);
            if (m > 100) {
                foreach (var g in x) {
                    x[j] = g / 3;
                    j++;

                }
                k = true;
            }
            foreach (var g in x) {
                for (var i = 0; i < g; i++) {
                    if (g > 50) {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.BackgroundColor = ConsoleColor.Red;
                    } else if (g > 25) {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.BackgroundColor = ConsoleColor.Yellow;
                    } else if (g < 25) {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.BackgroundColor = ConsoleColor.Green;
                    }
                    Console.Write("#");
                    Console.ResetColor();

                }
                if (k == true) {
                    Console.Write(" " + g * 3 + "ms");
                } else if (k == false) {
                    Console.Write(" " + g + "ms");
                }
                Console.WriteLine();
            }
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("AVG: " + v.Average() + "ms");
            Console.WriteLine("MAX: " + v.Max() + "ms");
            Console.WriteLine("MIN: " + v.Where(h => h != 0).Min() + "ms");
            Console.WriteLine("1: Run Another Test | 2: Save Results to File | 3: Exit");
            string[] o = new string[v.Length + 4];
            int f = 4;
            foreach (var l in v) {
                o[f] = l.ToString();
                f++;
            }
            o[1] = d;
            o[2] = v.Average().ToString();
            o[3] = v.Max().ToString();
            o[4] = v.Min().ToString();
            var u = Console.ReadKey().Key;
            if (u == ConsoleKey.D1) {
                Console.Clear();
                Main();
            } else if (u == ConsoleKey.D2) {
                await File.WriteAllLinesAsync(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Results.txt", o);
                Console.WriteLine("Results saved to %Documents%/Results.txt");
                Console.ReadLine();
                Console.Clear();
                Main();
            } else if (u == ConsoleKey.D3) {
                Environment.Exit(0);
            }

        }
    }
}
