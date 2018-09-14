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
        public ActionResult Post(BuildRequestModel request)
        {
            var requestStart = DateTime.UtcNow;

            if (request == null)
            {
                return BadRequest(new { error = "No build request was detected."});
            }

            if (String.IsNullOrEmpty(request.GitUrl))
            {
                return BadRequest(new { error = "No Repo URL was received."});
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

            var outputResults = from e in validator.Issues.Issues
                                where e.Code != ApiDoctor.Validation.Error.ValidationErrorCode.Unknown
                                select new { e.Code, e.Message, e.Source };

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

            var requestCompleted = DateTime.UtcNow;
            var duration = requestCompleted.Subtract(requestStart);

            return Ok(new {
                contentIssues = outputResults,
                duration = duration,
                branch = request.Branch,
                repo = request.GitUrl
            });
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
