using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Sturfee.XRCS.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Sturfee.XRCS
{
    public delegate void XrUploadEvent(float progress);
    public interface IProjectProvider
    {
        event XrUploadEvent OnProjectAssetUploadProgress;

        Task<List<XrProjectData>> FindXrProjects();
        Task<List<XrProjectData>> GetXrProjects();
        Task<List<XrProjectData>> GetLocalXrProjects();
        Task<XrProjectData> GetXrProject(Guid id);
        Task<XrProjectData> GetPublicXrProject(Guid id);
        Task SaveXrProject(XrProjectData data);
        Task PublishProject(XrProjectData project);
        Task UnPublishProject(XrProjectData project);
        Task DownloadProject(XrProjectData project);
        Task DownloadProjectAssets(Guid projectId, List<XrProjectAssetData> projectAssets);

        Task DeleteProject(XrProjectData project);

        //void RemoveLocalProject(XrProjectData project);
        Task RemoveLocalProject(XrProjectData project);

        // scenes
        Task SaveXrScene(XrSceneData data);
        Task<bool> DeleteXrScene(XrSceneData data);
        Task<XrSceneData> GetXrScene(Guid projectId, Guid sceneId);

        // project assets
        Task SaveProjectAsset(XrProjectAssetData xrProjectAsset);
        Task<List<XrProjectAssetData>> GetProjectAssets(Guid projectId);
        Task DeleteProjectAsset(Guid projectId, Guid projectAssetId);


        // scene assets
        Task<XrSceneAssetData> SaveSceneAsset(XrSceneAssetData xrSceneAsset);
        Task<List<XrSceneAssetData>> GetSceneAssets(Guid sceneId);
        Task DeleteSceneAsset(Guid sceneId, Guid sceneAssetId);

    }

    public class WebProjectProvider : IProjectProvider
    {
        public event XrUploadEvent OnProjectAssetUploadProgress;

        // TODO: move these to manager or cache class?
        public List<XrProjectData> Projects = new List<XrProjectData>();
        public List<XrProjectData> LocalProjects = new List<XrProjectData>();

        public List<XrSceneData> Scenes = new List<XrSceneData>();

        private string _bucketName = "xrcs-projects";
        private string _localProjectsPath;

        public WebProjectProvider()
        {
            _localProjectsPath = $"{Application.persistentDataPath}/{XrConstants.LOCAL_PROJECTS_PATH}";
        }

        public async Task<List<XrProjectData>> GetXrProjects()
        {
            var currentAccountId = IOC.Resolve<IAuthProvider>().CurrentUser.AccountId;

            //MyLogger.Log($"Getting projects... {XrConstants.XRCS_API}/accounts/{currentAccountId}/projects");
            MyLogger.Log($"Getting projects... {XrConstants.XRCS_API}/projects?accountId={currentAccountId}");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{XrConstants.XRCS_API}/projects?accountId={currentAccountId}");
            request.Method = "GET";
            AuthHelper.AddXrcsTokenAuthHeader(request);

            //HttpWebResponse response;
            try
            {
                MyLogger.Log($"   Waiting for response...");
                using (var response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    //if (response.StatusCode != HttpStatusCode.OK)
                    //{
                    //    Debug.LogError($"ERROR:: API => {response.StatusCode} - {response.StatusDescription}");
                    //    return new List<XrProjectData>();
                    //}

                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string jsonResponse = reader.ReadToEnd();
                        MyLogger.Log($"Projects from API:\n{jsonResponse}");
                        Projects = JsonConvert.DeserializeObject<List<XrProjectData>>(jsonResponse);
                    }
                }
            }
            catch (WebException e)
            {
                Debug.LogError(e);

                if (e.Message.ToLower().Contains("timeout"))
                {
                    return null;
                }

                using (WebResponse res = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)res;

                    Debug.LogError(e);
                    Debug.LogError($"ERROR:: API => {httpResponse.StatusCode} - {httpResponse.StatusDescription}");

                    if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Debug.LogWarning($"UNAUTHORIZED. LOGGING OUT...");
                        var userProvider = IOC.Resolve<IAuthProvider>();
                        userProvider.Logout();
                        //SimpleSceneLoader.Instance.LoadScene(SceneName.Login);
                        return new List<XrProjectData>();
                    }

                    try
                    {
                        using (Stream data = res.GetResponseStream())
                        using (var r = new StreamReader(data))
                        {
                            // read response body
                            if (data != null && r != null)
                            {
                                string text = r.ReadToEnd();
                                Debug.LogError($"   ERROR :: Message = {text}");
                            }
                        }
                    }
                    catch (WebException ex)
                    {
                        // Oh, well, we tried
                    }

                    return new List<XrProjectData>();
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }

            return Projects;

        }

        public async Task<List<XrProjectData>> FindXrProjects()
        {
            var accountIds = IOC.Resolve<IAuthProvider>().CurrentUser.AccountIds;

            //MyLogger.Log($"Getting projects... {XrConstants.XRCS_API}/accounts/{currentAccountId}/projects");
            MyLogger.Log($"Finding projects... {XrConstants.XRCS_API}/projects/find \n{JsonUtility.ToJson(accountIds)}");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{XrConstants.XRCS_API}/projects/find");
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";
            AuthHelper.AddXrcsTokenAuthHeader(request);

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                string json = JsonConvert.SerializeObject(new XrProjectFilter
                {
                    AccountIds = accountIds
                });

                streamWriter.Write(json);
                streamWriter.Flush();
            }

            //HttpWebResponse response;
            try
            {
                MyLogger.Log($"   Waiting for response...");
                using (var response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    //if (response.StatusCode != HttpStatusCode.OK)
                    //{
                    //    Debug.LogError($"ERROR:: API => {response.StatusCode} - {response.StatusDescription}");
                    //    return new List<XrProjectData>();
                    //}

                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string jsonResponse = reader.ReadToEnd();
                        MyLogger.Log($"Projects from API:\n{jsonResponse}");
                        Projects = JsonConvert.DeserializeObject<List<XrProjectData>>(jsonResponse);
                    }
                }
            }
            catch (WebException e)
            {
                Debug.LogError(e);

                if (e.Message.ToLower().Contains("timeout"))
                {
                    return null;
                }

                using (WebResponse res = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)res;

                    Debug.LogError(e);
                    Debug.LogError($"ERROR:: API => {httpResponse.StatusCode} - {httpResponse.StatusDescription}");

                    if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Debug.LogWarning($"UNAUTHORIZED. LOGGING OUT...");
                        var userProvider = IOC.Resolve<IAuthProvider>();
                        userProvider.Logout();
                        //SimpleSceneLoader.Instance.LoadScene(SceneName.Login);
                        return new List<XrProjectData>();
                    }

                    try
                    {
                        using (Stream data = res.GetResponseStream())
                        using (var r = new StreamReader(data))
                        {
                            // read response body
                            if (data != null && r != null)
                            {
                                string text = r.ReadToEnd();
                                Debug.LogError($"   ERROR :: Message = {text}");
                            }
                        }
                    }
                    catch (WebException ex)
                    {
                        // Oh, well, we tried
                    }

                    return new List<XrProjectData>();
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }

            return Projects;
        }

        public async Task DeleteProject(XrProjectData project)
        {
            // delete metadata from DB
            MyLogger.Log($"Deleting Project... {XrConstants.XRCS_API}/projects/{project.Id}/destroy");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{XrConstants.XRCS_API}/projects/{project.Id}/destroy");
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";
            request.ContentLength = 0;
            AuthHelper.AddXrcsTokenAuthHeader(request);

            try
            {
                using (var response = await request.GetResponseAsync() as HttpWebResponse)
                {

                }
            }
            catch (WebException e)
            {
                using (WebResponse res = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)res;

                    Debug.LogError($"ERROR:: API => {httpResponse.StatusCode} - {httpResponse.StatusDescription}");

                    try
                    {
                        using (Stream data = res.GetResponseStream())
                        using (var r = new StreamReader(data))
                        {
                            // read response body
                            if (data != null && r != null)
                            {
                                string text = r.ReadToEnd();
                                Debug.LogError($"   ERROR :: Message = {text}");
                            }
                        }
                    }
                    catch (WebException ex) { /* Oh, well, we tried */ }
                }

                MyLogger.LogError($"Error deleting project. Please try again.");
                Debug.LogError(e);
                throw;
            }

            // delete from local data
            try
            {
                MyLogger.Log($"Deleting Local Project Data...");

                var existingProject = Projects.FirstOrDefault(p => p.Id == project.Id);
                if (existingProject != null)
                {
                    Projects.Remove(existingProject); // add
                }
                var existingLocalProject = LocalProjects.FirstOrDefault(p => p.Id == project.Id);
                if (existingLocalProject != null)
                {
                    LocalProjects.Remove(existingLocalProject); // add
                }
                await SaveXrProjects(); // save changes to projects cache

                // TODO: delete all relevant files like thumbnails, zips, etc
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        public async Task DeleteProjectAsset(Guid projectId, Guid projectAssetId)
        {
            MyLogger.Log($"Deleting project asset on server: \n {XrConstants.XRCS_API}/projects/{projectId}/assets/{projectAssetId}/destroy");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{XrConstants.XRCS_API}/projects/{projectId}/assets/{projectAssetId}/destroy");
            request.Method = "POST";
            request.ContentLength = 0;
            AuthHelper.AddXrcsTokenAuthHeader(request);

            try
            {
                using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        MyLogger.Log($"Successfully Deleted Project Asset! (ID={projectAssetId})");
                    }
                }
            }
            catch (WebException ex)
            {
                Debug.LogError(ex);
                throw;
            }

            // TODO: remove from local cache

            var projectAssetPath = $"{Application.persistentDataPath}/{XrConstants.LOCAL_PROJECTS_PATH}/{projectId}/Assets/{projectAssetId}";

            if (Directory.Exists(projectAssetPath))
            {
                Directory.Delete(projectAssetPath, true);
            }
            else
            {
                Debug.LogError($"ERROR: NOT FOUND => Project Asset data not found. pId={projectId} | paId={projectAssetId}");
            }
        }

        public async Task DeleteSceneAsset(Guid sceneId, Guid sceneAssetId)
        {
            MyLogger.Log($"Deleting scene asset on server: \n {XrConstants.XRCS_API}/scenes/{sceneId}/assets/{sceneAssetId}/destroy");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{XrConstants.XRCS_API}/scenes/{sceneId}/assets/{sceneAssetId}/destroy");
            request.Method = "POST";
            request.ContentLength = 0;
            AuthHelper.AddXrcsTokenAuthHeader(request);

            try
            {
                using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        MyLogger.Log($"Successfully Deleted Scene Asset! (ID={sceneAssetId})");
                    }
                }
            }
            catch (WebException ex)
            {
                Debug.LogError(ex);
                throw;
            }

            // TODO: remove from local cache
        }

        public async Task<bool> DeleteXrScene(XrSceneData data)
        {
            MyLogger.Log($"Deleting scene on server: \n {XrConstants.XRCS_API}/projects/{data.ProjectId}/scenes/{data.Id}/destroy");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{XrConstants.XRCS_API}/projects/{data.ProjectId}/scenes/{data.Id}/destroy");
            request.Method = "POST";
            request.ContentLength = 0;
            AuthHelper.AddXrcsTokenAuthHeader(request);

            try
            {
                using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        MyLogger.Log($"Successfully Deleted Scene! (ID={data.Id})");
                        return true;
                    }
                }
            }
            catch (WebException ex)
            {
                Debug.LogError(ex);
                throw;
            }
            
            // TODO: remove from local cache
        }

        public async Task DownloadProject(XrProjectData project)
        {
            if (!Directory.Exists($"{_localProjectsPath}/{project.Id}/Assets/"))
            {
                Directory.CreateDirectory($"{_localProjectsPath}/{project.Id}/Assets/");
            }

            //var errors = new List<string>();

            var projectAssets = await GetProjectAssets(project.Id);

            await DownloadProjectAssets(project.Id, projectAssets);

            // add the project to the local json file (cache)
            var foundProject = Projects.FirstOrDefault(x => x.Id == project.Id);
            if (foundProject == null) { Projects.Add(project); }
            else { Projects[Projects.IndexOf(foundProject)] = project; }

            var foundLocalProject = LocalProjects.FirstOrDefault(x => x.Id == project.Id);
            if (foundLocalProject == null) { LocalProjects.Add(project); }
            else { LocalProjects[LocalProjects.IndexOf(foundLocalProject)] = project; }

            await SaveXrProjects();
        }

        public async Task DownloadProjectAssets(Guid projectId, List<XrProjectAssetData> projectAssets)
        {
            if (!Directory.Exists($"{_localProjectsPath}/{projectId}/Assets/"))
            {
                Directory.CreateDirectory($"{_localProjectsPath}/{projectId}/Assets/");
            }

            var errors = new List<string>();

            var downloadTasks = new List<Task>();
            // download all project assets from the sever and unzip them
            foreach (var projectAsset in projectAssets)
            {
                if (projectAsset.TemplateType == TemplateAssetType.Image)
                {
                    continue;
                }
                if (projectAsset.TemplateType == TemplateAssetType.Billboard)
                {
                    continue;
                }

                var projectAssetId = projectAsset.Id;

                // get download URL
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{XrConstants.XRCS_API}/projects/{projectId}/assets/download_url?projectAssetId={projectAssetId}");
                AuthHelper.AddXrcsTokenAuthHeader(request);

                string url;
                try
                {
                    using (var response = await request.GetResponseAsync() as HttpWebResponse)
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            Debug.LogError($"ERROR:: API => {response.StatusCode} - {response.StatusDescription}");
                        }

                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            url = reader.ReadToEnd();
                            MyLogger.Log($"Download Asset Response from API:\n{url}");
                        }
                    }

                }
                catch (WebException ex)
                {
                    Debug.LogError(ex);
                    throw;
                }

                // download and unzip file
                var newTask = Task.Run(async () =>
                {
                    using (var client = new WebClient())
                    {
                        try
                        {
                            await client.DownloadFileTaskAsync(url, $"{_localProjectsPath}/{projectId}/Assets/{projectAssetId}.zip");

                            FastZip fz = new FastZip();
                            //fz.ExtractZip(zipFilePath, extractionPath, null);                                

                            fz.ExtractZip($"{_localProjectsPath}/{projectId}/Assets/{projectAssetId}.zip", $"{_localProjectsPath}/{projectId}/Assets/{projectAssetId}/", null);

                            // TODO: clean up (delete) zip files after unzipping

                            // delete ZIP file
                            File.Delete($"{_localProjectsPath}/{projectId}/Assets/{projectAssetId}.zip");
                        }
                        catch (WebException wex)
                        {
                            Debug.LogError(wex);
                            Debug.LogError($"Unable to Download Asset with ID = {projectAssetId} (project id = {projectId})");

                            errors.Add($"Unable to Download Asset with ID = {projectAssetId} (project id = {projectId})");

                            //ToastManager.Instance.ShowToastWithButton($"One or more assets could not be loaded.");
                        }
                    }
                });

                downloadTasks.Add(newTask);
            }

            await Task.WhenAll(downloadTasks);

            if (errors.Count > 0)
            {
                MyLogger.LogError( $"One or more assets could not be loaded.");
            }
        }
        

        public async Task<List<XrProjectData>> GetLocalXrProjects()
        {
            if (!Directory.Exists(_localProjectsPath))
            {
                Directory.CreateDirectory(_localProjectsPath);
            }

            LocalProjects = new List<XrProjectData>();

            MyLogger.Log($"Loading local projects ...");

            var json = await SimpleJsonIO.ReadJsonFileAsync(_localProjectsPath, "projects");

            if (!string.IsNullOrEmpty(json))
            {
                LocalProjects = JsonConvert.DeserializeObject<List<XrProjectData>>(json);
            }

            //if (!Directory.Exists(_localProjectsPath))
            //{
            //    Directory.CreateDirectory(_localProjectsPath);
            //}
            //else
            //{
            //    var projectsDirs = Directory.GetDirectories(_localProjectsPath);

            //    foreach (var projectDir in projectsDirs)
            //    {
            //        var projectPath = $"{projectDir}/project";

            //        if (!File.Exists(projectPath)) { continue; }

            //        using (StreamReader r = new StreamReader(projectPath))
            //        {
            //            string json = await r.ReadToEndAsync();
            //            var data = JsonConvert.DeserializeObject<XrProjectData>(json);
            //            LocalProjects.Add(data);
            //        }
            //    }
            //}

            return LocalProjects;
        }

        public async Task<List<XrProjectAssetData>> GetProjectAssets(Guid projectId)
        {
            MyLogger.Log($"Getting project assets... {XrConstants.XRCS_API}/projects/{projectId}/assets");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{XrConstants.XRCS_API}/projects/{projectId}/assets");
            AuthHelper.AddXrcsTokenAuthHeader(request);

            try
            {
                using (var response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        Debug.LogError($"ERROR:: API => {response.StatusCode} - {response.StatusDescription}");
                        return null;
                    }

                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string jsonResponse = reader.ReadToEnd();
                        MyLogger.Log($"Project Assets from API:\n{jsonResponse}");
                        var projectAssets = JsonConvert.DeserializeObject<List<XrProjectAssetData>>(jsonResponse);
                        return projectAssets;
                    }


                    // TODO: move this to local cache

                    //var scenePath = $"{_localProjectsPath}/{projectId}/Scenes";
                    //scenePath = scenePath.Replace(@"//", @"/");

                    //if (!Directory.Exists(scenePath)) { MyLogger.Log($"No scenes for project (pId={projectId})"); return null; }
                    //if (!File.Exists($"{scenePath}/scene_{sceneId}")) { MyLogger.Log($"No scene metadata found at {scenePath} (sId={sceneId})"); return null; }

                    //using (StreamReader r = new StreamReader($"{scenePath}/scene_{sceneId}", true))
                    //{
                    //    string json = await r.ReadToEndAsync(); //.ReadToEnd();
                    //                                            //var data = JsonUtility.FromJson<XrProjectData>(json);
                    //    var data = JsonConvert.DeserializeObject<XrSceneData>(json);
                    //    return data;
                    //}
                }
            }
            catch (WebException ex)
            {
                Debug.LogError($"ERROR:: API => \n" + ex);
                throw;
            }
        }

        public async Task<XrProjectData> GetPublicXrProject(Guid id)
        {
            MyLogger.Log($"Getting projects... {XrConstants.XRCS_API}/projects/{id}");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{XrConstants.XRCS_API}/projects/{id}");
            request.Method = "GET";
            //await AuthHelper.AddXrcsTokenAuthHeader(request);

            //HttpWebResponse response;
            try
            {
                MyLogger.Log($"   Waiting for response...");
                using (var response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    //if (response.StatusCode != HttpStatusCode.OK)
                    //{
                    //    Debug.LogError($"ERROR:: API => {response.StatusCode} - {response.StatusDescription}");
                    //    return new List<XrProjectData>();
                    //}

                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string jsonResponse = reader.ReadToEnd();
                        MyLogger.Log($"Projects from API:\n{jsonResponse}");

                        if (string.IsNullOrEmpty(jsonResponse))
                        {
                            Debug.LogError($"Could not get project from server with ID={id}");
                            return null;
                        }

                        var xrProject = JsonConvert.DeserializeObject<XrProjectData>(jsonResponse);
                        return xrProject;
                    }
                }
            }
            catch (WebException e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        public async Task<List<XrSceneAssetData>> GetSceneAssets(Guid sceneId)
        {
            MyLogger.Log($"Getting scene assets... {XrConstants.XRCS_API}/scenes/{sceneId}/assets");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{XrConstants.XRCS_API}/scenes/{sceneId}/assets");
            AuthHelper.AddXrcsTokenAuthHeader(request);

            try
            {
                using (var response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        Debug.LogError($"ERROR:: API => {response.StatusCode} - {response.StatusDescription}");
                        return null;
                    }

                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string jsonResponse = reader.ReadToEnd();
                        MyLogger.Log($"Scene Assets from API:\n{jsonResponse}");
                        var sceneAssets = JsonConvert.DeserializeObject<List<XrSceneAssetData>>(jsonResponse);
                        return sceneAssets;
                    }


                    // TODO: save to local cache
                }
            }
            catch (WebException ex)
            {
                Debug.LogError($"ERROR:: API => \n" + ex);
                throw;
            }
        }

        public async Task<XrProjectData> GetXrProject(Guid id)
        {
            var projectPath = $"{_localProjectsPath}/{id}";
            projectPath = projectPath.Replace(@"//", @"/");

            if (!Directory.Exists(projectPath)) { MyLogger.Log($"No project at {projectPath}"); return null; }

            var foundProject = Projects.FirstOrDefault(x => x.Id == id);

            if (foundProject == null)
            {
                // check the server
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{XrConstants.XRCS_API}/projects/{id}");
                AuthHelper.AddXrcsTokenAuthHeader(request);

                try
                {
                    using (HttpWebResponse xrcsUserResponse = await request.GetResponseAsync() as HttpWebResponse)
                    {
                        using (StreamReader reader = new StreamReader(xrcsUserResponse.GetResponseStream()))
                        {
                            string jsonResponse = reader.ReadToEnd();

                            MyLogger.Log($"Project from API:\n{jsonResponse}");

                            foundProject = JsonConvert.DeserializeObject<XrProjectData>(jsonResponse);

                            Projects.Add(foundProject);
                            await SaveXrProjects(); // save changes to projects cache
                        }
                    }
                }
                catch (WebException ex)
                {
                    Debug.LogError(ex);
                    throw;
                }
            }

            if (foundProject == null)
            {
                MyLogger.LogError($"Error loading project. Please try again.");
                return null;
            }

            return foundProject;
        }

        

        public async Task<XrSceneData> GetXrScene(Guid projectId, Guid sceneId)
        {
            //MyLogger.Log($"Getting projects... {XrConstants.XRCS_API}/accounts/{currentAccountId}/projects");
            MyLogger.Log($"Getting scene... {XrConstants.XRCS_API}/projects/{projectId}/scenes/{sceneId}");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{XrConstants.XRCS_API}/projects/{projectId}/scenes/{sceneId}");
            //await AuthHelper.AddXrcsTokenAuthHeader(request);

            try
            {
                using (var response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        Debug.LogError($"ERROR:: API => {response.StatusCode} - {response.StatusDescription}");

                        return null;
                    }

                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string jsonResponse = reader.ReadToEnd();

                        MyLogger.Log($"Scene from API:\n{jsonResponse}");

                        var scene = JsonConvert.DeserializeObject<XrSceneData>(jsonResponse);

                        Scenes.Add(scene);

                        return scene;
                    }


                    // TODO: move this to local cache

                    //var scenePath = $"{_localProjectsPath}/{projectId}/Scenes";
                    //scenePath = scenePath.Replace(@"//", @"/");

                    //if (!Directory.Exists(scenePath)) { MyLogger.Log($"No scenes for project (pId={projectId})"); return null; }
                    //if (!File.Exists($"{scenePath}/scene_{sceneId}")) { MyLogger.Log($"No scene metadata found at {scenePath} (sId={sceneId})"); return null; }

                    //using (StreamReader r = new StreamReader($"{scenePath}/scene_{sceneId}", true))
                    //{
                    //    string json = await r.ReadToEndAsync(); //.ReadToEnd();
                    //                                            //var data = JsonUtility.FromJson<XrProjectData>(json);
                    //    var data = JsonConvert.DeserializeObject<XrSceneData>(json);
                    //    return data;
                    //}
                }
            }
            catch (WebException ex)
            {
                Debug.LogError($"ERROR:: API => " + ex);
                throw;
            }
        }

        public async Task PublishProject(XrProjectData project)
        {
            MyLogger.Log($"Publishing Project... {XrConstants.XRCS_API}/projects/{project.Id}");

            var currentAccountId = IOC.Resolve<IAuthProvider>().CurrentUser.AccountId;

            //ToastManager.Instance.ShowToast($"Publishing Project...", -1);

            project.IsPublished = true;
            project.PublishedDate = DateTime.UtcNow;

            project.SceneIds = project.SceneIds.Distinct().ToList();
            project.AssetIds = project.SceneIds.Distinct().ToList();
            project.ProjectAssetIds = project.SceneIds.Distinct().ToList();


            await SaveXrProject(project);

            MyLogger.Log($"   DONE!");

        }

        public async Task RemoveLocalProject(XrProjectData project)
        {
            // delete folder
            var projectPath = $"{_localProjectsPath}/{project.Id}";
            if (Directory.Exists(projectPath))
            {
                Directory.Delete(projectPath, true);
            }

            // delete zip file
            if (File.Exists($"{projectPath}.zip"))
            {
                File.Delete($"{projectPath}.zip");
            }

            var foundProject = Projects.FirstOrDefault(x => x.Id == project.Id);
            if (foundProject != null) { Projects.Remove(foundProject); MyLogger.Log($"Removed project from Projects"); }

            var foundLocalProject = LocalProjects.FirstOrDefault(x => x.Id == project.Id);
            if (foundLocalProject != null) { LocalProjects.Remove(foundLocalProject); MyLogger.Log($"Removed project from LocalProjects"); }

            var json = JsonConvert.SerializeObject(Projects);
            MyLogger.Log($"   Current Projects:\n{json}");
            json = JsonConvert.SerializeObject(LocalProjects);
            MyLogger.Log($"   Current Local Projects:\n{json}");

            await SaveXrProjects(); // save changes to projects cache
        }

        public async Task SaveProjectAsset(XrProjectAssetData xrProjectAsset)
        {
            // TODO: remove this when using an Asset uploaded (i.e. from the Asset Library and not from local files)
            xrProjectAsset.AssetId = null;

            string json = JsonConvert.SerializeObject(xrProjectAsset);

            MyLogger.Log($"Saving project asset to server: \n {XrConstants.XRCS_API}/projects/{xrProjectAsset.ProjectId}/assets/save \n {json}");

            xrProjectAsset.ModifiedDate = DateTime.UtcNow;

            // save the metadata
            HttpWebRequest xrcsUserRequest = (HttpWebRequest)WebRequest.Create($"{XrConstants.XRCS_API}/projects/{xrProjectAsset.ProjectId}/assets/save");
            xrcsUserRequest.Method = "POST";
            xrcsUserRequest.ContentType = "application/json; charset=utf-8";
            AuthHelper.AddXrcsTokenAuthHeader(xrcsUserRequest);

            using (var streamWriter = new StreamWriter(await xrcsUserRequest.GetRequestStreamAsync()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
            }

            try
            {
                using (HttpWebResponse response = await xrcsUserRequest.GetResponseAsync() as HttpWebResponse)
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string jsonResponse = reader.ReadToEnd();
                        MyLogger.Log($"Project Asset from API:\n{jsonResponse}");
                        //var projectAsset = JsonConvert.DeserializeObject<XrProjectAssetData>(jsonResponse);
                    }
                }
            }
            catch (WebException ex)
            {
                Debug.LogError(ex);
                throw;
            }

            // upload the thumbnail
            var thumbnailProvider = IOC.Resolve<IThumbnailProvider>();
            await thumbnailProvider.SaveThumbnail(xrProjectAsset.Id);
        }

        public async Task<XrSceneAssetData> SaveSceneAsset(XrSceneAssetData xrSceneAsset)
        {
            string json = JsonConvert.SerializeObject(xrSceneAsset);

            MyLogger.Log($"Saving scene asset to server: \n {XrConstants.XRCS_API}/scenes/{xrSceneAsset.SceneId}/assets/save \n {json}");

            xrSceneAsset.ModifiedDate = DateTime.UtcNow;

            // save the metadata
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{XrConstants.XRCS_API}/scenes/{xrSceneAsset.SceneId}/assets/save");
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";
            AuthHelper.AddXrcsTokenAuthHeader(request);

            using (var streamWriter = new StreamWriter(await request.GetRequestStreamAsync()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
            }

            try
            {
                using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string jsonResponse = reader.ReadToEnd();
                        MyLogger.Log($"Scene Asset from API:\n{jsonResponse}");
                        var sceneAsset = JsonConvert.DeserializeObject<XrSceneAssetData>(jsonResponse);
                        return sceneAsset;
                    }
                }
            }
            catch (WebException ex)
            {
                Debug.LogError(ex);
                throw;
            }

            // TODO: save to local cache
        }

        public async Task SaveXrProject(XrProjectData data)
        {
            MyLogger.Log($"Saving project...\n{JsonUtility.ToJson(data)}");

            var projectPath = $"{_localProjectsPath}/{data.Id}";
            projectPath = projectPath.Replace(@"//", @"/");
            if (!Directory.Exists(projectPath)) { Directory.CreateDirectory(projectPath); }

            //ToastManager.Instance.ShowToast($"Saving Project...", -1);

            data.ModifiedDate = DateTime.UtcNow;

            string json = JsonConvert.SerializeObject(data);

            HttpWebRequest xrcsUserRequest = (HttpWebRequest)WebRequest.Create($"{XrConstants.XRCS_API}/projects/save");
            xrcsUserRequest.Method = "POST";
            xrcsUserRequest.ContentType = "application/json; charset=utf-8";
            AuthHelper.AddXrcsTokenAuthHeader(xrcsUserRequest);

            using (var streamWriter = new StreamWriter(await xrcsUserRequest.GetRequestStreamAsync()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
            }

            XrProjectData projectResponse;
            try
            {
                using (HttpWebResponse xrcsUserResponse = await xrcsUserRequest.GetResponseAsync() as HttpWebResponse)
                {
                    using (StreamReader reader = new StreamReader(xrcsUserResponse.GetResponseStream()))
                    {
                        string jsonResponse = reader.ReadToEnd();

                        MyLogger.Log($"Project from Save API:\n{jsonResponse}");

                        projectResponse = JsonConvert.DeserializeObject<XrProjectData>(jsonResponse);
                    }
                }
            }
            catch (WebException ex)
            {
                Debug.LogError(ex);
                throw;
            }


            // save filesystem cache
            MyLogger.Log($"   (before) Projects List size = {Projects.Count}");
            var existingProject = Projects.FirstOrDefault(p => p.Id == data.Id);
            if (existingProject == null)
            {
                Projects.Add(projectResponse); // add
            }
            else
            {
                // update
                //existingProject = data;
                Projects[Projects.IndexOf(existingProject)] = projectResponse;

                //Projects.Remove(existingProject);
                //Projects.Add(data);                
            }

            MyLogger.Log($"   (after) Projects List size = {Projects.Count}");
            await SaveXrProjects(); // save changes to projects cache
        }

        public async Task SaveXrScene(XrSceneData data)
        {
            MyLogger.Log($"Saving Scene ...");

            data.ModifiedDate = DateTime.UtcNow;

            string json = JsonConvert.SerializeObject(data);

            MyLogger.Log($"   Saving Scene to server: {XrConstants.XRCS_API}/projects/{data.ProjectId}/scenes/save \n {json}");

            HttpWebRequest xrcsUserRequest = (HttpWebRequest)WebRequest.Create($"{XrConstants.XRCS_API}/projects/{data.ProjectId}/scenes/save");
            xrcsUserRequest.Method = "POST";
            xrcsUserRequest.ContentType = "application/json; charset=utf-8";
            AuthHelper.AddXrcsTokenAuthHeader(xrcsUserRequest);

            using (var streamWriter = new StreamWriter(await xrcsUserRequest.GetRequestStreamAsync()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
            }

            XrSceneData scene;
            try
            {
                using (HttpWebResponse xrcsUserResponse = await xrcsUserRequest.GetResponseAsync() as HttpWebResponse)
                {
                    using (StreamReader reader = new StreamReader(xrcsUserResponse.GetResponseStream()))
                    {
                        string jsonResponse = reader.ReadToEnd();

                        MyLogger.Log($"Scene from API:\n{jsonResponse}");

                        scene = JsonConvert.DeserializeObject<XrSceneData>(jsonResponse);
                    }
                }
            }
            catch (WebException ex)
            {
                Debug.LogError(ex.Message);
                Debug.LogError(ex);
                throw;
            }

            // TODO: save to local cache

        }

        public async Task UnPublishProject(XrProjectData project)
         {
            MyLogger.Log($"UN-Publishing Project... {XrConstants.XRCS_API}/projects/{project.Id}");

            project.IsPublished = false;

            project.SceneIds = project.SceneIds.Distinct().ToList();
            project.AssetIds = project.SceneIds.Distinct().ToList();
            project.ProjectAssetIds = project.SceneIds.Distinct().ToList();

            await SaveXrProject(project);

            MyLogger.Log($"   DONE!");
         }


        private async Task SaveXrProjects()
        {
            MyLogger.Log($"Saving all projects ...");
            //var json = JsonConvert.SerializeObject(Projects.Distinct());
            var json = JsonConvert.SerializeObject(LocalProjects.Distinct());
            await SimpleJsonIO.SaveJsonFileAsync(_localProjectsPath, "projects", json);
        }

        private void HandleUploaderUpdated(float progress)
        {
            OnProjectAssetUploadProgress?.Invoke(progress);
        }
    }

    public class Uploader
    {
        public event XrUploadEvent OnProjectAssetUploadProgress;

        private Dictionary<Guid, int> _uploadMap = new Dictionary<Guid, int>();

        private int _currentProgress = 0;

        private static readonly HttpClient _httpClient = new HttpClient();

        public Uploader()
        {
            _httpClient.Timeout = TimeSpan.FromMinutes(10);
        }

        public async void UploadFile(Guid id, string url, string fileName)
        {
            MyLogger.Log($"Uploading File to URL: {url}");
            using (FileStream fileToUpload = File.OpenRead(fileName))
            {
                using (HttpContent content = new StreamContent(fileToUpload))
                {
                    //HttpRequestMessage msg = new HttpRequestMessage
                    //{
                    //    Content = content,
                    //    RequestUri = url,
                    //    Method = HttpMethod.Put
                    //};

                    bool keepTracking = true; //to start and stop the tracking thread
                    new Task(() => { progressTracker(fileToUpload, ref keepTracking); }).Start();
                    //var result = await _httpClient.SendAsync(msg);
                    var result = await _httpClient.PutAsync(url, content);
                    keepTracking = false; //stops the tracking thread
                }
            }
        }

        private void progressTracker(FileStream streamToTrack, ref bool keepTracking)
        {
            int prevPos = -1;
            while (keepTracking)
            {
                int pos = (int)Math.Round(100 * (streamToTrack.Position / (double)streamToTrack.Length));
                if (pos != prevPos)
                {
                    MyLogger.Log(pos + "%");
                }
                prevPos = pos;

                Thread.Sleep(1000); //only update progress every 100ms 
            }
        }

        //public void UploadFile(Guid id, Uri url, string fileName)
        //{
        //    var tcs = new TaskCompletionSource<bool>();
        //    using (var client = new WebClient())
        //    {
        //        try
        //        {
        //            //var task = AuthHelper.AddXrcsTokenAuthHeader(client);
        //            //task.Wait();

        //            //MyLogger.Log($"WebClient Auth = {client.Headers[HttpRequestHeader.Authorization]}");

        //            client.Headers.Add("Content-Type", "application/zip");
        //            client.Headers.Add("ContentType", "application/zip");

        //            client.UploadProgressChanged += (object sender, UploadProgressChangedEventArgs e) => { UploadProgressChangedHandler(id, sender, e); };
        //            client.UploadFileCompleted += (sender, args) => UploadCompletedHandler(fileName, tcs, args);
        //            client.UploadFileAsync(url, "PUT", fileName);
        //            tcs.Task.Wait();
        //        }
        //        catch (WebException ex)
        //        {
        //            Debug.LogError(ex);

        //            var resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();

        //            Debug.LogError(resp);

        //            dynamic obj = JsonConvert.DeserializeObject(resp);
        //            var messageFromServer = obj.error.message;

        //            Debug.LogError($"Error Message: {messageFromServer}");
        //        }
        //        catch (Exception ex)
        //        {
        //            Debug.LogError(ex);
        //            throw;
        //        }
        //    }
        //}

        //private void UploadCompletedHandler(string fileName, TaskCompletionSource<bool> tcs, UploadFileCompletedEventArgs e)
        //{
        //    MyLogger.Log($"Upload Complete!");

        //    // unregister event handlers
        //    foreach (Delegate d in OnProjectAssetUploadProgress.GetInvocationList())
        //    {
        //        OnProjectAssetUploadProgress -= (XrUploadEvent)d;
        //    }


        //    if (e.Cancelled)
        //    {
        //        tcs.TrySetCanceled();
        //    }
        //    else if (e.Error != null)
        //    {
        //        Debug.LogError(e.Error);

        //        try
        //        {
        //            Debug.LogError($"Error Message: \n{JsonUtility.ToJson(e)}");
        //        }
        //        catch { Debug.LogError($"CANNOT serialize error"); }

        //        tcs.TrySetException(e.Error);
        //    }
        //    else
        //    {
        //        tcs.TrySetResult(true);
        //    }
        //}

        //private void UploadProgressChangedHandler(Guid id, object sender, UploadProgressChangedEventArgs e)
        //{
        //    // Handle progress, e.g.
        //    //System.Diagnostics.Debug.WriteLine(e.ProgressPercentage);

        //    if (e.ProgressPercentage != _currentProgress)
        //    {
        //        _currentProgress = e.ProgressPercentage;
        //    }

        //    MyLogger.Log($"Upload Progress for {id} = {e.ProgressPercentage}%");

        //    //if (!_uploadMap.ContainsKey(id))
        //    //{
        //    //    _uploadMap.Add(id, e.ProgressPercentage);
        //    //}

        //    //if (_uploadMap[id] != e.ProgressPercentage)
        //    //{
        //    //    _uploadMap[id] = e.ProgressPercentage;

        //    //    //MyLogger.Log($"Upload Progress for {id} = {e.ProgressPercentage}%");

        //    //    //UnityMainThreadDispatcher.Instance().Enqueue(() => OnProjectAssetUploadProgress?.Invoke(e.ProgressPercentage / 100f));
        //    //}
        //}
    }
}