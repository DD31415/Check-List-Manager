using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;

public class Settings
{
	public static string CurrentProgramName;
	public static string CurrentDirectory;
	static Settings()
	{
		Settings.CurrentDirectory = Path.GetDirectoryName(Application.ExecutablePath);
		Settings.CurrentProgramName = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
	}
	
	private string _LastFileOpened = String.Empty;
	private string SaveFilePath;
	private List<string> _RecentFileList = new List<string>();
	private int _MaxRecentFiles = 10; 
	private bool _EnableAutoSave = false;
	
	public string LastFileOpened
	{
		get
		{
			return _LastFileOpened;
		}
		set
		{
			if (File.Exists(value) == true || value == String.Empty)
			{
				_LastFileOpened = value;
				this.Save();
			}
		}
	}
	
	public List<string> RecentFileList
	{
		get
		{
			return _RecentFileList;
		}
	}
	
	public int MaxRecentFiles
	{
		get
		{
			return _MaxRecentFiles;
		}
		set
		{
			_MaxRecentFiles = value;
		}
	}
	
	public bool EnableAutoSave
	{
		get
		{
			return _EnableAutoSave;
		}
		set
		{
			_EnableAutoSave = value;
			this.Save();
		}
	}
	
	public Settings()
	{
		if (this.EnsureSettingsFileExists() == true) this.Load();
	}
	
	public Settings(int maxRecentFiles)
	{
		this.MaxRecentFiles = maxRecentFiles;
		if (this.EnsureSettingsFileExists() == true) this.Load();
	}
	
	private bool EnsureSettingsFileExists()
	{
		SaveFilePath = Settings.CurrentDirectory + @"\" + Settings.CurrentProgramName + ".ini";
		if (File.Exists(SaveFilePath) == false)
		{
			File.Create(SaveFilePath).Close();
			return false;
		}
		return true;
	}
	
	public void AddRecentFile(string path)
	{
		if (RecentFileList.Contains(path) == false)
		{
			if (RecentFileList.Count == this.MaxRecentFiles)
			{
				RecentFileList.RemoveAt(RecentFileList.Count - 1);
			}
			RecentFileList.Insert(0, path);
		}
		else
		{
			if (RecentFileList.Count == this.MaxRecentFiles)
			{
				RecentFileList.RemoveAt(RecentFileList.Count - 1);
			}

			RecentFileList.Remove(path);
			RecentFileList.Insert(0, path);
		}
		this.Save();
	}
	
	public void ClearRecentFiles()
	{
		RecentFileList.Clear();
		this.Save();
	}
	
	public void Save()
	{
		StringBuilder str = new StringBuilder();
		str.AppendLine(this.LastFileOpened);
		
		str.AppendLine("---Recent Files---");
		
		for (int i = 0; i < RecentFileList.Count; i++)
		{
			str.AppendLine(RecentFileList[i]);
		}
		
		str.AppendLine("---Bools---");
		str.Append("EnableAutoSave:");
		str.AppendLine(this.EnableAutoSave.ToString());
		
		File.WriteAllText(this.SaveFilePath, str.ToString());
	}
	
	public void Load()
	{
		try
		{
			string[] saveFileLines = File.ReadAllLines(SaveFilePath);
			
			if (saveFileLines.Length > 0)
			{
				if (saveFileLines[0].StartsWith("#") == false)
				{
					this.LastFileOpened = saveFileLines[0];
				}
				else
				{
					this.LastFileOpened = String.Empty;
				}
			}

			if (saveFileLines.Length > 2)
			{
				if (saveFileLines[1] == "---Recent Files---")
				{
					for (int i = 2, c = 0; i < saveFileLines.Length; i++, c++)
					{
						if (c > this.MaxRecentFiles || saveFileLines[i] == "---Bools---") break;
						
						this.RecentFileList.Add(saveFileLines[i]);
					}
				}
			}
			
			int BoolsStart = -1;
			for (int i = 0; i < saveFileLines.Length; i++)
			{
				if (saveFileLines[i] == "---Bools---")
				{
					if (i + 1 < saveFileLines.Length)
					{
						BoolsStart = i + 1;
					}
				}
			}
			
			
			if (BoolsStart != -1)
			{
				
				if (saveFileLines[BoolsStart].StartsWith("EnableAutoSave") == true)
				{
					this.EnableAutoSave = bool.Parse(saveFileLines[BoolsStart].Split(':')[1]);
				}
				
				// for (int i = BoolsStart; i < saveFileLines.Length; i++)
				// {
						// For future use.
				// }
			}	
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.ToString(), "Check List Manager");
		}
	}
}