using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonlakeTools.Comcell;
using MoonlakeTools.MQTT;
using MoonlakeTools.MQTT.Models;
using System.Threading;
using MoonlakeTools.DTP;

namespace IoT.NETDemo
{
    class Program
    {
        static void Main(string[] args)
        {

            #region IoT.NET
            IoTAction ioTAction = new IoTAction("BS_VI_ASSY_FINAL_21", "127.0.0.1", 1883);
            ioTAction.Connect();

            ioTAction.PublishWarning("9529680912011287813", WarningParameter.WarningLevel.Info, "PLC", "产品未到位");//提示类消息
            ioTAction.PublishWarning("9529680912011287813", WarningParameter.WarningLevel.Error, "MES", "Checkin Failed");//错误类消息
            ioTAction.PublishWarning("9529680912011287813", WarningParameter.WarningLevel.Critical, "MES", "temp");

            ioTAction.PublishWIPSingle("9529680912011287813", true, "7.8", "A2C9619770403");//单板产品
            ioTAction.PublishWIPPanel("9529680912011287813", true, "7.8", "A2C9619770403");//Panel产品

            ioTAction.PublishChangeOver("A2C9619770403", "A夹具");
            ioTAction.PublishConsumable("9529680912011287813", "5394", "1号切刀");
            ioTAction.PublishSensor("9529680912011287813", "1", "检平传感器");
            ioTAction.PublishParameter("10.221.128.93:40122");
            ioTAction.PublishEVAPROD("9529680912011287813", "Presure", true, "8.9", "6", "9");
            #endregion

            #region MES

            string errorMsg;
            GHPAction ghpAction = new GHPAction();

            //ghpAction.Init("10.221.128.54", 40114, "BS_ASSY_FINAL_20", 60);
            ghpAction.Init("10.221.128.93", 40122, "BS_VI_ASSY_FINAL_21", 60, "iTwJawbD816FQn11EhH4rZulzpB+lHQc1zX/wqVNiSk=");

            ghpAction.Ping(out errorMsg);
            string msgRequest = ghpAction.msgRequest;
            string msgResponse = ghpAction.msgResponse;
            ghpAction.Identification(out errorMsg);
            string timeStamp;
            ghpAction.ReqTimeDate(out timeStamp, out errorMsg);

            ghpAction.SetupChange("A2C3927280322", out errorMsg);
            string materialOut;
            ghpAction.UnitCheckin("9529680922102271799", out materialOut, out errorMsg);
            ghpAction.UnitCheckout("A3C0116680091", true, out errorMsg);
            //带参数的： nokReason_val="0" nest_number="101"
            string xmlMsg = string.Concat(new string[]
            {
                "nokReason_val=\"",
                "0",
                "\" nest_number=\"",
                "101",
                "\""
            });
            ghpAction.UnitCheckout("A3C0116680091", true, xmlMsg, out errorMsg);

            string responseXML;
            ghpAction.ReqRunText("153343040120092106787", "CGQ", "A2C1996900391", "10006", out responseXML, out errorMsg);
            ghpAction.ReqAllIds("9805860522101190008", "CGQ", out responseXML, out errorMsg);
            ghpAction.ReqUnitInfo("9805860522101190008", "CGQ", out responseXML, out errorMsg);
            string matExp;
            string matPN;
            string matQtyAvail;
            ghpAction.CheckMaterial("00000A2C5335640549@0702014476@UCCQ004933821", out matExp, out matPN, out matQtyAvail, out errorMsg);

            #endregion

            #region DTP

            DTPAction dtpAction = new DTPAction();//实例化
            // The syntax of path is "\\server\folder\...\ Remark: The string have to terminated with "\"
            dtpAction.Dtp_SetDtpServerPath("L:\\DTP_DATA", out errorMsg);
            // if ready_to_check = 0 only released version the action reads
            // if ready_to_check=1  ready_to_check and released version the action reads
            // if ready_to_check=2  only ready_to_check versions the action reads
            int iReady_to_check = 1;
            // Parameter Sachzeichnungsnummer to search in dtp-directory.
            // Use wildcards(?) for every position for unknown or different Sachzeichnungsnummer
            string sSachzeichnungsnummer = "A2C194994";
            // Parameter returns the MlfB of selected ProcessStep
            string sMlfb = "A2C1949941100";
            // Process Step(e.g.EP; ICT; SPR-OUT.....)
            string sProcessStep = "EOL";
            // Returns the selected "Sachzeichnungsnummer" from the dialog
            string sSelectedSachzeichnungsnummer;
            // Parameter returns the MlfB of selected ProcessStep
            string sSelectedMlfb;
            // Parameter returns the version in relation to Mlfb of the selected ProcessStep
            string sSelectedVersion;
            //选择DTP
            dtpAction.DTP_mlfb_sz_select(iReady_to_check, sSachzeichnungsnummer, sProcessStep, out sSelectedSachzeichnungsnummer, out sSelectedMlfb, out sSelectedVersion, out errorMsg);

            // if iReady_to_check = 0->only read RELEASED version will read.
            // 1. The parameter "sVersion" is empty and the only one released version is read and      returns in this parameter.
            // 2. The parameter "sVersion" is not empty, the action search this referenced version.If the version not released, the action return fail.
            string sVersion = "";
            // if iForce_to_read = 0->only check Atf-path and - files
            // if iForce_to_read = 1 ->read all variables of all Atf-files into memory
            int iForce_to_read = 1;
            // This parameter is necassary only to copy files of the subfolder "atf\data" to the local drive.
            // iCopyFlag = 0  -> no copy
            // iCopyFlag = 1  -> copy if necassary
            // iCopyFlag = 2  -> copy and overwrite
            // iCopyFlag = 3  -> copy and verify all local files
            int iCopyFlag = 0;
            // This parameter defines the local drive letter by the caller and returns the local path where the files copy to.
            // This parameter is an input- and output-parameter.
            // input:
            // This parameter is necassary only to copy files of the subfolder "atf\data" to the local drive and will be ignore if parameter CopyFlag = 0. 
            // The parameter is only the drive letter without ":\" and path.
            // The path, if not exist, creates the action and is the same as  DTP-path.
            // output:
            // if the action successful, it returns the full local DTP-path.
            // e.g.C:\dtp\dtp_wall\dtp\dtp_data\ sachzeichnungsnummer \.....
            string sLocalDriveName = "D";
            // if OK = 1->returns in sData_Path the Atf - path   for example \\server:\dtp_wall\dtp\Data_Path\Sachzeichnungsnummer >\Mlfb\Version\
            // if OK != 1  ->returns errordescription
            string sData_Path;
            // Output Only: Returns the number of ATF variables read into memory
            // if iForce_to_read = 1 else iN_Variables = 0. Parameter added in version v3_03 of dtp action dll.
            int iN_Variables;
            //检查DTP状态并读取
            dtpAction.Dtp_check_and_read(iReady_to_check, sSachzeichnungsnummer, sMlfb, sProcessStep, sVersion, iForce_to_read, iCopyFlag, sLocalDriveName, out sData_Path, out iN_Variables, out errorMsg);

            string sVariableName = "PROUDCT_PART_NUMBER_FIN_A2C";
            string sVariableText;
            dtpAction.Dtp_Get_Variable(sVariableName, out sVariableText, out errorMsg);//获取ATF中单个变量

            string[] sarrVariableName = new string[2] { "PROUDCT_PART_NUMBER_FIN_A2C", "HW_VERSION" };
            string[] sarrVariableText = new string[2];
            dtpAction.Dtp_Get_Variables(sarrVariableName, out sarrVariableText, out errorMsg);//获取ATF中多个变量

            List<string> lstVariableName = new List<string> { "PROUDCT_PART_NUMBER_FIN_A2C", "HW_VERSION" };
            List<string> lstVariableText = new List<string>();
            dtpAction.Dtp_Get_Variables(lstVariableName, out lstVariableText, out errorMsg);//获取ATF中多个变量

            string[] sVersions;
            string[] sVersionInfos;
            dtpAction.Dtp_Get_State_Info(sSachzeichnungsnummer, sMlfb, sProcessStep, out sVersions, out sVersionInfos, out errorMsg);//获取State文件状态

            #endregion

        }
    }
}
