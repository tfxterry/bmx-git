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
            var milestones = (JavaScriptArray)this.Invoke("GET", string.Format("https://api.github.com/repos/{0}/{1}/milestones", ownerName, repositoryName));

            int? milestoneNumber = milestones
                .Cast<JavaScriptObject>()
                .Where(m => string.Equals(m["title"].ToString(), milestone, StringComparison.OrdinalIgnoreCase))
                .Select(m => m["number"] as int?)
                .FirstOrDefault();

            if (milestoneNumber == null)
                return Enumerable.Empty<JavaScriptObject>();

            return this.EnumIssues((int)milestoneNumber, ownerName, repositoryName);
        }
        public IEnumerable<JavaScriptObject> EnumIssues(int milestoneNumber, string ownerName, string repositoryName)
        {
            var issues = (JavaScriptArray)this.Invoke("GET", string.Format("https://api.github.com/repos/{0}/{1}/issues?milestone={2}", ownerName, repositoryName, milestoneNumber));
            return issues.Cast<JavaScriptObject>();
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

            var milestones = (JavaScriptArray)this.Invoke("GET", url);

            int? milestoneNumber = milestones
                .Cast<JavaScriptObject>()
                .Where(m => string.Equals(m["title"].ToString(), milestone, StringComparison.OrdinalIgnoreCase))
                .Select(m => m["number"] as int?)
                .FirstOrDefault();

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

            var milestones = (JavaScriptArray)this.Invoke("GET", url);

            int? milestoneNumber = milestones
                .Cast<JavaScriptObject>()
                .Where(m => string.Equals(m["title"].ToString(), milestone, StringComparison.OrdinalIgnoreCase))
                .Select(m => m["number"] as int?)
                .FirstOrDefault();

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
