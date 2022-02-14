
/* ----------------------------------------------------------------------------
 * "THE BEER-WARE LICENSE" (Revision 42):
 * <gustavwostrack@web.de> wrote this file. As long as you retain this
 * notice you can do whatever you want with this stuff. If we meet some day,
 * and you think this stuff is worth it, you can buy me a beer in return
 * Gustav Wostrack
 * ----------------------------------------------------------------------------
 */

#include <Arduino.h>
#include "WiFiAdr.h"
#include <WiFi.h>
#include "esp_timer.h"
#include "ticker.h"
#include "EEPROM.h"

// EEPROM-Adressen
const uint16_t eepromAdr = 0x00;
const uint8_t eepromChr = 0xFF;
uint16_t adrMax = 0x1000;

Ticker blinkTckr;
const float tckrTime = 0.01;

enum blinkStatus
{
  blinkFast = 0, // wartet auf Password
  blinkSlow,     // wartet auf WiFi
  blinkNo        // mit WiFi verbunden
};
blinkStatus blink;

uint8_t setup_todo;

void get_EEPROM_SIZE(uint16_t SIZE)
{
  adrMax = SIZE;
}
// Liest das Passwort über das Terminalprogramm
String liesEingabe()
{
  boolean newData = false;
  char receivedChars[numChars]; // das Array für die empfangenen Daten
  static byte ndx = 0;
  char endMarker = '\r';
  char rc;

  while (newData == false)
  {
    while (Serial.available() > 0)
    {
      rc = Serial.read();

      if (rc != endMarker)
      {
        receivedChars[ndx] = rc;
        Serial.print(rc);
        ndx++;
        if (ndx >= numChars)
        {
          ndx = numChars - 1;
        }
      }
      else
      {
        receivedChars[ndx] = '\0'; // Beendet den String
        Serial.println();
        ndx = 0;
        newData = true;
      }
    }
  }
  return receivedChars;
}
// Scannt das heimische Netzwerk und zeigt alle möglichen WLAN an
String netzwerkScan()
{
  // Zunächst Station Mode und Trennung von einem AccessPoint, falls dort eine Verbindung bestand
  WiFi.mode(WIFI_STA);
  WiFi.disconnect();
  delay(100);

  Serial.println("Scan-Vorgang gestartet");

  // WiFi.scanNetworks will return the number of networks found
  int n = WiFi.scanNetworks();
  Serial.println("Scan-Vorgang beendet");
  if (n == 0)
  {
    Serial.println("Keine Netzwerke gefunden!");
  }
  else
  {
    Serial.print(n);
    Serial.println(" Netzwerke gefunden");
    for (int i = 0; i < n; ++i)
    {
      // Drucke SSID and RSSI für jedes gefundene Netzwerk
      Serial.print(i + 1);
      Serial.print(": ");
      Serial.print(WiFi.SSID(i));
      Serial.print(" (");
      Serial.print(WiFi.RSSI(i));
      Serial.print(")");
      Serial.println((WiFi.encryptionType(i) == WIFI_AUTH_OPEN) ? " " : "*");
      delay(10);
    }
  }
  uint8_t number;
  do
  {
    Serial.println("Bitte Netzwerk auswaehlen: ");
    String no = liesEingabe();
    number = uint8_t(no[0]) - uint8_t('0');
  } while ((number > n) || (number == 0));

  return WiFi.SSID(number - 1);
}

