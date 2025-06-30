using System;
using System.IO;
using System.Timers;
using System.Security.Cryptography;

class Program
{
    private static System.Timers.Timer? aTimer;

    //initialize directory paths
    public static string sourceFolder = string.Empty;
    private static string replicaFolder = string.Empty;
    private static string logFilePath = string.Empty;
    private static int interval = 2000; //default interval is 2 second


    public static void Main(string[] args)
    {
        try
        {
            //In case you want to skip the input in the keyboard
            /*sourceFolder = "Source";
            replicaFolder = "Replica";
            logFilePath = "log.txt";*/
            ReadInput();
            SetTimer();

            // Keep the application running
            Console.WriteLine("Timer started. Press 'q' to quit.");
            while (Console.ReadKey().KeyChar != 'q') { }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            aTimer?.Stop();
            aTimer?.Dispose();
            Console.WriteLine("File copy operation completed.");
        }

    }

    public static void SetTimer()
    {
        aTimer = new System.Timers.Timer(interval);
        aTimer.Elapsed += start;
        aTimer.AutoReset = true;
        aTimer.Enabled = true;
        Console.WriteLine($"Timer started with {interval} second interval");
    }

    public static void ReadInput()
    {
        // Get source folder with validation
        sourceFolder = GetValidInput(
            "Enter the source folder path:",
            input => Directory.Exists(input),
            "Source folder does not exist. Please try again."
        );

        // Create replica folder if it doesn't exist
        replicaFolder = GetValidInput(
            "Enter the replica folder path:",
            input =>
            {
                if (!Directory.Exists(input))
                {
                    try
                    {
                        Directory.CreateDirectory(input);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
                return true;
            },
            "Cannot create or access replica folder. Please try again."
        );

        // Create log file if it doesn't exist
        logFilePath = GetValidInput(
            "Enter the log file path:",
            input =>
            {

                if (!File.Exists(input))
                {
                    File.Create(input).Dispose();
                }
                return true;
            },
            "Log file does not exist and can not be created. Please try again.",
            input =>
            {
                // Force .txt extension no matter what
                input = Path.ChangeExtension(input, ".txt");
                return input;
            }
        );

        // Get synchronization interval
        interval = 1000 * int.Parse(GetValidInput(
            "Enter the synchronization interval (in seconds):",
            input => int.TryParse(input, out int n) && n > 0,
            "Invalid input. Please enter a valid integer."
        ));

    }

    public static string GetValidInput(string prompt, Func<string, bool> validator, string errorMessage, Func<string, string>? format = null)
    {
        while (true)
        {
            Console.WriteLine(prompt);
            string? input = Console.ReadLine();
            if (input != null)
            {
                if (format != null)
                {
                    input = format(input);
                }

                if (validator(input))
                {
                    return input;
                }

            }
            Console.WriteLine(errorMessage);
        }
    }

    public static void start(object? source, ElapsedEventArgs e)
    {
        FileHandler(sourceFolder, replicaFolder);
        FolderHandler(sourceFolder, replicaFolder);
    }

    public static void FolderHandler(string source, string replica)
    {
        //The recursive call is only necessary to be called inside the fisrt "foreach" and it will
        //deal with everything inside the folder
        foreach (string subFolder in Directory.GetDirectories(source))
        {
            // Get just the folder name
            string folderName = subFolder.Split('\\').Last();

            // Create the destination folder path
            string destinationFolderPath = Path.Combine(replica, folderName);

            //If the folder doesn't exist, create it
            if (!Directory.Exists(destinationFolderPath))
            {
                Directory.CreateDirectory(destinationFolderPath);
                FileHandler(subFolder, destinationFolderPath);
                //LogOperation($"Created folder: {destinationFolderPath}", logFilePath);
            }
            //If the folder exists, compare the contents and update if necessary
            else if (!AreDirectoriesEqual(subFolder, destinationFolderPath))
            {
                FileHandler(subFolder, destinationFolderPath);
                //LogOperation($"Updated folder: {destinationFolderPath}", logFilePath);
            }

            FileHandler(subFolder, destinationFolderPath);
            FolderHandler(subFolder, destinationFolderPath);
        }

        foreach (string replicaSubFolder in Directory.GetDirectories(replica))
        {
            // Get just the folder name
            string folderName = replicaSubFolder.Split('\\').Last();

            // Get the source folder path
            string sourceFolderPath = Path.Combine(source, folderName);

            // If the folder doesn't exist, delete it
            if (!Directory.Exists(sourceFolderPath))
            {
                FileHandler(sourceFolderPath, replicaSubFolder);
                Directory.Delete(replicaSubFolder, true);
                //LogOperation($"Deleted folder: {replicaSubFolder}", logFilePath);
            }
        }
    }

    public static void FileHandler(string folder, string destFolder)
    {
        try
        {
            // specific if if folder is deleted
            if (Directory.Exists(folder))
            {
                // Get all files in source folder
                foreach (string sourceFile in Directory.GetFiles(folder))
                {
                    // Get file name and extension
                    string fileName = Path.GetFileName(sourceFile);
                    string destinationFilePath = Path.Combine(destFolder, fileName);

                    //in any case the file will be overwritten, create or update the file
                    if (!File.Exists(destinationFilePath))
                    {
                        File.Copy(sourceFile, destinationFilePath, true);
                        LogOperation($"Created file: {destinationFilePath}", logFilePath);
                    }
                    else if (!FilesAreEqual(sourceFile, destinationFilePath))
                    {
                        File.Copy(sourceFile, destinationFilePath, true);
                        LogOperation($"Updated file: {destinationFilePath}", logFilePath);
                    }
                }
            }
            foreach (string file in Directory.GetFiles(destFolder))
            {
                string fileName = Path.GetFileName(file);
                string sourceFilePath = Path.Combine(folder, fileName);
                if (Directory.Exists(folder))
                {
                    if (!File.Exists(sourceFilePath))
                    {
                        File.Delete(file);
                        LogOperation($"Deleted file: {file}", logFilePath);
                    }
                }
                else
                {
                    LogOperation($"Deleted File: {file} and Folder: {folder}", logFilePath);
                }
            }
        }
        catch (Exception ex)
        {
            LogOperation($"Error: {ex.Message}", logFilePath);
        }
    }

    public static bool FilesAreEqual(string file1, string file2)
    {
        string hash1 = GetFileHash(file1);
        string hash2 = GetFileHash(file2);
        return hash1 == hash2;
    }
//
    public static string GetFileHash(string filePath)
    {
        using (var md5 = MD5.Create())
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);
            byte[] hashBytes = md5.ComputeHash(fileBytes);
            return Convert.ToHexString(hashBytes);
        }
    }

    public static void LogOperation(string message, string logFilePath)
    {
        string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}";
        Console.WriteLine(logEntry);
        File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
    }

    public static bool AreDirectoriesEqual(string sourceDir, string replicaDir)
    {
        var sourceFiles = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories)
                                   .Select(f => f.Substring(sourceDir.Length + 1))  // relative path
                                   .ToList();
        
        var replicaFiles = Directory.GetFiles(replicaDir, "*.*", SearchOption.AllDirectories)
                                    .Select(f => f.Substring(replicaDir.Length + 1))
                                    .ToList();

        if (!sourceFiles.SequenceEqual(replicaFiles))
        {
            Console.WriteLine("File lists differ.");
            return false;
        }

        foreach (var relativePath in sourceFiles)
        {
            string sourceFilePath = Path.Combine(sourceDir, relativePath);
            string replicaFilePath = Path.Combine(replicaDir, relativePath);

            if (!File.Exists(replicaFilePath))
            {
                Console.WriteLine($"Missing file in replica: {relativePath}");
                return false;
            }

            if (!FilesAreEqual(sourceFilePath, replicaFilePath))
            {
                Console.WriteLine($"File differs: {relativePath}");
                return false;
            }
        }

        return true;
    }

}
    