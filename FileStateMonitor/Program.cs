using System;
using System.IO;
using System.Configuration;


namespace FileStateMonitor
{
    internal class Program
    {
        static int newFilesCount = 0;
        static void Main(string[] args)
        {
            string? path = ConfigurationManager.AppSettings["Path"];

            Console.WriteLine("Вас приветствует система мониторинга появления новых файлов в папке и подпапках! ");
            Console.WriteLine("Путь к целевой папке должен быть указан в файле App.config.");
            Console.WriteLine("Указан путь: " + path ?? "[ПУСТО]");

            using(var watch = new FileSystemWatcher())
            {
                // Увеличиваем размер буфера на случай огромного потока новых файлов или слишком длинных имен путей
                watch.InternalBufferSize = watch.InternalBufferSize * 5;
                Console.WriteLine("Размер буфера памяти: " + watch.InternalBufferSize);

                bool started = StartMonitoring(watch, path);

                if (!started)
                {
                    Console.WriteLine("Для завершения нажмите Enter");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine("\n********************************");
                Console.WriteLine("Мониторинг запущен.");
                Console.WriteLine("Для остановки нажмите Enter");
                Console.ReadLine();

                StopMonitoring(watch);
            }
            

            Console.WriteLine("Мониторинг остановлен.");
            Console.WriteLine("********************************");
            Console.WriteLine("\nБыло обнаружено новых файлов: " + newFilesCount.ToString());            
            Console.WriteLine("Для завершения программы нажмите Enter");
            Console.ReadLine();
        }

        

        private static bool StartMonitoring(FileSystemWatcher watcher, string? path) 
        {
            if (path == null || !Directory.Exists(path))
            {
                Console.WriteLine("Директория по указанному пути не найдена!");
                return false;
            }

            watcher.Path = path; // ?? @"D:\__Temp\Test";

            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime;

            watcher.Filter = "*"; //".txt";

            watcher.IncludeSubdirectories = true;

            watcher.Created += OnFileCreated;
            watcher.Error += OnError;

            watcher.EnableRaisingEvents = true;

            return true;
        }

        private static void StopMonitoring(FileSystemWatcher watcher)
        {
            watcher.EnableRaisingEvents = false;
        }

        private static void OnFileCreated(object sender, FileSystemEventArgs e) 
        {            
            DateTime createdAt = File.GetCreationTime(e.FullPath);            
            Console.WriteLine(createdAt.ToString() + ": " + "Обнаружен новый файл: " + e.FullPath);
            Console.WriteLine($"Новых файлов: { ++newFilesCount }");
            //newFilesCount++;
        }

        private static void OnError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine("Error: " + e.GetException().Message);
        }
    }
}
