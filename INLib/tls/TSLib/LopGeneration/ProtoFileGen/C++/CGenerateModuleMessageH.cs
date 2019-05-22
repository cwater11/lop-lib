﻿using System.Collections.Generic;
using System.IO;

namespace Proto2Code
{
    public class CGenerateModuleMessageH
    {
        private Dictionary<string, List<string>> serverDic = new Dictionary<string, List<string>>();

        private CNameUnit m_nameUnit;
        private string m_writePath_h;

        private FileStream m_file;
        private StreamWriter m_writer;

        private List<string> m_PTBufList = new List<string>();

        public CGenerateModuleMessageH(CNameUnit nameUnit, string writeRoot)
        {
            m_nameUnit = nameUnit;
            m_writePath_h = writeRoot + m_nameUnit.MoudleSysName + "Msg" + ".h";
        }

        public void StartWriter(List<SMsgID> systemList,string EFUNC)
        {
            m_file = new FileStream(m_writePath_h, FileMode.OpenOrCreate);
            m_writer = new StreamWriter(m_file);

            WriteFixedHead();
            WriteExtraIncludeFile(systemList);

            WriteNamespaceBegin();

            WritePragmaBegin();
            WriteMsgDefine(systemList,EFUNC);
            WritePragmaEnd();

            _WriteFuncDeclare(EFUNC);

            WriteNamespaceEnd();

            m_writer.Close();
        }

        private void WriteFixedHead()
        {
            m_writer.WriteLine("// ------------------------------------------------------------------------------");
            m_writer.WriteLine("//  <autogenerated>");
            m_writer.WriteLine("//      This code was generated by a tool.");
            m_writer.WriteLine("//      Changes to this file may cause incorrect behavior and will be lost if ");
            m_writer.WriteLine("//      the code is regenerated.");
            m_writer.WriteLine("//  </autogenerated>");
            m_writer.WriteLine("// ------------------------------------------------------------------------------");
            m_writer.WriteLine("");

            m_writer.WriteLine("#ifndef __SHLIB_MESSAGE_{0}MSG_H__", m_nameUnit.MoudleSysUpper);
            m_writer.WriteLine("#define __SHLIB_MESSAGE_{0}MSG_H__", m_nameUnit.MoudleSysUpper);
            m_writer.WriteLine("");

            m_writer.WriteLine("#include <SHLib/message/message.h>");
            m_writer.WriteLine("#include <BCLib/framework/msgExec.h>");
            m_writer.WriteLine("#include <BCLib/framework/msgExecMgr.h>");
            m_writer.WriteLine("#include <BCLib/framework/thdMsgLabel.h>");
            m_writer.WriteLine("#include <SHLib/protobuf/{0}.pb.h>", m_nameUnit.MoudleSysName);   
        }

        private void WriteExtraIncludeFile(List<SMsgID> list)
        {
            foreach (SMsgID item in list)
            {
                //if (item.enumPTBuf==string.Empty)
                //{
                //    continue;
                //}
                if (!item.enumPTBuf.Contains("PTBuf::"))
                {
                    continue;
                }
                string head = item.enumPTBuf.Split(':')[2];
                string path = new DirectoryInfo("../").FullName + "\\11_ProTableGen_Out\\";

                string headFile = CHelper.WriteHead(head);

                if (!m_PTBufList.Contains(headFile) && File.Exists(path + headFile + ".proto"))
                {
                    m_writer.WriteLine("#include <SHLib/protobuf/{0}.pb.h>", headFile);
                    m_PTBufList.Add(headFile);          
                }

            }
            m_writer.WriteLine("");
        }

        private void WriteNamespaceBegin()
        {
            m_writer.WriteLine("using namespace PTBuf;");
            m_writer.WriteLine("");

            m_writer.WriteLine("namespace SHLib");
            m_writer.WriteLine("{");
            m_writer.WriteLine("std::string SHLIB_SH_API {0}(BCLib::int32 msgID);", m_nameUnit.MoudleSysName + "Msg2Str");
            m_writer.WriteLine("");

            m_writer.WriteLine("namespace Message");
            m_writer.WriteLine("{");
        }

        private void WritePragmaBegin()
        {
            m_writer.WriteLine("#ifdef WIN32");
            m_writer.WriteLine("#pragma pack (push, 1)");
            m_writer.WriteLine("#else");
            m_writer.WriteLine("#pragma pack (1)");
            m_writer.WriteLine("#endif");
        }

        private void WriteMsgDefine(List<SMsgID> systemList,string EFUNC)
        {
            foreach (SMsgID item in systemList)
            {
                string msgEnumName = item.enumName;
                string PTBuf = item.enumPTBuf;
                string type = CHelper.FindELGCServerType(msgEnumName.Split('_')[1].Split('2')[1]);

                if (!serverDic.ContainsKey(type))
                {
                    List<string> serverList = new List<string>();
                    serverDic.Add(type, serverList);
                }
                serverDic[type].Add(msgEnumName);

                string[] label = PTBuf.Split(',');
                //label[0] = （SHLIB_BUFMSG_DEFINE/SHLIB_NETMSG_DEFINE
                //label[1] = PTBuf::XX...
                if (!PTBuf.Contains("PTBuf::"))
                {
                    m_writer.Write("    {4}({0}, {1}, {2}, {3});", type, EFUNC,
                        msgEnumName, CHelper.ChangeEnum(msgEnumName, "CMsg"), label[0].Trim());
                    m_writer.WriteLine("");
                }
                else
                {
                    m_writer.Write("    {5}({0}, {1}, {2}, {3},{4});", type, EFUNC,
                        msgEnumName, CHelper.ChangeEnum(msgEnumName, "CMsg"), label[1].Trim(), label[0].Trim());
                    m_writer.WriteLine("");
                }
            }
        }

