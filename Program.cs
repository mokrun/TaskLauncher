using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Launcher
{
    class TaskLauncher
    {

        public static void Main()
        {
            string path = "C:\\Windows\\System32\\NotePad.exe";
            int interval = 3000;
            bool exit = false;
           
            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("Welcome to the Task Launcher");
                Console.WriteLine("Please, make a choice with the number keys and Enter");
                Console.WriteLine($"1 - Change the process path (Set to {path})");
                Console.WriteLine($"2 - Change the polling interval (Set to {interval} milliseconds)");
                Console.WriteLine("3 - Start the process");
                Console.WriteLine("4 - Exit");
                int key = 0;
                try
                {
                    key = Convert.ToInt32(Console.ReadLine());
                }
                catch (FormatException e)
                {
                    Console.WriteLine($"{e} has been caught. Input must be wrong.");
                }
                switch (key)
                {
                    case 1:
                        Console.Clear();
                        Console.WriteLine("Please, enter the full path and filename of the process and confirm with Enter.");
                        Console.WriteLine("Double backslash (\\) and forward slash (/) can be used.");
                        string newpath = "C:\\Windows\\System32\\NotePad.exe";
                        try
                        {   string pp = Console.ReadLine() ?? throw new FileNotFoundException();
                            newpath = $@"{pp}"; //creating an object link to store path as is
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"{e} has been caught. Input format wasn't recognized.");
                        }
                        
                        if (CheckPath(newpath)) 
                        {
                            path = newpath;
                            Console.Clear();
                            Console.WriteLine("The new path has been saved. Press any key to return to the menu.");
                            Console.ReadLine();
                            break;
                        }
                        else 
                        {
                            Console.Clear();
                            Console.WriteLine("Invalid filename or path, try again.");
                            Console.ReadLine();
                            break;
                        }
                        
                    case 2:
                        Console.Clear();
                        int newinterval = 3000;
                        Console.WriteLine("Please, enter the required polling interval in milliseconds and confirm with Enter.");
                        try
                        {
                            newinterval = Convert.ToInt32(Console.ReadLine());
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("{0} has been caught.", e);
                        }
                        if (newinterval > 500)
                        {
                            Console.Clear();
                            interval = newinterval;
                            Console.WriteLine("New interval has been saved.");
                            Console.ReadLine();
                            break;
                        }
                        else
                        {
                            Console.Clear();
                            Console.WriteLine("The entered interval is invalid. Try an interval that is bigger than 500."); //The CPU usage is calculated as a change of stock over time
                            Console.ReadLine(); //It needs at least a bit of time to compare two values
                            break;
                        }
                        
                    case 3:
                        Console.Clear();
                        // This block fixes a random crash upon launching a process
                        static void Sleep(int ms)
                        {
                            new ManualResetEvent(false).WaitOne(ms);
                        }
                        ProcessStartInfo processInfo = new ProcessStartInfo
                        {
                            FileName = path,
                            UseShellExecute = true
                        };
                        Process newProcess = new Process();
                        newProcess.StartInfo = processInfo;
                        newProcess.Start(); 
                        using (newProcess)
                        { 
                         double GetCpuUsage()
                            {
                                var startTime = DateTime.UtcNow;
                                var startCpuUse = newProcess.TotalProcessorTime;
                                Sleep(495);
                                var endTime = DateTime.UtcNow;
                                var endCpuUse = newProcess.TotalProcessorTime;
                                var cpuUsedMs = (endCpuUse - startCpuUse).TotalMilliseconds;
                                var msPassed = (endTime - startTime).TotalMilliseconds;
                                var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * msPassed);
                                return cpuUsageTotal * 100;
                            }

                            do
                            {
                                if (!newProcess.HasExited)
                                {
                                    newProcess.Refresh();
                                    Console.WriteLine();
                                    var cpuPercentage = GetCpuUsage();
                                    var workingSet = newProcess.WorkingSet64;
                                    var privateMemory = newProcess.PrivateMemorySize64;
                                    var openHandles = newProcess.HandleCount;
                                    Console.WriteLine($"--------------{newProcess.ProcessName}--------------");
                                    Console.WriteLine($"  Processor use                 : {cpuPercentage}");
                                    Console.WriteLine($"  Working set used by process   : {workingSet}");
                                    Console.WriteLine($"  Number of private bytes       : {privateMemory}");
                                    Console.WriteLine($"  Open handle count             : {openHandles}");
                                    using (StreamWriter sw = File.AppendText("output.csv"))
                                    {
                                        sw.WriteLine("{0},{1},{2},{3}", cpuPercentage, workingSet, privateMemory, openHandles);
                                    }
                                    Sleep(interval - 495);
                                }
                            }
                            while (!newProcess.WaitForExit(1000));
                        }
                        break;

                    case 4:
                        exit = true;
                        break;

                    default:
                        Console.WriteLine("Wrong input, please try again.");
                        Console.ReadLine();
                        break;

                }
            }

            static bool CheckPath(string path)
            {
                FileInfo fi = new FileInfo(path) ?? throw new FileNotFoundException();
                if (File.Exists(path) && fi.Extension == ".exe")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
