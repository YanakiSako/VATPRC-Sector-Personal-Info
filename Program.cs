﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

class Program
{
    static readonly string PRF_EXTENSION = ".prf";
    static readonly string SETTINGS_FOLDER = "Common Data/Settings";
    static readonly string TOPSKY_FOLDER = "Plugin/Topsky";
    static readonly List<string> CONFIG_PREFIXES = new List<string> {
        "LastSession\tcallsign",
        "LastSession\trealname",
        "LastSession\tcertificate",
        "LastSession\tpassword",
        "LastSession\trating",
        "LastSession\tserver",
        "LastSession\ttovatsim"
    };

    static readonly Dictionary<string, string> SERVER_MAPPING = new Dictionary<string, string> {
        { "0", "AUTOMATIC" },
        { "1", "AMSTERDAM" },
        { "2", "CANADA" },
        { "3", "GERMANY" },
        { "4", "GERMANY2" },
        { "5", "UK" },
        { "6", "USA-EAST" },
        { "7", "USA-EAST2" },
        { "8", "USA-WEST" }
    };

    static readonly Dictionary<string, string> RATING_MAPPING = new Dictionary<string, string> {
        { "0", "OBS" },
        { "1", "S1" },
        { "2", "S2" },
        { "3", "S3" },
        { "4", "C1" },
        { "6", "C3" },
        { "7", "I1" },
        { "9", "I3" }
    };

    static readonly Dictionary<string, string> RADAR_MODE_MAPPING = new Dictionary<string, string> {
        { "0", "Easy" },
        { "1", "Mode-S" },
    };

