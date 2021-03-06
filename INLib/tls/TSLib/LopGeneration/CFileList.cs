﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Proto2Code
{
    public enum EFileType
    {
        Deleted,    //已被删除
        New,        //新文件或被修改过
        UnModified, //未被修改
    }

    class CFileInfo
    {
        public string name;
        public string md5;
        public EFileType type;
        public CFileInfo(string name,string md5,EFileType type)
        {
            this.name = name;
            this.md5 = md5;
            this.type = type;
        }
    }
    
    class CFileList : CSLib.Utility.CSingleton<CFileList>
    {
        private string m_strRootDirectory = "";
        public string RootDirectory
        {
            get
            {
                return m_strRootDirectory;
            }
            set
            {
                m_strRootDirectory = value;
                m_strRootDirectory = m_strRootDirectory.Replace('\\', '/');
            }
        }

        /// <summary>
        /// 读取生成记录
        /// </summary>
        public void ReadFileList()
        {
            m_dicFileList.Clear();

            string strExe = m_strRootDirectory + "/TableGen/01_LopGeneration/LopGeneration.exe";
            string strDll1 = m_strRootDirectory + "/TableGen/01_LopGeneration/tableGeneration.dll";
            string strDll2 = m_strRootDirectory + "/TableGen/01_LopGeneration/protoGeneration.dll";

            if (!File.Exists(m_strFileList))
            {
                File.Create(m_strFileList).Close();
                IsNewFile(strExe); // 把工具文件信息放进去
                IsNewFile(strDll1);
                IsNewFile(strDll2);
                return;
            }

            //
            StreamReader reader = new StreamReader(m_strFileList, Encoding.UTF8);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }
                    
                string[] infos = line.Split('|');
                string fileFullPath = m_strRootDirectory + "/" + infos[0];
                FileInfo fi = new FileInfo(fileFullPath);
                if (!fi.Exists)
                {
                    Console.WriteLine("file deleted:"+ infos[0]);
                    m_dicFileList.Add(infos[0],new CFileInfo(infos[0],infos[1],EFileType.Deleted));
                    continue;
                }

                string newMd5 = CSLib.Security.CMd5.EncodeFile(fileFullPath);
                if (newMd5 == infos[1])
                {
                    m_dicFileList.Add(infos[0], new CFileInfo(infos[0], infos[1], EFileType.UnModified));
                }
                else
                {
                    m_dicFileList.Add(infos[0], new CFileInfo(infos[0], newMd5, EFileType.New));
                }
            }
            reader.Close();

            //
            if (IsNewFile(strExe) || IsNewFile(strDll1) || IsNewFile(strDll2))
            {
                m_dicFileList.Clear();
                IsNewFile(strExe); // 把工具文件信息放进去
                IsNewFile(strDll1);
                IsNewFile(strDll2);
            }
        }

        /// <summary>
        /// 保存生成记录
        /// </summary>
        public void SaveFileList()
        {
            StreamWriter writer = new StreamWriter(m_strFileList, false,Encoding.UTF8);
            foreach (var v in m_dicFileList)
            {
                if(v.Value.type != EFileType.Deleted)
                {
                    writer.WriteLine(v.Key + "|" + v.Value.md5);
                }
            }
            writer.Close();
        }

        /// <summary>
        /// 清理无用文件
        /// </summary>
        public void ClearUnuseFile()
        {
            string pbCsFilter = m_strRootDirectory + "/TableGen/01_LopGeneration/fileterFiles.pb.cs";
            string pbLuaFilter = m_strRootDirectory + "/TableGen/01_LopGeneration/fileterFiles.pb.lua";
            string peCsFilter = m_strRootDirectory + "/TableGen/01_LopGeneration/fileterFiles.pe.cs";
            string peLuaFilter = m_strRootDirectory + "/TableGen/01_LopGeneration/fileterFiles.pe.lua";
            string ltHFilter = m_strRootDirectory + "/TableGen/01_LopGeneration/fileterFiles.lt.h";
            string ltCcFilter = m_strRootDirectory + "/TableGen/01_LopGeneration/fileterFiles.lt.cc";

            List<string> pbCsFileList = new List<string>();
            List<string> pbluaFileList = new List<string>();
            List<string> peCsFileList = new List<string>();
            List<string> peLuaFileList = new List<string>();
            List<string> ltHFileList = new List<string>();
            List<string> ltCcFileList = new List<string>();

            pbCsFileList.AddRange(File.ReadAllLines(pbCsFilter));
            pbluaFileList.AddRange(File.ReadAllLines(pbLuaFilter));
            peCsFileList.AddRange(File.ReadAllLines(peCsFilter));
            peLuaFileList.AddRange(File.ReadAllLines(peLuaFilter));
            ltHFileList.AddRange(File.ReadAllLines(ltHFilter));
            ltCcFileList.AddRange(File.ReadAllLines(ltCcFilter));

            File.Delete(pbCsFilter);
            File.Delete(pbLuaFilter);
            File.Delete(peCsFilter);
            File.Delete(peLuaFilter);
            File.Delete(ltHFilter);
            File.Delete(ltCcFilter);

            #region
            foreach (var v in m_dicFileList)
            {
                if (v.Value.type != EFileType.Deleted)
                {
                    continue;
                }

                // 残留的生成文件要删掉，先写死。
                if (v.Key.EndsWith(".xlsx"))
                {
                    Console.WriteLine("删除" + v.Key + "生成的残留文件");
                    string name = _GetFileNameAndFirstCharToLower(v.Key);

                    // 删除过滤文件里信息
                    string pbLuaFilterLine = name + "_pb.lua";
                    string pbCsFilterLine = name + ".pb.cs";
                    string peLuaFilterLine = name + "_pe.lua";
                    string peCsFilterLine = name + ".pe.cs";
                    string ltHFilterLine = name + ".lt.h";
                    string ltCcFilterLine = name + ".lt.cc";

                    if (pbluaFileList.Remove(pbLuaFilterLine)) { Console.WriteLine("filterFiles.pb.lua删除行："+ pbLuaFilterLine); }
                    if (pbCsFileList.Remove(pbCsFilterLine)) { Console.WriteLine("filterFiles.pb.cs删除行："+ pbCsFilterLine); }
                    if (peLuaFileList.Remove(peLuaFilterLine)) { Console.WriteLine("filterFiles.pe.cs删除行："+ peLuaFilterLine); }
                    if (peCsFileList.Remove(peCsFilterLine)) { Console.WriteLine("filterFiles.pe.lua删除行：" + peCsFilterLine); }
                    if (peCsFileList.Remove(ltHFilterLine)) { Console.WriteLine("filterFiles.lt.h删除行：" + ltHFilterLine); }
                    if (peCsFileList.Remove(ltCcFilterLine)) { Console.WriteLine("filterFiles.lt.cc删除行：" + ltCcFilterLine); }

                    // 删除生成的PE文件
                    string proto = m_strRootDirectory + string.Format("/TableOut/Temp/1_Protoext/{0}.proto", name);
                    string cspe = m_strRootDirectory + string.Format("/TableOut/Temp/1_Protoext/C#/{0}.pe.cs", name);
                    string ccpeh = m_strRootDirectory + string.Format("/TableOut/Temp/1_Protoext/C++/{0}.pe.h", name);
                    string ccpecc = m_strRootDirectory + string.Format("/TableOut/Temp/1_Protoext/C++/{0}.pe.cc", name);
                    string gope = m_strRootDirectory + string.Format("/TableOut/Temp/1_Protoext/GO/{0}.pe.go", name);
                    string luape = m_strRootDirectory + string.Format("/TableOut/Temp/1_Protoext/Lua/{0}_pe.go", name);
                    string cclth = m_strRootDirectory + string.Format("/TableOut/Temp/1_Protoext/C++/{0}.lt.h", name);
                    string ccltcc = m_strRootDirectory + string.Format("/TableOut/Temp/1_Protoext/C++/{0}.lt.cc", name);

                    if (File.Exists(proto)) { Console.WriteLine("删除" + proto); File.Delete(proto); }
                    if (File.Exists(cspe)) { Console.WriteLine("删除" + cspe); File.Delete(cspe); }
                    if (File.Exists(ccpeh)) { Console.WriteLine("删除" + ccpeh); File.Delete(ccpeh); }
                    if (File.Exists(ccpecc)) { Console.WriteLine("删除" + ccpecc); File.Delete(ccpecc); }
                    if (File.Exists(ccltcc)) { Console.WriteLine("删除" + ccltcc); File.Delete(ccltcc); }
                    if (File.Exists(cclth)) { Console.WriteLine("删除" + cclth); File.Delete(cclth); }
                    if (File.Exists(gope)) { Console.WriteLine("删除" + gope); File.Delete(gope); }
                    if (File.Exists(luape)) { Console.WriteLine("删除" + luape); File.Delete(luape); }

                    // 删除生成的PB文件
                    string cspb = m_strRootDirectory + string.Format("/TableOut/Temp/2_Protobuf/C#/{0}.pb.cs", name);
                    string ccpbh = m_strRootDirectory + string.Format("/TableOut/Temp/2_Protobuf/C++/{0}.pb.h", name);
                    string ccpbcc = m_strRootDirectory + string.Format("/TableOut/Temp/2_Protobuf/C++/{0}.pb.cc", name);
                    string luapb = m_strRootDirectory + string.Format("/TableOut/Temp/2_Protobuf/Lua/{0}_pb.lua", name);
                    string pybp = m_strRootDirectory + string.Format("/TableOut/Temp/2_Protobuf/Python/{0}_pb2.py", name);

                    if (File.Exists(cspb)) { Console.WriteLine("删除" + cspb); File.Delete(cspb); }
                    if (File.Exists(ccpbh)) { Console.WriteLine("删除" + ccpbh); File.Delete(ccpbh); }
                    if (File.Exists(ccpbcc)) { Console.WriteLine("删除" + ccpbcc); File.Delete(ccpbcc); }
                    if (File.Exists(luapb)) { Console.WriteLine("删除" + luapb); File.Delete(luapb); }
                    if (File.Exists(pybp)) { Console.WriteLine("删除" + pybp); File.Delete(pybp); }

                    // 删除生成的二进制文件
                    string txt = m_strRootDirectory + string.Format("/TableOut/Temp/3_Protobin/{0}.txt", name);
                    string dbg = m_strRootDirectory + string.Format("/TableOut/Temp/3_Protobin/{0}.txt.dbg", name);

                    if (File.Exists(txt)) { Console.WriteLine("删除" + txt); File.Delete(txt); }
                    if (File.Exists(dbg)) { Console.WriteLine("删除" + dbg); File.Delete(dbg); }
                }

                if (v.Key.EndsWith(".proto"))
                {
                    Console.WriteLine("删除" + v.Key + "生成的残留文件");
                    string name = _GetFileNameAndFirstCharToLower(v.Key);

                    // 删除过滤文件里信息
                    string pbLuaFilterLine = name + "_pb.lua";
                    string pbCsFilterLine = name + ".pb.cs";
                    string peLuaFilterLine = name + "_pe.lua";
                    string peCsFilterLine = name + ".pe.cs";
                    string ltHFilterLine = name + ".lt.h";
                    string ltCcFilterLine = name + ".lt.cc";

                    if (pbluaFileList.Remove(pbLuaFilterLine)) { Console.WriteLine("filterFiles.pb.lua删除行：" + pbLuaFilterLine); }
                    if (pbCsFileList.Remove(pbCsFilterLine)) { Console.WriteLine("filterFiles.pb.cs删除行：" + pbCsFilterLine); }
                    if (peLuaFileList.Remove(peLuaFilterLine)) { Console.WriteLine("filterFiles.pe.cs删除行：" + peLuaFilterLine); }
                    if (peCsFileList.Remove(peCsFilterLine)) { Console.WriteLine("filterFiles.pe.lua删除行：" + peCsFilterLine); }
                    if (ltHFileList.Remove(ltHFilterLine)) { Console.WriteLine("filterFiles.pe.cs删除行：" + ltHFilterLine); }
                    if (ltCcFileList.Remove(ltCcFilterLine)) { Console.WriteLine("filterFiles.pe.lua删除行：" + ltCcFilterLine); }

                    // 删除生成的PB文件
                    string cspb = m_strRootDirectory + string.Format("/TableOut/Temp/2_Protobuf/C#/{0}.pb.cs", name);
                    string ccpbh = m_strRootDirectory + string.Format("/TableOut/Temp/2_Protobuf/C++/{0}.pb.h", name);
                    string ccpbcc = m_strRootDirectory + string.Format("/TableOut/Temp/2_Protobuf/C++/{0}.pb.cc", name);
                    string luapb = m_strRootDirectory + string.Format("/TableOut/Temp/2_Protobuf/Lua/{0}_pb.lua", name);
                    string pybp = m_strRootDirectory + string.Format("/TableOut/Temp/2_Protobuf/Python/{0}_pb2.py", name);
                    string cclth = m_strRootDirectory + string.Format("/TableOut/Temp/1_Protoext/C++/{0}.lt.h", name);
                    string ccltcc = m_strRootDirectory + string.Format("/TableOut/Temp/1_Protoext/C++/{0}.lt.cc", name);

                    if (File.Exists(cspb)) { Console.WriteLine("删除" + cspb); File.Delete(cspb); }
                    if (File.Exists(ccpbh)) { Console.WriteLine("删除" + ccpbh); File.Delete(ccpbh); }
                    if (File.Exists(ccpbcc)) { Console.WriteLine("删除" + ccpbcc); File.Delete(ccpbcc); }
                    if (File.Exists(ccltcc)) { Console.WriteLine("删除" + ccltcc); File.Delete(ccltcc); }
                    if (File.Exists(cclth)) { Console.WriteLine("删除" + cclth); File.Delete(cclth); }
                    if (File.Exists(luapb)) { Console.WriteLine("删除" + luapb); File.Delete(luapb); }
                    if (File.Exists(pybp)) { Console.WriteLine("删除" + pybp); File.Delete(pybp); }
                }
            }
            #endregion

            _ClearUnuseFile4Writer(pbCsFilter, pbCsFileList);
            _ClearUnuseFile4Writer(pbLuaFilter, pbluaFileList);
            _ClearUnuseFile4Writer(peCsFilter, peCsFileList);
            _ClearUnuseFile4Writer(peLuaFilter, peLuaFileList);
            _ClearUnuseFile4Writer(ltHFilter, ltHFileList);
            _ClearUnuseFile4Writer(ltCcFilter, ltCcFileList);
        }

        //根据文件的绝对路径获取首字母小写的文件名
        private string _GetFileNameAndFirstCharToLower(string file)
        {
            if (string.IsNullOrEmpty(file))
                return "";
            string s = file.Replace('\\', '/');
            string[] strArray = s.Split('/');
            string temp = char.ToLower(strArray[strArray.Length - 1][0]) + strArray[strArray.Length - 1].Substring(1);
            return temp.Substring(0, temp.IndexOf('.'));
        }

        private void _ClearUnuseFile4Writer(string strFilter, List<string> fileList)
        {
            UTF8Encoding utf8WithBom = new UTF8Encoding(true);
            FileStream fileStream = new FileStream(strFilter, FileMode.Create);
            StreamWriter streamWriter = new StreamWriter(fileStream, utf8WithBom);
            foreach (var line in fileList)
            {
                streamWriter.WriteLine(line);
            }
            streamWriter.Close();
            fileStream.Close();
            fileStream.Dispose();
        }

        /// <summary>
        /// 文件是否被修改过
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool IsNewFile(string file)
        {
            FileInfo fileInfo = new FileInfo(file);
            if (!fileInfo.Exists)
            {
                CSLib.Utility.CDebugOut.LogError("文件不存在。\r{0}", file);
                Console.WriteLine("文件不存在。\r{0}", file);
                return false;
            }

            string strFileName = fileInfo.FullName.Replace('\\', '/');
            string strFileKey = strFileName.Replace(RootDirectory, "");

            CFileInfo cfi = null;
            m_dicFileList.TryGetValue(strFileKey, out cfi);
            if (null == cfi)
            {
                string strMD5 = CSLib.Security.CMd5.EncodeFile(strFileName);
                cfi = new CFileInfo(strFileKey, strMD5, EFileType.New);
                m_dicFileList.Add(strFileKey, cfi);
            }

            if (cfi.type == EFileType.New)
            {
                return true;
            }

            return false;
        }

        //将文件标记为新文件
        public bool SetNewFile(string file)
        {
            FileInfo fileInfo = new FileInfo(file);
            if(!fileInfo.Exists)
            {
                return false;
            }

            string strFileName = fileInfo.FullName.Replace('\\', '/');
            string strFileKey = strFileName.Replace(RootDirectory, "");

            if (m_dicFileList.ContainsKey(strFileKey))
            {
                m_dicFileList[strFileKey].type = EFileType.New;
                return true;
            }

            return false;
        }

        //将文件名首字母转为小写
        public string FirstCharToLower(string str)
        {
            if (string.IsNullOrEmpty(str))
                return "";
            string s = str.Replace('\\', '/');
            string[] strArray = s.Split('/');
            string result = "";
            for(int i=0;i<strArray.Length-1;i++)
            {
                result = result + strArray[i] + "/";
            }
            return result + char.ToLower(strArray[strArray.Length-1][0]) + strArray[strArray.Length - 1].Substring(1);
        }

        //
        public string m_strFileList = @".\TableOut\Temp\FileList.all";
        private Dictionary<string, CFileInfo> m_dicFileList = new Dictionary<string, CFileInfo>();
    }
}
