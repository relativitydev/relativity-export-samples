using System.Text;
using Relativity.Export.Samples.RelConsole.Helpers;

System.Console.OutputEncoding = Encoding.UTF8;
System.Console.InputEncoding = Encoding.UTF8;

string relativityUrl = "http://host/";
string username = "username";
string password = "password";

await OutputHelper.StartAsync(args, relativityUrl, username, password);


// args
// {number} - Sample ID from the sample list
// -noui disables some UI elements on the initial screen
// -json adds additional JSON output to the console