using System;
using System.Windows.Forms;

public class CheckList
{
	[STAThread]
	public static void Main(string[] args)
	{
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);
		
		if (args.Length > 0)
		{
			if (args.Length > 1 && args[0].ToLower() == "list") Application.Run(new Form1(args));
			else Application.Run(new Form1(args[0]));
		}
		else
		{
			Application.Run(new Form1());
		}
	}
}