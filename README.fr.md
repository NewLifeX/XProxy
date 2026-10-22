# XProxy - Serveur proxy

![GitHub License](https://img.shields.io/github/license/NewLifeX/XProxy?logo=github)
![.NET](https://img.shields.io/badge/.NET-net10.0-512BD4?logo=dotnet)
![NativeAOT](https://img.shields.io/badge/NativeAOT-ready-brightgreen?logo=dotnet)
![platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-blue)
[![contributors](https://img.shields.io/github/contributors/NewLifeX/XProxy?logo=github)](https://github.com/NewLifeX/XProxy/graphs/contributors)

<!-- lang:start -->
🌐 [English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [Español](README.es.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [Português](README.pt.md) | [Русский](README.ru.md) | [한국어](README.ko.md) | [العربية](README.ar.md)
<!-- lang:end -->

**XProxy** est un hôte de serveurs proxy de l'écosystème NewLife. Un seul processus héberge plusieurs services `ProxyBase`, notamment le proxy inverse NAT, le proxy direct HTTP et le proxy inverse HTTP. Il s'appuie sur la pile réseau haute performance NewLife.Net, s'installe comme service Windows ou démon Linux via NewLife.Agent, se publie avec NativeAOT et localise ses messages d'interface via NewLife.Core.

> ℹ️ Cette traduction peut être obsolète ; le [English README](README.md) fait foi.

![XProxy console](Doc/Demo.png)

**Sommaire** : [Démarrage rapide](#démarrage-rapide) · [Fonctionnalités](#fonctionnalités) · [Configuration](#configuration) · [NativeAOT](#nativeaot) · [Localisation](#localisation) · [Carte de l'écosystème](#carte-de-lécosystème) · [Compilation et tests](#compilation-et-tests) · [Équipe](#équipe) · [Licence](#licence)

## Démarrage rapide

Le SDK .NET 10 est requis.

```bash
git clone https://github.com/NewLifeX/XProxy.git
cd XProxy
dotnet run --project XProxy
```

Au premier lancement, XProxy crée `Config/XProxy.config` et démarre les proxys de démonstration :

| Démo | Fournisseur | Écoute | Cible |
| --- | --- | --- | --- |
| Demo1 | NAT | `tcp://0.0.0.0:3388` | `tcp://localhost:3389` |
| Demo2 | Http | `tcp://0.0.0.0:1080` | Proxy direct HTTP (authentification Basic `user=` / `pass=newlife`) |
| Demo3 | HttpIp | `tcp://0.0.0.0:8080` | `tcp://www.newlifex.com:80` |
| Demo4 | HttpReverse | `tcp://0.0.0.0:8081` | `tcp://www.newlifex.com:80` |

La console interactive permet d'installer le programme comme service Windows (`-i`) ou de simuler son exécution (`-run`). L'installation nécessite des droits administrateur.

## Fonctionnalités

- **Proxy inverse NAT** : transfère n'importe quel port TCP ou UDP vers un serveur cible, avec ports multiples et vérification de préfixe facultatifs.
- **Proxy direct HTTP** : accepte les requêtes de forme absolue (`GET http://...` et `GET https://...`), prend en charge l'authentification Basic, supprime les en-têtes propres au proxy et transfère vers des cibles HTTP ou HTTPS selon le schéma.
- **Proxy inverse HTTP** : réécrit `Host` et `Referer` pour les sites publiés en inverse.
- **Sortie multi-IP** : `HttpIp` choisit l'adresse source locale selon les modes `any`, `round`, `random` ou `origin`.
- **Hébergement de service** : s'exécute comme service Windows, unité systemd ou application console via NewLife.Agent.
- **Traçage distribué** : envoie les traces à Stardust lorsqu'un serveur est configuré (builds JIT ; désactivé sous NativeAOT).
- **Configuration à chaud** : recharge `Config/XProxy.config` en moins de dix secondes.
- **NativeAOT et localisation** : se publie comme exécutable natif autonome ; les messages suivent la langue sélectionnée.

## Configuration

`Config/XProxy.config` est généré au premier lancement :

```xml
<Setting>
  <Debug>true</Debug>
  <Items>
    <ProxyItem Name="Web" Provider="HttpReverse" Enable="true"
               Local="tcp://0.0.0.0:8081" Remote="tcp://www.newlifex.com:80" Config="" />
  </Items>
  <Language>fr</Language>
</Setting>
```

`Provider` accepte `NAT`, `Http`, `HttpIp` et `HttpReverse`. `Config` dépend du fournisseur :

| Fournisseur | Clés de configuration |
| --- | --- |
| `NAT` | `Ports` (nombre de ports consécutifs), `Prefix` (vérification de préfixe hexadécimal) |
| `Http` | `user`, `pass` et `realm` pour l'authentification Basic du proxy |
| `HttpIp` | `IpMode` = `Any` / `Round` / `Random` / `Origin` |

## NativeAOT

```bash
# ligne de commande
dotnet publish XProxy/XProxy.csproj -c Release -r win-x64 -p:PublishAot=true

# ou le profil de publication fourni
dotnet publish XProxy/XProxy.csproj -c Release -p:PublishProfile=win-x64-aot
```

L'exécutable publié contient le runtime .NET et ne nécessite aucune installation séparée. Le transfert, la configuration et l'hébergement de service fonctionnent sous NativeAOT. Le traçage Stardust est désactivé car il repose sur la réflexion dynamique ; XProxy affiche un message au démarrage sans traçage.

## Localisation

- L'anglais est la langue source et de repli. Les catalogues chinois simplifié et chinois traditionnel sont fournis dans `XProxy/Locales` ; les autres langues reviennent à l'anglais, tandis que NewLife.Core intègre 31 langues.
- Définissez `Language` dans `Config/XProxy.config` (par exemple `en`, `zh-CN`, `zh-TW`) ou appelez `NewLife.Setting.Current.Language`. Une valeur vide suit la langue de l'interface du système.
- Ajoutez `XProxy/Locales/<culture>.yml` pour étendre le catalogue ; les clés sont les chaînes anglaises exactes.

<!-- ecosystem:start -->
## Carte de l'écosystème

NewLife est un écosystème en couches de projets open source versionnés indépendamment. La carte ci-dessous est reprise à l'identique par tous les dépôts NewLife ; chaque projet renvoie à son dépôt et à son paquet NuGet principal.

| Projet | Rôle | Téléchargements |
| --- | --- | --- |
| **Fondation** | | |
| [NewLife.Core](https://github.com/NewLifeX/X) | Journalisation, configuration, cache, réseau, sérialisation, ordonnancement et traçage APM. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Core?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Core) |
| [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | Intergiciel de données : tables uniques à l'échelle de dizaines de milliards, partitionnement automatique, séparation lecture/écriture et plus de 10 bases de données. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCode?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCode) |
| [NewLife.Net](https://github.com/NewLifeX/NewLife.Net) | Bibliothèque réseau hautes performances : 1,77 M pps / 14,5 Gbps sur une machine (TCP 1KB, 2026-10), 4 M connexions TCP persistantes. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Net?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Net) |
| [NewLife.Remoting](https://github.com/NewLifeX/NewLife.Remoting) | Communication protocolaire et framework RPC pour applications client/serveur et appareils IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Remoting?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Remoting) |
| [NewLife.Agent](https://github.com/NewLifeX/NewLife.Agent) | Installe des applications .NET comme démons système : services Windows et systemd Linux. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Agent?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Agent) |
| [NewLife.Templates](https://github.com/NewLifeX/NewLife.Zero) | Modèles de projet `dotnet new` pour console, web, API, service, données et IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Templates?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Templates) |
| **Données et messagerie** | | |
| [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | Client Redis avec une latence en microsecondes, des millions d'opérations/s et des files de messages intégrées. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Redis?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Redis) |
| [NewLife.RocketMQ](https://github.com/NewLifeX/NewLife.RocketMQ) | Client RocketMQ entièrement managé pour Apache RocketMQ et Alibaba Cloud Message Queue. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.RocketMQ?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.RocketMQ) |
| [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | Client et serveur MQTT, y compris la connectivité Alibaba Cloud IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MQTT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MQTT) |
| [NewLife.NovaDb](https://github.com/NewLifeX/NewLife.NovaDb) | Base de données hybride en C# : embarquée ou en réseau, relationnelle, séries temporelles, file de messages et KV réunis. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.NovaDb?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.NovaDb) |
| [NewLife.MySql](https://github.com/NewLifeX/NewLife.MySql) | Pilote et couche d'assistance MySql construits sur la pile de données NewLife. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MySql?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MySql) |
| **Web, IA et distribué** | | |
| [NewLife.Cube](https://github.com/NewLifeX/NewLife.Cube) | Framework rapide de plateforme d'administration : permissions, SSO, serveur OAuth et échafaudage CRUD. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Cube?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Cube) |
| [Stardust](https://github.com/NewLifeX/Stardust) | Plateforme de services distribués : gestion des nœuds, APM, centre de configuration, registre et centre de publication. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Stardust?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Stardust) |
| [AntJob](https://github.com/NewLifeX/AntJob) | Plateforme de tâches distribuées et d'ordonnancement de données en temps réel avec partitionnement, reprises et backfill. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AntJob?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AntJob) |
| [NewLife.AI](https://github.com/NewLifeX/NewLife.AI) | Client unifié pour 46 fournisseurs de LLM, plus ChatAI, une application web de chat prête à l'emploi. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AI?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AI) |
| **Industriel et IoT** | | |
| [NewLife.IoT](https://github.com/NewLifeX/NewLife.IoT) | Bibliothèque standard IoT définissant les contrats de protocole et les abstractions d'appareils. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IoT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IoT) |
| [NewLife.Modbus](https://github.com/NewLifeX/NewLife.Modbus) | Implémentation Modbus TCP/RTU/ASCII au-dessus de NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Modbus?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Modbus) |
| [NewLife.Siemens](https://github.com/NewLifeX/NewLife.Siemens) | Protocole pour automate Siemens au-dessus de NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Siemens?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Siemens) |
| [NewLife.Omron](https://github.com/NewLifeX/NewLife.Omron) | Protocole pour automate Omron au-dessus de NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Omron?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Omron) |
| [NewLife.Melsec](https://github.com/NewLifeX/NewLife.Melsec) | Protocole pour automate Mitsubishi Melsec au-dessus de NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Melsec?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Melsec) |
| [NewLife.OPC](https://github.com/NewLifeX/NewLife.OPC) | Client OPC pour l'accès aux données industrielles. | — |
| [NewLife.BACnet](https://github.com/NewLifeX/NewLife.BACnet) | Pile de protocoles BACnet pour l'automatisation des bâtiments. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.BACnet?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.BACnet) |
| [ZeroIoT](https://github.com/NewLifeX/ZeroIoT) | Plateforme IoT open source légère avec gestion des passerelles et des appareils. | — |
| **Télématique embarquée** | | |
| [NewLife.JT808](https://github.com/NewLifeX/NewLife.JT808) | Protocole de positionnement de véhicules JT/T 808 : plus de 70 corps de message, 2011/2013/2019. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT808?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT808) |
| [NewLife.JT809](https://github.com/NewLifeX/NewLife.JT809) | Protocole plateforme à plateforme JT/T 809 pour les autorités de transport. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT809?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT809) |
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | Vidéo embarquée JT/T 1078 : analyse RTP, remuxage FLV/fMP4 et diffusion en direct. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT1078?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT1078) |
| **Composants métier** | | |
| [NewLife.Office](https://github.com/NewLifeX/NewLife.Office) | Un seul paquet pour Excel, Word, PPT, PDF et plus de 15 formats de documents. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Office?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Office) |
| [NewLife.Audio](https://github.com/NewLifeX/NewLife.Audio) | Audio multiplateforme : codecs, E/S d'appareils, DSP, conteneurs et streaming. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Audio?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Audio) |
| [NewLife.Video](https://github.com/NewLifeX/NewLife.Video) | Codecs vidéo, formats de conteneurs, traitement des images et streaming. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Video?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Video) |
| [NewLife.Map](https://github.com/NewLifeX/NewLife.Map) | Interface unifiée pour Baidu, AMap, Tencent et Tianditu avec géocodage et conversion de coordonnées. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Map?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Map) |
| [NewLife.IP](https://github.com/NewLifeX/NewLife.IP) | Géolocalisation IPv4 avec une base de données hors ligne mappée en mémoire et une API en ligne en secours. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IP?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IP) |
| **Outils et produits** | | |
| [CrazyCoder](https://github.com/NewLifeX/XCoder) | Boîte à outils du développeur : réseau, port série, cryptographie, regex, Modbus et MQTT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCoder?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCoder) |
| [GitCandy](https://github.com/NewLifeX/GitCandy) | Système de gestion de code source Git auto-hébergé. | — |
| [LuckyClover](https://github.com/NewLifeX/LuckyClover) | Installe automatiquement le runtime .NET correspondant lorsqu'il est lancé depuis un hôte plus ancien. | — |

> Les badges indiquent le paquet principal de chaque projet ; les paquets de thèmes, d'extensions et satellites sont répertoriés sur leurs pages NuGet. Les plus de 30 paquets non listés ici (FTP, LoRa, extensions Office, pilotes de protocoles, etc.) sont disponibles dans l'[organisation NewLifeX](https://github.com/NewLifeX) et sur [newlifex.com](https://newlifex.com).
<!-- ecosystem:end -->

## Compilation et tests

```bash
dotnet build XProxy.slnx -c Release
```

`Test` est une console d'intégration manuelle qui envoie des requêtes via un proxy HTTP en cours d'exécution. Ce dépôt ne contient pas de projet de tests automatisés ; la construction des en-têtes HTTP est couverte par les tests unitaires de NewLife.Core.

Le format de version est `3.2.yyyyMMdd`. La sortie de compilation se trouve dans `Bin/Server`.

<!-- team:start -->
## Équipe

<img src="https://newlifex.com/logo.png" width="48" align="right" alt="NewLife" />

NewLife a été fondée en 2002 et œuvre depuis plus de deux décennies dans l'écosystème .NET et IoT, maintient près de 100 projets open source (dont 34 principaux) et fournit des solutions matérielles et logicielles intégrées aux secteurs de l'énergie, de la logistique, de la santé, du contrôle industriel et d'Internet. Notre devise : « l'apprentissage ne connaît pas l'ancienneté — celui qui maîtrise devient maître ».

Les composants open source ont été téléchargés <!--nuget-total:start-->6.48M<!--nuget-total:end--> fois depuis NuGet (à l'échelle de l'écosystème, en date de 2026-10).

Site web : https://newlifex.com · Organisation open source : https://github.com/NewLifeX

<details>
<summary>Compte officiel WeChat : 智能大石头 (scannez pour suivre)</summary>

![智能大石头](https://newlifex.com/stone.jpg)

</details>
<!-- team:end -->

## Licence

Ce projet est distribué sous licence [MIT](LICENSE). Le code source peut être modifié et redistribué sans attribution.

> Si ce projet vous est utile, ajoutez-lui une Star, partagez-le avec votre équipe ou proposez des améliorations via Issue/PR.
