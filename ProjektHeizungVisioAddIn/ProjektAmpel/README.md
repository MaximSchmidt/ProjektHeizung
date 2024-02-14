# Heizungsprojekt

App zur Überwachung und Steuerung von Heizungsgerät-Daten in Microsoft Visio

## 1. Description

Dieses Projekt implementiert ein intelligentes Heizungssystem, das die Steuerung und Überwachung von Heizgerät-Daten über ein MQTT-Broker ermöglicht. Das System nutzt einen MQTT-Broker für die Kommunikation zwischen einem Visio-basierten Frontend und einem Home Assistenten der einen FritzBox Router integriert.

![Heizungssystem-Übersicht](HeizungsProjekt.png)

## 2. Getting Started

### 2.1. Dependencies

- **Docker**: Windows Desktop
- **Router(Friz Box)**: LAN verbunden im Netzwerk
- **Operating System**: Windows 10
- **.NET Framework**: 6. or higher.
- **Microsoft Visio**: Microsoft Visio 2016 or later (for the Visio Add-In functionality).
- **MQTT Broker**: HiveMQ (or any other MQTT Broker)
- **NuGet Libraries**:
  - Newtonsoft.Json (for handling JSON operations in C#).
  - MQTTnet (for MQTT client and server functionality in C#).

### 2.2. Installing

This project is containerized with Docker. Ensure you have Docker installed on your system before proceeding. Follow these steps to set up the project:

### Step 1: Clone the Repository
```bash
git clone https://github.com/MaximSchmidt/ProjektHeizung.git
```

### Step 2: Install HiveMQ Broker
```bash
$ docker run -d -p 1883:1883 -p 8080:8080 --name hivemq hivemq/hivemq4
```

### Step 3: Install and Configure Home Assistent

Please change the USERNAME according to your Username in Linux

```bash
$ mkdir /home/{USERNAME}/home-assistant-config
```
```bash
$ docker run -d   --name=home-assistant   --restart=unless-stopped  -v /home/{USERNAME}/home-assistant-config:/config   -p 8123:8123   homeassistant/home-assistant:stable
```

For more Info, please refer to the offical Home Assistent Guide on Docker: https://www.home-assistant.io/installation/alternative/


### Step 4: Integrate AVM FRITZ!SmartHome with Home Assistent:

1. Navigate in the Home Assistent Interface to "Einstellungen"
2. Navigate to "Geräte und Dienste"
3. Add Integration "AVM FRITZ!SmartHome"
4. Login with your FritzBox Username and Password


- **DEFAULT HOST: fritz.box**

### Step 5: Integrate MQTT Broker with Home Assistent:

1. Navigate in the Home Assistent Interface to "Einstellungen"
2. Navigate to "Geräte und Dienste"
3. Add Integration "MQTT"
4. Login with your MQTT Username and Password

- **DEFAULT SERVER: 172.17.0.1**
- **DEFAULT PORT: 1883**
- **DEFAULT USERNAME: admin**
- **DEFAULT PASSWORD: hivemq**

### Step 6: Add VSTO-Addin to Visio

1. Open 
2. 

### 2.3. Executing program



## 4. Authors

Contributors names and contact info

ex. MaximSchmidt
ex. [@MaximSchmidt](https://twitter.com/)

## 5. Version History

* 0.2
    * Various bug fixes and optimizations
    * See [commit change]() or See [release history]()
* 0.1
    * Initial Release

## 6. License

This project is licensed under the [MIT] License - see the LICENSE.md file for details

## 7. Acknowledgments

Inspiration, code snippets, etc.
* [awesome-readme](https://github.com/matiassingers/awesome-readme)