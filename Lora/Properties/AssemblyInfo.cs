﻿using System.Reflection;
using System.Runtime.InteropServices;

// Allgemeine Informationen über eine Assembly werden über die folgenden
// Attribute gesteuert. Ändern Sie diese Attributwerte, um die Informationen zu ändern,
// die einer Assembly zugeordnet sind.
[assembly: AssemblyTitle("Lora")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Lora")]
[assembly: AssemblyCopyright("Copyright ©  2018 - 14.04.2019")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Durch Festlegen von ComVisible auf FALSE werden die Typen in dieser Assembly
// für COM-Komponenten unsichtbar.  Wenn Sie auf einen Typ in dieser Assembly von
// COM aus zugreifen müssen, sollten Sie das ComVisible-Attribut für diesen Typ auf "True" festlegen.
[assembly: ComVisible(false)]

// Die folgende GUID bestimmt die ID der Typbibliothek, wenn dieses Projekt für COM verfügbar gemacht wird
[assembly: Guid("85a78b05-5843-4e4d-8c56-4bcb12613750")]

// Versionsinformationen für eine Assembly bestehen aus den folgenden vier Werten:
//
//      Hauptversion
//      Nebenversion
//      Buildnummer
//      Revision
//
// Sie können alle Werte angeben oder Standardwerte für die Build- und Revisionsnummern verwenden,
// indem Sie "*" wie unten gezeigt eingeben:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.8.1")]
[assembly: AssemblyFileVersion("1.8.1")]

/*
 * 1.1.0 Now awaiing Battery as Double and fix the sending twise issue
 * 1.2.0 Restructure and remove old code
 * 1.3.0 Status event and fileds added and refactoring
 * 1.4.0 Implement Height in Lora
 * 1.4.1 Fixing parsing bug with linebreaks
 * 1.4.2 Adding test for LoraBinary
 * 1.5.0 Add support for IC880A board
 * 1.6.0 Fixing binary data transmission
 * 1.6.1 Update to local librarys
 * 1.7.0 Add Parsing for new Statusformat and Panic Packet
 * 1.8.0 Add field that indicates when the last gps position was recieved, change all times to UTC
 * 1.8.1 Add Hostname to MQTT, so you can see from witch device the data is recieved
 */