    static void RemoveLinesFromFile(string filePath, List<string> prefixes)
    {
        try
        {
            List<string> lines = new List<string>(File.ReadAllLines(filePath));
            using (StreamWriter file = new StreamWriter(filePath))
            {
                foreach (string line in lines)
                {
                    if (!prefixes.Exists(p => line.StartsWith(p)))
                    {
                        file.WriteLine(line);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occurred while processing {filePath}: {e.Message}");
        }
    }

    static string GetServerName(string serverCode)
    {
        return SERVER_MAPPING.TryGetValue(serverCode, out string serverName) ? serverName : "AUTOMATIC";
    }

    static string GetRating(string ratingCode)
    {
        return RATING_MAPPING.TryGetValue(ratingCode, out string rating) ? rating : "OBS";
    }

    static string GetRadarMode(string modeCode)
    {
        return RADAR_MODE_MAPPING.TryGetValue(modeCode, out string mode) ? mode : "Easy";
    }

    static void UpdateCpdlcCode(string folderPath, string cpdlcCode)
    {
        string[] airportCodes = { "PRC", "ZBPE", "ZGZU", "ZHWH", "ZJSA", "ZLHW", "ZPKM", "ZSHA", "ZWUQ", "ZYSH" };
        foreach (string airportCode in airportCodes)
        {
            string topskyFolder = Path.Combine(folderPath, airportCode, TOPSKY_FOLDER);
            string cpdlcFilePath = Path.Combine(topskyFolder, "TopSkyCPDLChoppieCode.txt");
            try
            {
                Directory.CreateDirectory(topskyFolder);
                File.WriteAllText(cpdlcFilePath, cpdlcCode);
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while updating CPDLC code for {airportCode}: {e.Message}");
            }
        }
    }

    static void UpdateRadarMode(string folderPath, string modeCode)
    {
        string settingFilePath = Path.Combine(folderPath, SETTINGS_FOLDER, "GeneralA.txt");
        try
        {
            string[] lines = File.ReadAllLines(settingFilePath);
            using (StreamWriter writer = new StreamWriter(settingFilePath))
            {
                foreach (string line in lines)
                {
                    if (line.StartsWith("m_CorrelationMode:"))
                    {
                        string mode = RADAR_MODE_MAPPING.ContainsKey(modeCode) ? RADAR_MODE_MAPPING[modeCode] : "0";
                        writer.WriteLine($"m_CorrelationMode:{mode}");
                    }
                    else
                    {
                        writer.WriteLine(line);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occurred while updating radar mode: {e.Message}");
        }
    }

    static void Main(string[] args)
    {
        ProcessFolder(CONFIG_PREFIXES);
    }

    static void ProcessFolder(List<string> prefixes)
    {
        Console.Title = "VATPRC-Sector Personal Info";
        string scriptDirectory = Directory.GetCurrentDirectory();
        while (true)
        {
            Console.WriteLine("请输入你的Real Name：");
            string realName = Console.ReadLine();

            Console.WriteLine("\n请输入你的VATSIM CID：");
            string cid = Console.ReadLine();

            Console.WriteLine("\n请输入你的密码：");
            string password = Console.ReadLine();

            Console.WriteLine("\n请输入你的VATSIM Rating (OBS-'0' S1-'1' S2-'2' S3-'3' C1-'4' C3-'6' I1-'7' I3-'9')：");
            string ratingCode = Console.ReadLine();

            Console.WriteLine("\n请输入你要连接的服务器 (AUTOMATIC-'0' AMSTERDAM-'1' CANADA-'2' GERMANY-'3' GERMANY2-'4' UK-'5' USA-EAST-'6' USA-EAST2-'7' USA-WEST-'8')：");
            string serverCode = Console.ReadLine();

            Console.WriteLine("请输入你的Hoppie CPDLC Code：");
            string cpdlcCode = Console.ReadLine();

            Console.WriteLine("请输入你的默认雷达模式 (0-Easy Mode, 1-Mode S)：");
            string radarMode = Console.ReadLine();

            string server = GetServerName(serverCode);
            string rating = GetRating(ratingCode);
            string RadarMode = GetRadarMode(radarMode);

            Console.WriteLine("\n请检查下面的信息：");
            Console.WriteLine("------------------------------");
            Console.WriteLine($"Real Name:\t{realName}\nVATSIM CID:\t{cid}\nVATSIM Rating:\t{rating}\nPassword:\t{password}\nServer:\t\t{server}\nCPDLC Code:\t{cpdlcCode}\t\nRadar Mode:\t{RadarMode}\t");
            Console.WriteLine("------------------------------\n");

            Console.WriteLine("信息是否正确？(Y/N)");
            string confirmation = Console.ReadLine().ToUpper();

            while (confirmation != "Y" && confirmation != "N")
            {
                Console.WriteLine("请键入 'Y' 或 'N'");
                confirmation = Console.ReadLine().ToUpper();
            }

            if (confirmation == "Y")
            {
                string[] files = Directory.GetFiles(scriptDirectory, $"*{PRF_EXTENSION}", SearchOption.AllDirectories);

                foreach (string file in files)
                {
                    RemoveLinesFromFile(file, prefixes);
                    AppendUserInfo(file, realName, cid, ratingCode, password, server);
                }

                Console.WriteLine("\n！！信息已保存成功！！\n");
                Console.WriteLine("------------------------------\n");
                Thread.Sleep(1000);
                break;
            }
            else
            {
                Console.WriteLine("\n！！未保存，请重新输入！！\n");
            }
        }
    }

    static void AppendUserInfo(string filePath, string realName, string cid, string rating, string password, string server)
    {
        try
        {
            using (StreamWriter file = File.AppendText(filePath))
            {
                file.WriteLine($"LastSession\trealname\t{realName}");
                file.WriteLine($"LastSession\tcertificate\t{cid}");
                file.WriteLine($"LastSession\trating\t{rating}");
                file.WriteLine($"LastSession\tpassword\t{password}");
                file.WriteLine($"LastSession\tserver\t{server}");
                file.WriteLine("LastSession\ttovatsim\t1");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occurred while updating {filePath}: {e.Message}");
        }
    }
}