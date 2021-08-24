using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.VisualBasic;
using System.Data.SQLite;
using System.Data;

public class LocalStorage
{
    private static string SettingsFolder;
    private static string SettingsFile;
    public static string SettingsApplication { get; set; } = "MyApp";

    public static string Email
    {
        get
        {
            return GetSetting(SettingsApplication, "User", "Email", "?");
        }
        set
        {
            SaveSetting(SettingsApplication, "User", "Email", value);
        }
    }



    /// <summary>
    /// Get the User's Setting
    /// </summary>
    /// <param name="Application"></param>
    /// <param name="Section"></param>
    /// <param name="Key"></param>
    /// <param name="DefaultValue"></param>
    /// <returns>String value for setting</returns>
    internal static string GetSetting(string Application, string Section, string Key, string DefaultValue = "")
    {
        try
        {
            object Result = null;
            if (System.IO.File.Exists(SettingsFile) == false) CreateDatabase();
            using (SQLiteConnection con = new SQLiteConnection(GetConString()))
            {
                if (con.State == ConnectionState.Closed) con.Open();
                SQLiteCommand cmd = new SQLiteCommand("Select SettingValue from UserSettings where SectionName=@SectionName And KeyName=@KeyName And Application=@Application", con);
                cmd.Parameters.AddWithValue("@Application", Application);
                cmd.Parameters.AddWithValue("@SectionName", Section);
                cmd.Parameters.AddWithValue("@KeyName", Key);
                Result = cmd.ExecuteScalar();
                cmd.Dispose();
                cmd = null;
            }

            if (Result == null)
            {
                SaveSetting(Application, Section, Key, DefaultValue);
                return DefaultValue;
            }
            else
                return Result.ToString();
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Save the user's setting - if the setting exist it gets updated
    /// </summary>
    /// <param name="Application"></param>
    /// <param name="Section"></param>
    /// <param name="Key"></param>
    /// <param name="Setting"></param>
    internal static void SaveSetting(string Application, string Section, string Key, string Setting)
    {
        if (System.IO.File.Exists(SettingsFile) == false) CreateDatabase();

        if (SettingExist(Application, Section, Key) == false)
        {
            using (SQLiteConnection con = new SQLiteConnection(GetConString()))
            {
                if (con.State == ConnectionState.Closed) con.Open();
                SQLiteCommand cmd = new SQLiteCommand("insert into UserSettings (SectionName,KeyName,Application,SettingValue) VALUES (@SectionName,@KeyName,@Application,@SettingValue)", con);
                cmd.Parameters.AddWithValue("@Application", Application);
                cmd.Parameters.AddWithValue("@SectionName", Section);
                cmd.Parameters.AddWithValue("@KeyName", Key);
                cmd.Parameters.AddWithValue("@SettingValue", Setting);
                cmd.ExecuteScalar();
                cmd.Dispose();
                cmd = null;
            }
        }
        else

            // Update

            using (SQLiteConnection con = new SQLiteConnection(GetConString()))
            {
                if (con.State == ConnectionState.Closed)
                    con.Open();
                SQLiteCommand cmd = new SQLiteCommand("update UserSettings Set SettingValue=@SettingValue where SectionName=@SectionName And KeyName=@KeyName And Application=@Application", con);
                cmd.Parameters.AddWithValue("@Application", Application);
                cmd.Parameters.AddWithValue("@SectionName", Section);
                cmd.Parameters.AddWithValue("@KeyName", Key);
                cmd.Parameters.AddWithValue("@SettingValue", Setting);
                cmd.ExecuteScalar();
                cmd.Dispose();
                cmd = null;
            }
    }

    /// <summary>
    /// Check if the setting exist
    /// </summary>
    /// <param name="Application"></param>
    /// <param name="Section"></param>
    /// <param name="Key"></param>
    /// <returns>true or false</returns>
    private static bool SettingExist(string Application, string Section, string Key)
    {
        object Result;
        using (SQLiteConnection con = new SQLiteConnection(GetConString()))
        {
            if (con.State == ConnectionState.Closed) con.Open();

            SQLiteCommand cmd = new SQLiteCommand("Select KeyName from UserSettings where SectionName=@SectionName And KeyName=@KeyName And Application=@Application", con);
            cmd.Parameters.AddWithValue("@Application", Application);
            cmd.Parameters.AddWithValue("@SectionName", Section);
            cmd.Parameters.AddWithValue("@KeyName", Key);
            Result = cmd.ExecuteScalar();
            cmd.Dispose();
            cmd = null;
        }

        if (Result == null)
            return false;
        else
            return System.Convert.ToString(Result) == Key;
    }


    /// <summary>
    /// Create the settings database with one table calles Usersettings.
    /// </summary>
    private static void CreateDatabase()
    {
        try
        {
            string sql;
            if (!System.IO.Directory.Exists(SettingsFolder)) System.IO.Directory.CreateDirectory(SettingsFolder);

            using (SQLiteConnection con = new SQLiteConnection(GetConString()))
            {
                if (con.State == ConnectionState.Closed) con.Open();
                sql = "Create table UserSettings (SectionName NVARCHAR(100)," +
                    "KeyName NVARCHAR(100) , " +
                    "SettingValue NVARCHAR(1024)," +
                    "Application NVARCHAR(100), " +
                    "CONSTRAINT PKUser PRIMARY KEY (SectionName,KeyName,Application))";
                using (SQLiteCommand cmd = new SQLiteCommand(sql, con))
                {
                    cmd.ExecuteNonQuery();
                }

            }
        }
        catch (Exception)
        {
        }
    }
    
    /// <summary>
    /// return the connectionstring
    /// </summary>
    /// <returns></returns>
    private static string GetConString()
    {
        return string.Format("Data Source={0};Encrypt=TRUE", SettingsFile);
    }

    static LocalStorage()
    {
        SettingsFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Firsthope");
        SettingsFile = string.Format("{0}\\{1}-{2}-Settings.db", SettingsFolder, Environment.UserName, SettingsApplication);
    }

}

