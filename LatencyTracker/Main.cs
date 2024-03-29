﻿using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Text.Json;
using lcExceptions;
using lcSettings;
using System.Security.Cryptography.X509Certificates;

namespace LatencyTracker {

    public class Program {

        private static readonly string workingFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/SI/LatencyTracker";

        public static async Task Main() {
            var settings = new Settings();
            short? PingAutoAmount;
            string PreviousSaveSettings = "0";
            string defaultOption1 = "0";
            string defaultOption2 = "0";
            Ping ping = new();
            List<Settings> data = new() {
                new Settings() { PreviousSaveSettings = "0", PingAutoAmount = 100 }
            };



            if (!Directory.Exists(workingFolder)) {
                Directory.CreateDirectory(workingFolder);

            }
            if (!File.Exists(workingFolder + "/settings.json")) {

                await using FileStream dataStream = File.Create(workingFolder + "/settings.json");
                await JsonSerializer.SerializeAsync(dataStream, data);
                await dataStream.DisposeAsync();

            } else {
                try {
                    await using FileStream dataStream = File.OpenRead(workingFolder + "/settings.json");
                    Settings? fromFile = await JsonSerializer.DeserializeAsync<Settings>(dataStream);
                    PingAutoAmount = fromFile?.PingAutoAmount;
                    PreviousSaveSettings = fromFile?.PreviousSaveSettings ?? PreviousSaveSettings;
                    await dataStream.DisposeAsync();
                } catch (JsonException) {
                    await using FileStream dataStream = File.OpenWrite(workingFolder + "/settings.json");
                    await JsonSerializer.SerializeAsync(dataStream, data);
                    await dataStream.DisposeAsync();
                }
            }

            if (PreviousSaveSettings != "0") {
                defaultOption1 = "-- Default";
            } else {
                defaultOption2 = "-- Default";
            }

            string domain = "";
            Console.WriteLine("What domain would you like to ping?");
            Thread.Sleep(1000);
            string? userInputDomain = Console.ReadLine();
            if (userInputDomain != null) {
                try {
                    ping.Send(userInputDomain, 10000);
                    // 10000ms max trip time
                    domain = userInputDomain;
                } catch (PingException) {
                    await UserInputErrors.InvalidAddress();
                } catch (ArgumentNullException) {
                    await UserInputErrors.InvalidAddress();
                }
            } else {
                domain = "google.com";
            }


            Console.Clear();
            Console.WriteLine("Ping " + domain + " how many times? (ENTER for Auto or define value 1-65535)");
            Thread.Sleep(1000);
            string? input = Console.ReadLine();
            if (input == null || !Int16.TryParse(input, out short pingAmount) || pingAmount <= 1) {
                if (settings.PingAutoAmount >= 1) {
                    pingAmount = settings.PingAutoAmount;
                } else { pingAmount = 100; }
            } else {
                pingAmount = Int16.Parse(input);
                if (settings.PingAutoAmount == 0) {
                    var update = new Settings() { PreviousSaveSettings = PreviousSaveSettings, PingAutoAmount = 100 };
                    await using FileStream dataStream = File.OpenWrite(workingFolder + "/settings.json");
                    await JsonSerializer.SerializeAsync(dataStream, update);
                    await dataStream.DisposeAsync();
                }
            }
            Console.Clear();



            Console.WriteLine("Starting...");
            int pingCounter = 0;
            long[] results = new long[pingAmount];
            string[] timestamps = new string[pingAmount];

            while (pingCounter < pingAmount) {
                pingCounter++;
                timestamps[^1] = DateTime.Now.ToString();
                Console.WriteLine($"Pinging [{pingCounter} / {pingAmount}]");
                PingReply reply = ping.Send(domain, 10000);
                Thread.Sleep(100);
                Array.Resize(ref results, results.Length + 1);
                Console.WriteLine($"Processing [{pingCounter} / {pingAmount}]");


                results[^1] = reply.RoundtripTime;
                if (results.Length == pingAmount) {
                    Console.Clear();
                    await ProcessResults(results, timestamps, domain, defaultOption1, defaultOption2);
                    return;
                };
            };
        }
        public static async Task ProcessResults(long[] results, string[] timestamps, string domain, string? defaultOption1, string? defaultOption2) {
            long[] filteredResults = results.Distinct().ToArray();
            long resultsMax = results.Max();
            int counter = 0;
            bool dividedByThree = false;
            Array.Sort(filteredResults);
            if (resultsMax > 100) {
                foreach (var g in filteredResults) {
                    filteredResults[counter] = g / 3;
                    counter++;

                }
                dividedByThree = true;
            }
            foreach (var result in filteredResults) {
                for (var temp = 0; temp < result; temp++) {
                    if (result > 50) {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.BackgroundColor = ConsoleColor.Red;
                    } else if (result >= 25) {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.BackgroundColor = ConsoleColor.Yellow;
                    } else if (result < 25) {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.BackgroundColor = ConsoleColor.Green;
                    }
                    Console.Write("#");
                    Console.ResetColor();

                }
                if (dividedByThree == true) {
                    Console.Write(" " + result * 3 + "ms");
                } else if (dividedByThree == false && result != 0) {
                    Console.Write(" " + result + "ms");
                }
                Console.WriteLine();
            }
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("AVG: " + results.Average() + "ms");
            Console.WriteLine("MAX: " + results.Max() + "ms");
            Console.WriteLine("MIN: " + results.Where(temp => temp != 0).Min() + "ms");
            Console.WriteLine("1: Run Another Test | 2: Save Results to File | 3: Exit");
            string[] output = new string[results.Length + 4];
            int outputArrayStart = 4;
            foreach (var value in results) {
                output[outputArrayStart] = outputArrayStart - 4 + " " + value.ToString() + "ms";
                outputArrayStart++;
            }
            output[0] = "DOMAIN: " + domain;
            output[1] = "AVG: " + results.Average().ToString() + "ms";
            output[2] = "MAX: " + results.Max().ToString() + "ms";
            output[3] = "MIN: " + results.Min().ToString() + "ms";
            var keyPressed = Console.ReadKey().Key;
            Console.Clear();
            if (keyPressed == ConsoleKey.D1) {
                await Main();
            } else if (keyPressed == ConsoleKey.D2) {
                Console.WriteLine("Export Type");
                Console.WriteLine($"1: Simple (DOMAIN, AVG, MAX, MIN) {defaultOption1}");
                Console.WriteLine("2: Advanced (TIMESTAMP, DOMAIN, PINGCOUNT, AVG, MAX, MIN)");
                Console.WriteLine($"3: Custom {defaultOption2}");
                keyPressed = Console.ReadKey().Key;
                if (keyPressed == ConsoleKey.D1) {
                    await File.WriteAllLinesAsync(workingFolder + "/Results.txt", output);
                    Console.Clear();
                    Console.WriteLine($"Results saved to {workingFolder}/Results.txt - Press any key to continue");
                    Console.ReadLine();
                    Console.Clear();
                    await Main();
                } else if (keyPressed == ConsoleKey.D2) {
                    string[] output2 = new string[results.Length + 6];
                    output2[0] = "TIME: " + DateTime.Now.ToString();
                    output2[1] = "DOMAIN: " + domain;
                    output2[2] = "COUNT: " + results.Length.ToString();
                    output2[3] = "AVG: " + results.Average().ToString() + "ms";
                    output2[4] = "MAX: " + results.Max().ToString() + "ms";
                    output2[5] = "MIN: " + results.Min().ToString() + "ms";
                    outputArrayStart = 6;
                    foreach (var value in results) {
                        output2[outputArrayStart] = $"{outputArrayStart - 6} ${timestamps[outputArrayStart - 6]} ${value}ms";
                        outputArrayStart++;
                    }
                    await File.WriteAllLinesAsync(workingFolder + "/Results.txt", output2);
                    Console.Clear();
                    Console.WriteLine($"Results saved to {workingFolder}/Results.txt - Press any key to continue");
                    Console.ReadLine();
                    Console.Clear();
                    await Main();
                } else if (keyPressed == ConsoleKey.D3) {
                    Console.WriteLine("Define output table order or press enter for default");
                    Console.WriteLine("Variables: TIME (T), DOMAIN (D), COUNT (C), AVG (A), MAX (H), MIN (L)");
                    string? input = Console.ReadLine();
                    Console.Clear();
                    Console.Write("Processing");
                    if (input != null) {
                        string[] output3 = new string[results.Length];
                        foreach (char c in input) {
                            input = input.Remove(0, 1);
                            var currentWritePostion = 0;
                            if (!input.Contains(c)) {
                                long[] visibleToWriteHead = Array.Empty<long>();
                                results.CopyTo(visibleToWriteHead, 0);
                                Array.Resize(ref visibleToWriteHead, currentWritePostion);
                                switch (c) {
                                    case 'T':
                                        foreach (string line in output3) {
                                            output3[currentWritePostion] = $"{line}{timestamps[currentWritePostion]} ";
                                            currentWritePostion++;
                                        }
                                        currentWritePostion = 0;
                                        break;
                                    case 'D':
                                        foreach (string line in output3) {
                                            output3[currentWritePostion] = $"{line}{domain} ";
                                            currentWritePostion++;
                                        }
                                        currentWritePostion = 0;
                                        break;
                                    case 'C':
                                        foreach (string line in output3) {
                                            output3[currentWritePostion] = $"{line}{currentWritePostion} ";
                                            currentWritePostion++;
                                        }
                                        currentWritePostion = 0;
                                        break;
                                    case 'A':
                                        foreach (string line in output3) {
                                            output3[currentWritePostion] = $"{line}{visibleToWriteHead.Average()} ";
                                            currentWritePostion++;
                                        }
                                        currentWritePostion = 0;
                                        break;
                                    case 'H':
                                        foreach (string line in output3) {
                                            output3[currentWritePostion] = $"{line}{visibleToWriteHead.Max()} ";
                                            currentWritePostion++;
                                        }
                                        currentWritePostion = 0;
                                        break;
                                    case 'L':
                                        foreach (string line in output3) {
                                            output3[currentWritePostion] = $"{line}{visibleToWriteHead.Min()} ";
                                            currentWritePostion++;
                                        }
                                        currentWritePostion = 0;
                                        break;

                                }
                            }
                        };
                        Console.WriteLine($"Results saved to {workingFolder}/Results.txt - Press any key to continue");
                        Console.ReadLine();
                        Console.Clear();
                        await Main();
                    }

                }
                Console.ReadLine();
                Console.Clear();
                await Main();
            } else if (keyPressed == ConsoleKey.D3) {
                Environment.Exit(0);
            }

        }
    }
}