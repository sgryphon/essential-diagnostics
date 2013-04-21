using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Data.SqlClient;

namespace Essential.Diagnostics.RegSql
{
    class Program
    {
        const string InstallTraceScript = "InstallTrace.sql";
        const string UninstallTraceScript = "UninstallTrace.sql";
        static TraceSource trace = new TraceSource("Diagnostics.RegSql");

        static int Main(string[] args)
        {
            var named = ParseNamedArgs(args);
            if (args.Length == 0 || named.ContainsKey("-?"))
            {
                WriteUsage();
                return -1;
            }

            // Get arg values
            string server;
            if (!named.TryGetValue("-S", out server))
            {
                server = "."; // Default to local server
            }

            string connectionString;
            if (!named.TryGetValue("-C", out connectionString))
            {
                connectionString = null;
            }

            var trusted = named.ContainsKey("-E");

            string username;
            string password;
            if (!named.TryGetValue("-U", out username))
            {
                username = null;
            }
            if (!named.TryGetValue("-P", out password))
            {
                password = null;
            }

            string database;
            if (!named.TryGetValue("-d", out database))
            {
                database = "diagnosticsdb";
            }

            string exportFile;
            if (!named.TryGetValue("-sqlexportonly", out exportFile))
            {
                exportFile = null;
            }

            bool install = named.ContainsKey("-AD");
            bool uninstall = named.ContainsKey("-RD");

            if (VerifyOptions(install, uninstall, connectionString, server, trusted, username, password))
            {
                if (!string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password))
                {
                    ReadPasswordFromConsole();
                }

                string script;
                if (install)
                {
                    script = InstallTraceScript;
                }
                else
                {
                    if (!named.ContainsKey("-Q") && !ConfirmRemove(database))
                    {
                        return -1;
                    }
                    script = UninstallTraceScript;
                }

                RunScriptWithOptions(script, connectionString, server, trusted, username, password, database, exportFile);
            }
            else
            {
                return -1;
            }

            return 0;
        }

        private static bool VerifyOptions(bool install, bool uninstall, string connectionString, string server, bool trusted, string username, string password)
        {
            if (install && uninstall)
            {
                Console.WriteLine("ERROR: Conflicting action arguments.");
                return false;
            }
            if (!install && !uninstall)
            {
                Console.WriteLine("ERROR: Must specify action (either add or remove diagnostics).");
                return false;
            }

            if (!string.IsNullOrEmpty(password) && string.IsNullOrEmpty(username))
            {
                Console.WriteLine("ERROR: Cannot specify a password without a username.");
                return false;
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                if (trusted)
                {
                    if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password))
                    {
                        // Okay -- trusted only
                    }
                    else
                    {
                        Console.WriteLine("ERROR: Cannot specify both a username and a trusted connection.");
                        return false;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(username))
                    {
                        Console.WriteLine("ERROR: Must specify either a trusted connection, username, or a complete connection string.");
                        return false;
                    }
                    else
                    {
                        // Okay -- user name only
                    }
                }
            }
            else
            {
                if (trusted || !string.IsNullOrEmpty(username) || !string.IsNullOrEmpty(password) || !string.IsNullOrEmpty(server))
                {
                    Console.WriteLine("ERROR: Cannot specify both a connection string and connection options.");
                    return false;
                }
            }

            return true;
        }

        private static bool ConfirmRemove(string database)
        {
            Console.Write("Are you sure you want to remove the diagnostics from database '{0}' [Y/N]? ", database);
            var key = Console.ReadKey();
            Console.WriteLine();
            return key.KeyChar == 'y' || key.KeyChar == 'Y';
        }

        private static string ReadPasswordFromConsole()
        {
            Console.Write("Password: ");
            var password = new StringBuilder();
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }
                if (key.Key == ConsoleKey.Backspace)
                {
                    password.Remove(password.Length - 1, 1);
                    Console.Write(key.KeyChar);
                    Console.Write(" ");
                    Console.Write(key.KeyChar);
                }
                else
                {
                    password.Append(key.KeyChar);
                    Console.Write("*");
                }
            }
            Console.WriteLine();
            return password.ToString();
        }

        private static IDictionary<string,string> ParseNamedArgs(string[] args)
        {
            var named = new Dictionary<string, string>();
            string argName = null;
            string argValue = null;
            foreach (var arg in args)
            {
                if (arg.StartsWith("-", StringComparison.OrdinalIgnoreCase))
                {
                    if (argName != null)
                    {
                        named[argName] = argValue;
                        trace.TraceEvent(TraceEventType.Verbose, 1, "Parsed arg '{0}' with value '{1}'", argName, argValue);
                    }
                    argName = arg;
                    argValue = "";
                }
                else
                {
                    if (argName == null)
                    {
                        argName = string.Empty; // unnamed args
                    }
                    if (argValue.Length > 0)
                    {
                        argValue += " ";
                    }
                    argValue += arg;
                }
            }
            if (argName != null)
            {
                named[argName] = argValue;
                trace.TraceEvent(TraceEventType.Verbose, 1, "Parsed arg '{0}' with value '{1}'", argName, argValue);
            }
            return named;
        }

        static void WriteUsage()
        {
            Console.WriteLine(Resources.HelpText);
        }

        static void RunScriptWithOptions(string script, string connectionString, string server, bool trusted, string user, string password, string database, string exportFile)
        {
            // TODO: Check if tables already exist (i.e. already deployed).

            var assembly = Assembly.GetExecutingAssembly();
            var assemblyDirectory = Path.GetDirectoryName(assembly.Location);
            var scriptPath = Path.Combine(assemblyDirectory, script);
            var scriptText = File.ReadAllText(scriptPath);

            scriptText = scriptText.Replace("'diagnosticsdb'", "'" + database + "'");
            scriptText = scriptText.Replace("[diagnosticsdb]", "[" + database + "]");

            if (!string.IsNullOrEmpty(exportFile))
            {
                trace.TraceEvent(TraceEventType.Information, 2000, "Exporting script '{0}' to file '{1}'.", script, exportFile);
                File.WriteAllText(exportFile, scriptText);
                return;
            }

            trace.TraceEvent(TraceEventType.Information, 1000, "Running script '{0}' against database '{1}'.", script, database);

            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = ConstructConnectionString(server, user, password, trusted);
            }
            trace.TraceEvent(TraceEventType.Information, 1001, "Connection string: \"{0}\"", connectionString);

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand(null, connection);
                var reader = new StringReader(scriptText);
                var line = string.Empty;
                var section = new StringBuilder();
                while (line != null)
                {
                    line = reader.ReadLine();
                    if (line == null || string.Equals(line.Trim(), "GO", StringComparison.OrdinalIgnoreCase))
                    {
                        if (section.Length > 0)
                        {
                            command.CommandText = section.ToString();
                            trace.TraceEvent(TraceEventType.Verbose, 2, "Executing command: {0}", command.CommandText);
                            command.ExecuteNonQuery();

                            section = new StringBuilder();
                        }
                    }
                    else
                    {
                        section.AppendLine(line);
                    }
                }
            }

            trace.TraceEvent(TraceEventType.Information, 8000, "Script '{0}' complete.", script);
        }

        private static string ConstructConnectionString(string server, string user, string password, bool trusted)
        {
            string str = "server=" + server;
            if (trusted)
            {
                return (str + ";Trusted_Connection=true;");
            }
            return (str + ";UID=" + user + ";PWD=" + password + ";");
        }


    }
}
