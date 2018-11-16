using eFormData;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsServiceOutlookPlugin;
using WindowsServiceOutlookPlugin.Helpers;

namespace SourceCode
{
    class Program
    {

        static void Main(string[] args)
        {
            string serverConnectionString = "";
            //string fakedServiceName = "MicrotingOdense";
            Console.WriteLine("Enter database to use:");
            Console.WriteLine("> If left blank, it will use 'Microting'");
            Console.WriteLine("  Enter name of database to be used");
            string databaseName = Console.ReadLine();

            if (databaseName.ToUpper() != "")
                serverConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=" + databaseName + ";Integrated Security=True";
            if (databaseName.ToUpper() == "T")
                serverConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=" + "MicrotingTest_Outlook" + ";Integrated Security=True";
            if (databaseName.ToUpper() == "O")
                serverConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=" + "MicrotingOdense_Outlook" + ";Integrated Security=True";
            if (serverConnectionString == "")
                serverConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=" + "MicrotingSourceCode_Outlook" + ";Integrated Security=True";

            Core outCore = new Core();
            //serveiceLogic.OverrideServiceLocation("c:\\microtingservice\\" + fakedServiceName + "\\");
            InstallCA();
            outCore.Start(serverConnectionString, GetServiceLocation());
            Console.WriteLine("Press any key to close");
            Console.ReadKey();
            outCore.Stop(false);

            
        }

        static void Stump(object sender, EventArgs args)
        {
            Console.WriteLine("EVENT:" + sender.ToString());
        }

        static string GetServiceLocation()
        {
            string serviceLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            serviceLocation = Path.GetDirectoryName(serviceLocation) + "\\";

            return serviceLocation;
        }

        static void InstallCA()
        {
            string certsFolder = Path.Combine(GetServiceLocation(), "cert");
            Directory.CreateDirectory(certsFolder);
            CertHelper.GenerateSelfSignedCert(GetServiceLocation().Split('\\').Last(), "key.cer", "cert.pfx", certsFolder);
        }
    }
}