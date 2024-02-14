using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Visio = Microsoft.Office.Interop.Visio;
using Office = Microsoft.Office.Core;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using MQTTnet.Extensions;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Channel;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Office.Tools.Ribbon;
using Microsoft.Office.Tools;
using System.Diagnostics;
using Newtonsoft.Json;
using MQTTnet.Client.Options;

namespace ProjektAmpel
{
    public partial class ThisAddIn
    {
        private IMqttClient mqttClient;

        private void ThisAddIn_Startup(object sender, EventArgs e)
        {
            Visio.Application visioApp = Globals.ThisAddIn.Application;
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string fileName = "Heizung.vsdm";
            string filePath = System.IO.Path.Combine(folderPath, fileName);

            // Überprüfen, ob die Datei bereits existiert
            if (System.IO.File.Exists(filePath))
            {
                // Öffnet das vorhandene Dokument
                visioApp.Documents.Open(filePath);
            }
            else
            {
                // Erstellt ein neues Dokument, wenn es nicht existiert
                visioApp.Documents.Add("");
                // Optional: Speichern des neuen Dokuments
                Visio.Document newDoc = visioApp.ActiveDocument;
                newDoc.SaveAs(filePath);
            }
        }

        protected override Office.IRibbonExtensibility CreateRibbonExtensibilityObject()
        {
            return new Ribbon1();
        }

        public async Task ConnectToBroker()
        {
            try
            {
                // Broker-Verbindung konfigurieren
                var factory = new MqttFactory();
                mqttClient = factory.CreateMqttClient();

                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer("localhost", 1883)
                    .Build();

                await mqttClient.ConnectAsync(options);
                await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("home/climate/entwicklung").Build());
                mqttClient.UseApplicationMessageReceivedHandler(HandleReceivedMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public async Task DisconnectFromBroker()
        {
            if (mqttClient != null)
            {
                await mqttClient.DisconnectAsync();
            }
        }


        private void HandleReceivedMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            string messagePayload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            Visio.Application visioApp = Globals.ThisAddIn.Application;
            if (visioApp == null || visioApp.ActiveDocument == null)
            {
                Debug.WriteLine("Visio-Application-Objekt ist null oder kein aktives Dokument");
                return;
            }

            // Verwenden Sie das aktive Dokument, anstatt nach "Zeichenblatt-1" zu suchen
            Visio.Page activePage = visioApp.ActivePage;
            if (activePage == null)
            {
                Debug.WriteLine("Keine aktive Seite gefunden");
                return;
            }

            try
            {
                var heaterData = JsonConvert.DeserializeObject<HeaterData>(messagePayload);
                Visio.Shape shape = GetOrCreateShape(activePage, "HeizungShape");
                shape.Text = messagePayload; // Aktualisiert das Shape mit dem JSON-String
            }
            catch (JsonException jsonEx)
            {
                Debug.WriteLine("Fehler beim Deserialisieren der JSON-Nachricht: " + jsonEx.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Allgemeiner Fehler: " + ex.Message);
            }
        }

        private Visio.Shape GetOrCreateShape(Visio.Page page, string shapeName)
        {
            foreach (Visio.Shape shape in page.Shapes)
            {
                if (shape.NameU == shapeName)
                {
                    return shape; // Shape gefunden
                }
            }

            // Shape nicht gefunden, also neues Shape erstellen
            Visio.Shape newShape = page.DrawRectangle(1, 1, 2, 2); // Beispielposition und -größe
            newShape.NameU = shapeName;
            return newShape;
        }



        // Hilfsklasse zur Deserialisierung der JSON-Daten
        public class HeaterData
        {
            public string ID { get; set; }
            public string IstTemperatur { get; set; }
            public string SollTemperatur { get; set; }
        }

        private void ThisAddIn_Shutdown(object sender, EventArgs e)
        {
            // Hier kannst du Code hinzufügen, der beim Herunterfahren des Add-Ins ausgeführt wird
        }

        #region Von VSTO generierter Code

        private void InternalStartup()
        {
            this.Startup += new EventHandler(ThisAddIn_Startup);
            this.Shutdown += new EventHandler(ThisAddIn_Shutdown);
        }

        #endregion
    }
}