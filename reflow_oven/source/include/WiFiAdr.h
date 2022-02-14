#include <WiFi.h>
#include <ESPmDNS.h>
#include <WiFiUdp.h>
#include "EEPROM.h"

const uint8_t numChars = 0x20;
// EEPROM-Adressen
#define setup_NOT_done 0x00
#define setup_done 0x47

// EEPROM-Belegung
// EEPROM-Speicherplätze der Local-IDs
#define adrFlag 0x01
const uint16_t adr_setup_done = 0x00;
const uint16_t adr_ssid = adr_setup_done + adrFlag;
const uint16_t adr_password = adr_ssid + numChars;
const uint16_t adr_IP0 = adr_password + numChars;
const uint16_t adr_IP1 = adr_IP0 + adrFlag;
const uint16_t adr_IP2 = adr_IP1 + adrFlag;
const uint16_t adr_IP3 = adr_IP2 + adrFlag;
const uint16_t adr_reset = adr_IP3 + adrFlag;
const uint16_t lastAdr0 = adr_reset + adrFlag;

void Connect2WiFi();
void get_EEPROM_SIZE(uint16_t SIZE);

