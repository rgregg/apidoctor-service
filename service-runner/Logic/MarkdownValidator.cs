using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiDoctor.Validation;
using ApiDoctor.Validation.Error;
using ApiDoctor.Validation.Http;
using ApiDoctor.Validation.Json;
using ApiDoctor.Validation.OData;
using ApiDoctor.Validation.Params;
using ApiDoctor.Validation.Writers;

namespace service_runner.Logic
{
    public class MarkdownValidator
    {
        private readonly string pathToDocs;
        private DocSet docs;
        private IssueLogger issues;

        public MarkdownValidator(string pathToDocs) 
        {
            this.pathToDocs = pathToDocs;
            this.issues = new IssueLogger();
        }

        public void EnsureDocSet()
        {
            if (docs == null) {
                docs = new DocSet(this.pathToDocs);
                docs.ScanDocumentation(String.Empty, issues);
            }
        }

        public void ClearExistingIssues() {
            issues = new IssueLogger();
        }

        public bool CheckForBrokenLinks() {
            EnsureDocSet();

            if (docs == null)
            {
                Console.WriteLine("DocSet does not exist, aborting.");
                return false;
            }

            

            Console.WriteLine("Checking for broken links...");
            docs.ValidateLinks(includeWarnings: false, 
                               relativePathForFiles: null, 
                               issues: issues,
                               requireFilenameCaseMatch: true,
                               printOrphanedFiles: true );

            return issues.Errors.Any();
        }

        public IssueLogger Issues 
        {
            get {
                return issues;
            }
        }

    }
}