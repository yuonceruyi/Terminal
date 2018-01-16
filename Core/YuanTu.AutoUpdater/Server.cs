using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace YuanTu.AutoUpdater
{
    public class Server
    {
        private static readonly Version baseVersion = new Version(1, 0, 0, 0);
        public List<FileInfo> Make(string hospitalId = null)
        {
            var newFileList = new List<FileInfo>();

            if (hospitalId == null)
                hospitalId = string.Empty;

            var versionDir = new DirectoryInfo(Path.Combine(AutoUpdater.BaseDir, "Versions", hospitalId));
            if (!versionDir.Exists)
                versionDir.Create();

            var xmlFile = new FileInfo(Path.Combine(versionDir.FullName, ConstFile.REMOTEFILENAME));
            if (!xmlFile.Exists)
                CreateLocalXml(versionDir, xmlFile.FullName);

            var serverConfig = ServerConfig.LoadConfig(xmlFile.FullName);
            var localDic = AutoUpdater.PathList
                .ToDictionary(path => path, path => new FileInfo(Path.Combine(AutoUpdater.BaseDir, path)));

            foreach (var info in serverConfig.FileList)
            {
                if (!localDic.ContainsKey(info.Path)) //本地不生成 跳过
                    continue;
                var oldFile = new FileInfo(Path.Combine(versionDir.FullName, info.Url));
                if (!isTheSame(localDic[info.Path], oldFile))
                {
                    var v = IncVersion(Version.Parse(info.LastVer));
                    var f = CopyToNew(localDic[info.Path], versionDir, v, info.Path);
                    newFileList.Add(f);
                    info.LastVer = v.ToString();
                    info.Size = f.Length;
                    info.Url = CreateFolderName(f.Name,info.Path, versionDir);
                }
                localDic.Remove(info.Path);
            }
            foreach (var info in localDic)
            {
                serverConfig.FileList.Add(CreateRemoteFile(info.Key, versionDir));
            }
            serverConfig.SaveConfig(xmlFile.FullName);
            return newFileList;
        }

        private bool isTheSame(FileInfo f1, FileInfo f2)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hash1, hash2;
                using (var stream = f1.OpenRead())
                    hash1 = md5.ComputeHash(stream);
                using (var stream = f2.OpenRead())
                    hash2 = md5.ComputeHash(stream);
                return hash1.SequenceEqual(hash2);
            }
        }

        private Version IncVersion(Version v)
        {
            return new Version(v.Major, v.Minor, v.Build, v.Revision + 1);
        }

        private FileInfo CopyToNew(FileInfo f, DirectoryInfo d, Version v, string path)
        {
            var version = v.ToString();
            var name = version + "-" + path;
            var p = path.LastIndexOf('.');
            if (p > 0)
            {
                name = path.Substring(0, p) + "-" + version + path.Substring(p);
            }
            var c = new FileInfo(Path.Combine(d.FullName, name));
            var dir = c.Directory;
            if (!dir.Exists)
                dir.Create();
            f.CopyTo(c.FullName);
            return c;
        }

        private void CreateLocalXml(DirectoryInfo dir, string xml)
        {
            var serverConfig = new ServerConfig
            {
                ServerUrl = AutoUpdater.ServerUrl,
                FileList = new List<RemoteFile>()
            };
            foreach (var path in AutoUpdater.PathList)
                serverConfig.FileList.Add(CreateRemoteFile(path, dir));
            serverConfig.SaveConfig(xml);
        }

        private RemoteFile CreateRemoteFile(string path, DirectoryInfo dir)
        {
            var f = new FileInfo(Path.Combine(AutoUpdater.BaseDir, path));
            var c = CopyToNew(f, dir, baseVersion, path);
            var tempName = c.Name;
            var tempPath = path;
            if (!string.IsNullOrWhiteSpace(path) && path.Contains("\\"))
            {
                tempPath = tempPath.Replace("\\", "/");
                var p = tempPath.LastIndexOf('/');
                tempPath = tempPath.Substring(0, p+1);
                tempName = tempPath + c.Name;
            }
            return new RemoteFile
            {
                Path = path,
                LastVer = baseVersion.ToString(),
                Size = c.Length,
                NeedRestart = true,
                Url = tempName
            };
        }

        private string CreateFolderName(string fileName,string path, DirectoryInfo dir)
        {
            var tempName = fileName;
            var tempPath = path;
            if (!string.IsNullOrWhiteSpace(path) && path.Contains("\\"))
            {
                tempPath = tempPath.Replace("\\", "/");
                var p = tempPath.LastIndexOf('/');
                tempPath = tempPath.Substring(0, p + 1);
                tempName = tempPath + fileName;
            }
            return tempName;
        }
    }
}