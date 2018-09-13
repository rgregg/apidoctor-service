using System;
using System.Diagnostics;

namespace service_runner.Logic
{
    public static class GitCloner 
    {
        public static string CloneGitRepoToLocalPath(string gitRepoUrl, string branch = "master" )
        {
            var uniqueGuid = System.Guid.NewGuid().ToString("d");
            var outputDir = "/tmp/" + uniqueGuid;

            Console.WriteLine($"cloning from {gitRepoUrl} into {outputDir}:");
            var cmd = $"git clone {gitRepoUrl} --branch {branch} --depth 1 {outputDir}";
            
            var escapedArgs = cmd.Replace("\"", "\\\"");

            Console.WriteLine($"cmd: {escapedArgs}");
            
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            process.WaitForExit();
            return outputDir;
        }

    }
}