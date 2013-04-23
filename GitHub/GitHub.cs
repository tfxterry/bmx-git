using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Inedo.Linq;

namespace Inedo.BuildMasterExtensions.GitHub
{
    internal sealed class GitHub
    {
        public GitHub()
        {
        }

        public string OrganizationName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public IEnumerable<JavaScriptObject> EnumRepositories()
        {
            UriBuilder url;
            if (!string.IsNullOrEmpty(this.OrganizationName))
                url = new UriBuilder(string.Format("https://api.github.com/orgs/{0}/repos", HttpUtility.UrlEncode(this.OrganizationName)));
            else
                url = new UriBuilder("https://api.github.com/user/repos");

            url.UserName = this.UserName;
            url.Password = this.Password;

            var jsArray = (JavaScriptArray)this.Invoke("GET", url.ToString());
            return jsArray.Cast<JavaScriptObject>();
        }

        public IEnumerable<JavaScriptObject> EnumIssues(string milestone, string ownerName, string repositoryName)
        {
            int? milestoneNumber = this.FindMilestone(milestone, ownerName, repositoryName, false);

            if (milestoneNumber == null)
                return Enumerable.Empty<JavaScriptObject>();

            return this.EnumIssues((int)milestoneNumber, ownerName, repositoryName);
        }
        public IEnumerable<JavaScriptObject> EnumIssues(int milestoneNumber, string ownerName, string repositoryName)
        {
            var openIssues = (JavaScriptArray)this.Invoke("GET", string.Format("https://api.github.com/repos/{0}/{1}/issues?milestone={2}&state=open", ownerName, repositoryName, milestoneNumber));
            var closedIssues = (JavaScriptArray)this.Invoke("GET", string.Format("https://api.github.com/repos/{0}/{1}/issues?milestone={2}&state=closed", ownerName, repositoryName, milestoneNumber));
            return openIssues
                .Cast<JavaScriptObject>()
                .Concat(closedIssues.Cast<JavaScriptObject>());
        }

        public JavaScriptObject GetIssue(string issueId, string ownerName, string repositoryName)
        {
            return (JavaScriptObject)this.Invoke("GET", string.Format("https://api.github.com/repos/{0}/{1}/issues/{2}", ownerName, repositoryName, issueId));
        }
        public void UpdateIssue(string issueId, string ownerName, string repositoryName, object update)
        {
            this.Invoke("PATCH", string.Format("https://api.github.com/repos/{0}/{1}/issues/{2}", ownerName, repositoryName, issueId), update);
        }

        public void CreateMilestone(string milestone, string ownerName, string repositoryName)
        {
            var url = string.Format("https://api.github.com/repos/{0}/{1}/milestones", ownerName, repositoryName);
            int? milestoneNumber = this.FindMilestone(milestone, ownerName, repositoryName, false);
            if (milestoneNumber != null)
            {
                // Milestone already exists
                return;
            }

            this.Invoke(
                "POST",
                url,
                new { title = milestone }
            );
        }
        public void CloseMilestone(string milestone, string ownerName, string repositoryName)
        {
            var url = string.Format("https://api.github.com/repos/{0}/{1}/milestones", ownerName, repositoryName);
            int? milestoneNumber = this.FindMilestone(milestone, ownerName, repositoryName, false);
            if (milestoneNumber == null)
            {
                // Milestone not found
                return;
            }

            this.Invoke(
                "PATCH",
                url + "/" + milestoneNumber,
                new { state = "closed" }
            );
        }

        private int? FindMilestone(string title, string ownerName, string repositoryName, bool likelyClosed)
        {
            var openMilestones = this.EnumMilestones(ownerName, repositoryName, "open");
            var closedMilestones = this.EnumMilestones(ownerName, repositoryName, "closed");

            IEnumerable<JavaScriptObject> milestones;
            if (likelyClosed)
                milestones = closedMilestones.Concat(openMilestones);
            else
                milestones = openMilestones.Concat(closedMilestones);

            return milestones
                .Where(m => string.Equals(m["title"].ToString(), title, StringComparison.OrdinalIgnoreCase))
                .Select(m => m["number"] as int?)
                .FirstOrDefault();
        }
        private IEnumerable<JavaScriptObject> EnumMilestones(string ownerName, string repositoryName, string state)
        {
            // Implemented using an iterator just to make it lazy
            var milestones = (JavaScriptArray)this.Invoke("GET", string.Format("https://api.github.com/repos/{0}/{1}/milestones?state={2}", ownerName, repositoryName, state));
            foreach (JavaScriptObject obj in milestones)
                yield return obj;
        }

        private object Invoke(string method, string url)
        {
            return this.Invoke(method, url, null);
        }
        private object Invoke(string method, string url, object data)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = method;
            if (data != null)
            {
                using (var requestStream = request.GetRequestStream())
                using (var writer = new StreamWriter(requestStream, new UTF8Encoding(false)))
                {
                    Inedo.InedoLib.Util.JavaScript.WriteJson(writer, data);
                }
            }

            if (!string.IsNullOrEmpty(this.UserName))
                request.Headers[HttpRequestHeader.Authorization] = "basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(this.UserName + ":" + this.Password));

            try
            {
                using (var response = request.GetResponse())
                using (var responseStream = response.GetResponseStream())
                {
                    return JsonReader.ParseJson(new StreamReader(responseStream).ReadToEnd());
                }
            }
            catch (WebException ex)
            {
                using (var responseStream = ex.Response.GetResponseStream())
                {
                    throw new Exception(new StreamReader(responseStream).ReadToEnd());
                }
            }
        }
    }
}
