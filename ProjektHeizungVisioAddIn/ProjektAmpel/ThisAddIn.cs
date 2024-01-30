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

namespace ProjektAmpel
{
    public partial class ThisAddIn
    {
        private IMqttClient mqttClient;

        private async void ThisAddIn_Startup(object sender, EventArgs e)
        {
            var Server = "localhost";
            int Port = 1883;

            try
            {
                // Broker-Verbindung konfigurieren
                var factory = new MqttFactory();
                mqttClient = factory.CreateMqttClient();

                var options = new MQTTnet.Client.Options.MqttClientOptionsBuilder()
                    .WithTcpServer(Server, Port)
                    .Build();

                await mqttClient.ConnectAsync(options);

                // Themen abonnieren
                await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("/Heizungen/").Build());
                mqttClient.UseApplicationMessageReceivedHandler(HandleReceivedMessage);


                Visio.Application visioApp = Globals.ThisAddIn.Application;
                visioApp.Documents.Open("C:\\Users\\maxim.schmidt\\Documents\\Zeichnung3.vsdm");

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
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

                foreach (Visio.Shape shape in page.Shapes)
                {
                    try
                    {
                        var exist = shape.CellExistsU["Prop.Heizungen", 0];
                        if (exist !=0)
                        {
                            string heaterValue = shape.CellsU["Prop.Heizungen"].ResultStrU[Visio.VisUnitCodes.visNoCast];
                            if (heaterValue == heaterData.ID)
                            {

                                    var exist1 = shape.CellExistsU["Prop.Temperatur", 0];
                                    if (exist1 != 0)

                                {
                                    Debug.WriteLine($"Setze 'Prop.Temperatur' auf: {heaterData.Temperatur}");
                                    shape.CellsU["Prop.Temperatur"].FormulaU = $"\"{heaterData.Temperatur}\"";
                                    break; // Shape gefunden und aktualisiert, Schleife verlassen
                                }
                                else
                                {
                                    Debug.WriteLine($"Shape mit 'Prop.Heizungen' = '{heaterData.ID}' gefunden, aber es hat keine 'Prop.Temperatur' Eigenschaft.");
                                }
                            }
                        }
                    }
                    catch (COMException comEx)
                    {
                        Debug.WriteLine($"COMException beim Zugriff auf die Zellen des Shapes: {comEx.Message}");
                    }
                }
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

        // Hilfsklasse zur Deserialisierung der JSON-Daten
        public class HeaterData
        {
            public string ID { get; set; }
            public string Temperatur { get; set; }
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