// der Timer steuert da Blinken der LED
void timer1s()
{
  static uint8_t secs = 0;
  static uint8_t slices = 0;
  slices++;
  switch (blink)
  {
  case blinkFast:
    if (slices >= 10)
    {
      slices = 0;
      secs++;
    }
    break;
  case blinkSlow:
    if (slices >= 40)
    {
      slices = 0;
      secs++;
    }
    break;
  case blinkNo:
    secs = 2;
    break;
  }
  if (secs % 2 == 0)
    // turn the LED on by making the voltage HIGH
    digitalWrite(BUILTIN_LED, HIGH);
  else
    // turn the LED off by making the voltage LOW
    digitalWrite(BUILTIN_LED, LOW);
}
// Bei Bedarf wird der nicht flüchtige Speicher (EEPROM) gelöscht
void eraseEEPROM()
{
  Serial.println("Bitte warten!");
  for (uint16_t adr = 0; adr < adrMax; adr++)
    EEPROM.write(eepromAdr + adr, eepromChr);
  EEPROM.commit();
  // setup in der Anwndung erzwingen
  setup_todo = setup_NOT_done;
  Serial.println("EEPROM erfolgreich geloescht!");
}
// Falls ein Passwort bereits eingegeben wurde, wird versucht damit an das
// zugehörige WLAN zu koppeln. Andernfalls wird aufgefordert, ein WLAN auszusuchen
// und das zugehörige Passwort einzugeben.
// Die kommunikation läuft über den USB-Anschluss bzw. ein verbundenes Terminal-
// programm
void Connect2WiFi()
{
  blinkTckr.attach(tckrTime, timer1s); // each sec
  blink = blinkFast;
  String ssid = "";
  String password = "";
  setup_todo = EEPROM.readByte(adr_setup_done);
  if (setup_todo != setup_done)
  {
    String answer;
    do
    {
      Serial.println("EEPROM loeschen (J/N)?: ");
      answer = liesEingabe();
      answer.toUpperCase();
    } while ((answer != "J") && (answer != "N"));
    if (answer == "J")
      eraseEEPROM();
    else
    {
      answer = "";
      do
      {
        Serial.println("Neue Netzwerkdaten (J/N)?: ");
        answer = liesEingabe();
        answer.toUpperCase();
      } while ((answer != "J") && (answer != "N"));
    }
    if (answer == "J")
    {
      // alles fürs erste Mal
      //
      ssid = netzwerkScan();
      EEPROM.writeString(adr_ssid, ssid);
      EEPROM.commit();
      Serial.println();
      // liest das password ein
      Serial.println("Bitte das Passwort eingeben: ");
      Serial.print("(Falls Sie sich dabei vertippen, muessen Sie den Prozess neu starten!!): ");
      password = liesEingabe();
      EEPROM.writeString(adr_password, password);
      EEPROM.commit();
      Serial.println();
    }
  }
  else
  {
    Serial.println("Zur Neueingabe der Anmeldedaten beliebige Taste druecken!");
    delay(5000);
    if (Serial.available() > 0)
    {
      // zuviele Versuche für diese Runde
      EEPROM.writeByte(adr_setup_done, setup_NOT_done);
      EEPROM.commit();
      ESP.restart();
    }
    blink = blinkSlow;
    ssid = EEPROM.readString(adr_ssid);
    password = EEPROM.readString(adr_password);
  }
  char ssidCh[ssid.length() + 1];
  ssid.toCharArray(ssidCh, ssid.length() + 1);
  char passwordCh[password.length() + 1];
  password.toCharArray(passwordCh, password.length() + 1);

  // Connect to Wi-Fi network with SSID and password
  Serial.print("Verbinde mit dem Netzwerk -");
  Serial.print(ssidCh);
  Serial.println("-");
  //  Serial.print("Mit dem Passwort -");
  //  Serial.print(passwordCh);
  //  Serial.println("-");
  WiFi.begin(ssidCh, passwordCh);
  uint8_t trials = 0;
  blink = blinkSlow;
  while (WiFi.waitForConnectResult() != WL_CONNECTED)
  {
    delay(1000);
    Serial.print(".");
    Serial.println("Versuch: " + String(trials));
    trials++;
    if (trials > 5)
    {
      // zuviele Versuche für diese Runde
      EEPROM.writeByte(adr_setup_done, setup_NOT_done);
      EEPROM.commit();
      ESP.restart();
    }
  }
  // WLAN hat funktioniert
  blink = blinkNo;
  // setup_done setzen
  EEPROM.writeByte(adr_setup_done, setup_done);
  EEPROM.commit();
  // Print local IP address and start web server
  Serial.println();
  Serial.print("Starten Sie jetzt den Browser und geben die IP-Adresse ");
  IPAddress IP = WiFi.localIP();
  Serial.print(IP);
  Serial.println(" ein!");
  Serial.println();
  for (uint8_t ip = 0; ip < 4; ip++)
  {
    EEPROM.writeByte(adr_IP0 + ip, IP[ip]);
    EEPROM.commit();
  }

  blinkTckr.detach();
}
