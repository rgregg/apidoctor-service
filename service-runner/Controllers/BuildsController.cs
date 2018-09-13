using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using service_runner.Logic;

namespace service_runner.Controllers
{
    [Route("api/[controller]")]
    public class BuildsController : ControllerBase
    {
        // GET api/builds
        [HttpGet]
        public string Get(BuildRequestModel build)
        {
            if (build == null)
            {
                return "Error: No build request was detected.";
            }

            if (String.IsNullOrEmpty(build.GitUrl))
            {
                return "Error: No URL was received.";
            }

            return $"Starting to validate repo: {build.GitUrl}.";
        }

        // POST api/builds
        [HttpPost]
        public string Post(BuildRequestModel request)
        {
            if (request == null)
            {
                return "Error: No build request was detected.";
            }

            if (String.IsNullOrEmpty(request.GitUrl))
            {
                return "Error: No URL was received.";
            }

            // See if we can clone the URL provided
            var localPath = GitCloner.CloneGitRepoToLocalPath(request.GitUrl, request.Branch);

            // Do some validation on the cloned directory path
            var validator = new MarkdownValidator(localPath);
            validator.EnsureDocSet();
            validator.ClearExistingIssues();
            var result = validator.CheckForBrokenLinks();

            Console.WriteLine($"CheckForBrokenLinks returned {result}");

            Console.WriteLine($"Detected {validator.Issues.IssuesInCurrentScope} issues in the documentation.");

            var output = new StringBuilder();
            foreach(var error in validator.Issues.Issues) 
            {
                if (error.Code != ApiDoctor.Validation.Error.ValidationErrorCode.Unknown)
                {
                    string message = string.Format("{1}: {0}", error.Message, error.Code);
                    output.Append(message);
                    output.Append("\n");
                }
            }

            try 
            {
                // Delete the localPath since we're all done with that data
                Console.WriteLine($"Deleting local files in {localPath}.");
                System.IO.Directory.Delete(localPath, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to clean up local files: {ex.Message}");
            }

            return $"Results for {request.GitUrl} on branch {request.Branch}:\n\n{output.ToString()}";
        }



    }

    public class BuildRequestModel {
        public string GitUrl { get; set; }
        public string Branch { get; set; } 

        public BuildRequestModel() {
            this.Branch = "master";
        }
    }
}