        private void _WriteFuncDeclare(string EFUNC)
        {
            Dictionary<string, List<string>>.KeyCollection keyColl = serverDic.Keys;
            //Console.WriteLine(keyColl.Count);
            int i = 0;
            int length = 0;
            //处理间隔空行用计数期 i ，length为会打印的区域块数 --2018.12.6 亚古留
            foreach (var s in keyColl)
            {
                string type = s.Split('_')[1];
                if ((type == "DATABASE") || (type == "GAMECLIENT"))
                {
                    continue;
                }
                length++;
            }

            foreach (string server in keyColl)
            {
                string type = server.Split('_')[1];
                if ((type == "DATABASE") || (type == "GAMECLIENT"))
                {
                    continue;
                }

                #region _ONMSGFUNC_DECLARE

                m_writer.Write("#define {0}_{1}_ONMSGFUNC_DECLARE", type, m_nameUnit.MoudleSysName.ToUpper());
                m_writer.WriteLine("    " + @"\");
                int declareCount = serverDic[server].Count;
                int index = 1;
                foreach (string item in serverDic[server])
                {
                    m_writer.Write("    virtual void {0}(BCLib::Framework::SThdMsgLabel* msgLabel, BCLib::Framework::SMessage* msg);", CHelper.ChangeEnum(item, "_on"));
                    if (index != declareCount)
                    {
                        m_writer.WriteLine("    " + @"\");
                    }
                    else
                    {
                        m_writer.WriteLine("");
                    }
                    ++index;
                }
                m_writer.WriteLine("");

                #endregion

                m_writer.Write("#define {0}_{1}_CREATEMSGEXECPTR_01", type, m_nameUnit.MoudleSysUpper);
                m_writer.WriteLine("    " + @"\");

                m_writer.Write("    BCLIB_MSGEXEC_DEFINE_BEGIN(type)");
                m_writer.WriteLine("    " + @"\");

                m_writer.Write("        BCLIB_MSGEXEC_DEFINE_TYPE_BEGIN(SFLIB_MSG_TYPE(SHLib::{0}, PTBuf::{1}), id)", server, EFUNC);
                m_writer.WriteLine("    " + @"\");

                foreach (string item in serverDic[server])
                {
                    m_writer.Write("            BCLIB_MSGEXEC_DEFINE_ID(msgExecPtr, PTBuf::{0}, new BCLib::Framework::CMsgExec(&C{1}::{2}, this))", item, CHelper.ToCaptilize(m_nameUnit.MoudleSysName), CHelper.ChangeEnum(item, "_on"));
                    m_writer.Write("    " + @"\");
                    m_writer.WriteLine("");
                }
                m_writer.Write("        BCLIB_MSGEXEC_DEFINE_TYPE_END");
                m_writer.Write("    " + @"\");

                m_writer.WriteLine("");
                m_writer.WriteLine("    BCLIB_MSGEXEC_DEFINE_END");
                if (i < length - 1)
                {
                    i++;
                    m_writer.WriteLine("");
                }
            }

            // Class
            m_writer.WriteLine("");
            m_writer.WriteLine("class C{0} : public BCLib::Framework::CMsgExecMgr", CHelper.ToCaptilize(m_nameUnit.MoudleSysName));
            m_writer.WriteLine("{");
            m_writer.WriteLine("protected:");
            foreach (string server in keyColl)
            {
                string type = server.Split('_')[1];
                if ((type == "DATABASE") || (type == "GAMECLIENT"))
                {
                    continue;
                }
                m_writer.WriteLine("    {0}_{1}_ONMSGFUNC_DECLARE;", type, m_nameUnit.MoudleSysName.ToUpper());
            }
            m_writer.WriteLine("");
            m_writer.WriteLine("protected:");
            m_writer.WriteLine("    virtual bool _createMsgExecPtr(BCLib::uint16 type, BCLib::Framework::CMsgExecPtr& msgExecPtr);");
            m_writer.WriteLine("    virtual bool _createMsgExecPtr(BCLib::uint16 type, BCLib::uint16 id, BCLib::Framework::CMsgExecPtr& msgExecPtr);");
            m_writer.WriteLine("};");
            m_writer.WriteLine("");
        }

        private void WritePragmaEnd()
        {
            m_writer.WriteLine("#ifdef WIN32");
            m_writer.WriteLine("#pragma pack (pop)");
            m_writer.WriteLine("#else");
            m_writer.WriteLine("#pragma pack ()");
            m_writer.WriteLine("#endif");
            m_writer.WriteLine("");
        }

        private void WriteNamespaceEnd()
        {
            m_writer.WriteLine("} // Message");
            m_writer.WriteLine("} // SHLib");
            m_writer.WriteLine("#endif");
        }
    }
}
