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
            visioApp.Documents.Open("C:\\Users\\maxim.schmidt\\Documents\\Zeichnung3.vsdm");
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
            if (visioApp == null)
            {
                Debug.WriteLine("Visio-Application-Objekt ist null");
                return;
            }

            Visio.Document activeDocument = visioApp.ActiveDocument;
            if (activeDocument == null)
            {
                Debug.WriteLine("Kein aktives Dokument");
                return;
            }

            Visio.Page page;
            try
            {
                page = activeDocument.Pages["Zeichenblatt-1"];
            }
            catch (Exception)
            {
                Debug.WriteLine("Seite nicht gefunden");
                return;
            }

            // JSON-Nachricht deserialisieren
            try
            {
                var heaterData = JsonConvert.DeserializeObject<HeaterData>(messagePayload);
                // Erweitern Sie hier Ihren Code, um die spezifischen Werte anzuzeigen
                DisplayJsonInShape(page, messagePayload);
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

        private void DisplayJsonInShape(Visio.Page page, string jsonString)
        {
            foreach (Visio.Shape shape in page.Shapes)
            {
                if (shape.NameU == "Sheet.44") 
                {
                    shape.Text = jsonString;
                    break;
                }
            }
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