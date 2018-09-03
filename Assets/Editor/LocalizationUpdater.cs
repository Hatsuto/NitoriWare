﻿using Google.GData.Spreadsheets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

[CreateAssetMenu(menuName = "Localization/Localization Updater")]
[ExecuteInEditMode]
public class LocalizationUpdater : ScriptableObject
{
    private const string KeyIdentifier = "key";
    private const string CharsFileSuffix = "Chars.txt";
    private const string AllCharsFile = "AllChars.txt";
    private const string NonAsianCharsFile = "NonAsianChars.txt";

    [SerializeField]
    private LanguagesData languagesData;

    [SerializeField]
    private string spreadsheetId;
    [SerializeField]
    private int subsheetCount;

    //The feed likes to convert the first row to lowercase so the second row on the first subsheet has a buffer for that
    [SerializeField]
    private string idNameKey;

    [SerializeField]
    private string languagesPath;
    [SerializeField]
    private string charsPath;


    public void updateLanguages()
    {
        var languages = new Dictionary<string, SerializedNestedStrings>();
        for (int i = 1; i <= subsheetCount; i++)
        {
            var sheet = GDocService.GetSpreadsheet(spreadsheetId, i);
            if (i == 1)
                languages = generateLanguageDict(sheet);

            foreach (ListEntry row in sheet.Entries)
            {
                string rowKey = "";
                foreach (ListEntry.Custom element in row.Elements)
                {
                    if (element.LocalName.Equals(KeyIdentifier))
                        rowKey = element.Value;
                    else if (languages.ContainsKey(element.LocalName) && !string.IsNullOrEmpty(element.Value))
                    {

                        languages[element.LocalName][rowKey] = cleanseEntry(element.Value);
                    }
                }
            }
        }

        string fullLanguagesPath = Path.Combine(Application.dataPath, languagesPath);
        foreach (var languageData in languages)
        {
            string name = getLanguageIdName(languageData.Value);
            File.WriteAllText(Path.Combine(fullLanguagesPath, name), languageData.Value.ToString());
        }

        Debug.Log("Language content updated");
	}

    public void updateCharsFiles()
    {
        string fullLanguagesPath = Path.Combine(Application.dataPath, languagesPath);
        string fullCharsPath = Path.Combine(Application.dataPath, charsPath);

        //Language files
        var languageFiles = languagesData.languages.Select(a => Path.Combine(fullLanguagesPath, a.getFileName())).Distinct();
        foreach (var languageFile in languageFiles)
        {
            string language = Path.GetFileName(languageFile);
            File.WriteAllText(Path.Combine(fullCharsPath, language + CharsFileSuffix), getUniqueCharString(File.ReadAllLines(languageFile)));
        }
        Debug.Log("Language chars updated");

        //All chars
        var allChars = languageFiles.Select(a => getUniqueCharString(File.ReadAllLines(a))).SelectMany(a => a).Distinct().ToArray();
        File.WriteAllText(Path.Combine(fullCharsPath, AllCharsFile), new string(allChars));
        Debug.Log(AllCharsFile + " updated");

        //Non-Asian chars
        languageFiles = languagesData.languages.Where(a => !a.isAsian).Select(a => Path.Combine(fullLanguagesPath, a.getFileName())).Distinct();
        allChars = languageFiles.Select(a => getUniqueCharString(File.ReadAllLines(a))).SelectMany(a => a).Distinct().ToArray();
        File.WriteAllText(Path.Combine(fullCharsPath, NonAsianCharsFile), new string(allChars));
        Debug.Log(NonAsianCharsFile + " updated");
    }

    static string getUniqueCharString(string[] languageLines)
    {
        var str = new string((from line in languageLines
                              where line.Contains("=")
                              select line.Substring(line.IndexOf('=') + 1))
                .SelectMany(a => a).Distinct().ToArray());
        return str;
    }

    string cleanseEntry(string value)
    {
        value = Regex.Replace(value, @"^\n*", "");  //Remove leading line breaks
        value = Regex.Replace(value, @"\n*$", "");  //Remove trailing line breaks
        value = Regex.Replace(value, @"^ *", "");   //Remove leading whitespace
        value = Regex.Replace(value, @" *$", "");   //Remove trailing whitespace
        value = value.Replace("\n", "\\n");         //Format line breaks

        //TODO Re-implement parameter count

        return value;
    }

    //Use the second row sheet buffer to get proper codenames for langauges
    string getLanguageIdName(SerializedNestedStrings languageData)
    {
        return languageData[idNameKey];
    }

    Dictionary<string, SerializedNestedStrings> generateLanguageDict(ListFeed sheet)
    {
        var firstRow = (ListEntry)sheet.Entries.FirstOrDefault();
        var returnDict = new Dictionary<string, SerializedNestedStrings>();
        foreach (ListEntry.Custom element in firstRow.Elements)
        {
            if (element.LocalName.Equals(KeyIdentifier))
                continue;
            returnDict[element.LocalName] = new SerializedNestedStrings();
        }

        return returnDict;
    }

}
