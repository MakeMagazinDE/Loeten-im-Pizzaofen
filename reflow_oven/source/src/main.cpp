
/* ----------------------------------------------------------------------------
 * "THE BEER-WARE LICENSE" (Revision 42):
 * <gustavwostrack@web.de> wrote this file. As long as you retain this
 * notice you can do whatever you want with this stuff. If we meet some day,
 * and you think this stuff is worth it, you can buy me a beer in return
 * Gustav Wostrack
 * ----------------------------------------------------------------------------
 */

#include <Arduino.h>
// Import required libraries
#include <WiFi.h>
#include "ESPAsyncWebServer.h"
#include "SPIFFS.h"
#include <Wire.h>
#include "EEPROM.h"
#include "MAX31855.h"
#include <Ticker.h>
#include <WiFiAdr.h>

// Entferne den Kommentar, um weitere Infos auszugeben
//#define TESTPRINT

// Erste Version
const double Version = 1.00;

// Der Ofen kann im Reflow-Zustand sein oder nicht
typedef enum REFLOW_STATUS
{
  REFLOW_STATUS_OFF,
  REFLOW_STATUS_ON
} reflowStatus_t;

// Die folgenden Zustände werden in der Temperaturkurve durchlaufen
typedef enum TEMP_STATUS
{
  cold = 0, // Aufwärmen, unterhalb von BasisTemperatur
  preheat,  // von BasisTemperatur bis tempPreheatMax
  soak,     // von tempPreheatMax bis tempSoakMax
  reflow,   // von tempSoakMax bis tempReflowMax
  cooling,  // von tempReflowMax bis tempCoolMin
  complete  // unterhalb von tempCoolMin
} tempStatus_t;

// Diese Infos werden jede Sekunde im Verlauf gespeichert
struct sollTemperatureType
{
  double Temperature;
  TEMP_STATUS prePhase;
  TEMP_STATUS Phase;
  double preTemperature;
  uint8_t PWM;
};

// Anschluss des MAX31855
uint8_t const MAX31855_DO = GPIO_NUM_12;  // (12) MAX31855 data pin (HSPI MISO on ESP32)  - orange
uint8_t const MAX31855_CS = GPIO_NUM_27;  // (27) MAX31855 chip select pin                - red
uint8_t const MAX31855_CLK = GPIO_NUM_14; // (14) MAX31855 clock pin                      - brown
uint8_t const ssrPin = GPIO_NUM_16;
uint8_t const buzzerPin = GPIO_NUM_17;

// Create AsyncWebServer object on port 80
AsyncWebServer server(80);

// Initialize the Thermocouple
MAX31855 thermocouple(MAX31855_CLK, MAX31855_CS, MAX31855_DO);

// Damit der Buzzer Laut gibt
double const freqHi = 2000;
double const freqLo = 0;
uint8_t const channel = 0;
uint8_t const resolution = 8;
uint32_t const dutyCycle = 200;

// Einige  Variablen
uint16_t pwmOutput;
double currentTemperature;
uint16_t lastTemp = 0;
uint8_t preheatPWM, soakPWM, reflowPWM;
uint8_t preTimeFactor;

// Eckwerte der Temperaturkurve
const double BasisTemperatur = 50.0;
// Preheat
const double tempPreheatMax = 150.0;
const uint16_t preheatDuration = 120;
double preheatDelta = (tempPreheatMax - BasisTemperatur) / preheatDuration;
const uint16_t preheatFinishedTime = preheatDuration;
uint8_t overshootTimePreheat = 0;
// Soak
const double tempSoakMax = 183.0;
const uint16_t soakDuration = 180;
double soakDelta = (tempSoakMax - tempPreheatMax) / soakDuration;
const uint16_t soakFinishedTime = preheatFinishedTime + soakDuration;
uint8_t overshootTimeSoak = 0;
// Reflow
const double tempReflowMax = 220.0;
const uint16_t reflowDuration = 80;
const double reflowDelta = (tempReflowMax - tempSoakMax) / reflowDuration;
double tempReflowMin = 183.0;
const uint16_t reflowFinishedTime = soakFinishedTime + reflowDuration;
uint8_t overshootTimeReflow = 0;
// Cooling
const double tempCoolMin = 100.0;
const uint16_t coolingDuration = 90;
const uint16_t coolingFinishedTime = reflowFinishedTime + coolingDuration;
double coolingDelta = (tempCoolMin - tempReflowMax) / coolingDuration;

uint16_t timePreheatMaxPre, timeSoakMaxPre, timeReflowMaxPre;

const uint16_t ultimateSeconds = coolingFinishedTime;
sollTemperatureType sollTemperatur[ultimateSeconds];
uint16_t maxSeconds;
uint8_t curveTemperature[ultimateSeconds];

// EEPROM-Adressen (Non-Volatile Storage)
const uint8_t savedFlag = 0x47;
const uint16_t eepromAdrTemperatureFlag = lastAdr0 + adrFlag;
const uint16_t eepromAdrTemperature = eepromAdrTemperatureFlag + adrFlag;
const uint16_t eepromAdrParameterFlag = eepromAdrTemperature + sizeof(curveTemperature); // savedFlag
const uint16_t eepromAdrParameter0 = eepromAdrParameterFlag + adrFlag;                   // preheatPWM
const uint16_t eepromAdrParameter1 = eepromAdrParameter0 + sizeof(preheatPWM);           // soakPWM
const uint16_t eepromAdrParameter2 = eepromAdrParameter1 + sizeof(soakPWM);              // reflowPWM
const uint16_t eepromAdrParameter3 = eepromAdrParameter2 + sizeof(reflowPWM);            // overshootTimePreheat
const uint16_t eepromAdrParameter4 = eepromAdrParameter3 + sizeof(overshootTimePreheat);     // overshootTimeSoak
const uint16_t eepromAdrParameter5 = eepromAdrParameter4 + sizeof(overshootTimeSoak);        // overshootTimeReflow
const uint16_t eepromAdrParameter6 = eepromAdrParameter5 + sizeof(overshootTimeReflow);      // preTimeFactor
const uint16_t eepromAdrPasswordFlag = eepromAdrParameter6 + sizeof(preTimeFactor);
const uint16_t eepromAdrPassword = eepromAdrPasswordFlag + adrFlag;
const uint16_t eepromPWD = 0xFF;
const uint16_t EEPROM_SIZE = eepromAdrPassword + eepromPWD;

// Weitere Variablen
uint16_t secondsSOLL;
uint16_t secondsIST;
unsigned long windowStartTime;
tempStatus_t reflowState;    // Reflow oven controller state machine state variable
reflowStatus_t reflowStatus; // Reflow oven controller status
Ticker SSRtckr;
Ticker TSTtckr;
const uint32_t SSRtckrTime = 1;
const uint32_t TSTtckrTime = 1000;
const uint16_t minOutput = 0;
const uint16_t maxOutput = 100;

/////////////////// HELPER ///////////////////////
// Gibt Laut
void makeBeep()
{
  ledcWriteTone(channel, freqHi);
  delay(100);
  ledcWriteTone(channel, freqLo);
}
// Liest 5mal die Temperatur ein und ermittelt deren Durchschnitt.
// Dadurch werden Ausreißer vermieden.
double readTemperature()
{
  static double temp = 20.0;
  const uint8_t singleTemps = 5;
  static double temps[singleTemps] = {16.0f, 17.0f, 18.0f, 19.0f, 20.0f}; // Array for moving average filter

  thermocouple.read();

  if (thermocouple.getStatus())
  {
    Serial.print("FEHLER:\t\t");
    if (thermocouple.shortToGND())
      Serial.println("SHORT TO GROUND");
    if (thermocouple.shortToVCC())
      Serial.println("SHORT TO VCC");
    if (thermocouple.openCircuit())
      Serial.println("OPEN CIRCUIT");
    if (thermocouple.genericError())
      Serial.println("GENERIC ERROR");
    if (thermocouple.noRead())
      Serial.println("NO READ");
    if (thermocouple.noCommunication())
      Serial.println("NO COMMUNICATION");
    return 0.0;
  }

  temps[0] = thermocouple.getTemperature();

  temp = (temps[0] + temps[1] + temps[2] + temps[3] + temps[4]) / 5.0f; // Compute average of last 5 readings
  temps[4] = temps[3];                                                  // Update moving average filter
  temps[3] = temps[2];
  temps[2] = temps[1];
  temps[1] = temps[0];
  return temp;
}
// Schaltet das Solid State Relay ein
void ovenON()
{
  digitalWrite(ssrPin, HIGH);
}
// Schaltet das Solid State Relay (SSR) aus
void ovenOFF()
{
  digitalWrite(ssrPin, LOW);
}
// Setzt die Frequenz der Pulsweitenmodulation,
// mit der das SSR beaufschlagt wird.
void setPWM(uint16_t pwm)
{
  pwmOutput = pwm;
}
// Liest die Frequenz der PWM aus
uint16_t getPWM()
{
  return pwmOutput;
}
// Schaltet den Ofen entweder vollständig ein (MaxOutput)
// oder im Rythmus der PWM
// Diese Prozedur wird jede Millisekunden vom Timer SSRtckr aufgerufen
// Wurde PWM beispielsweise auf 60 gesetzt, dann ist der Ofen
// 60 Millisekunden ein- und 40 Millisekunden ausgeschaltet.
// Darüber hinaus wird die LED im Sekundenrythmus ein- und ausgeschaltet. 
void setSSR()
{
  static bool onOff = true;
  static uint16_t blinkCntr = 0;
  if (reflowStatus == REFLOW_STATUS_ON)
  {
    if (getPWM() == maxOutput)
    {
      ovenON();
    }
    else
    {
      blinkCntr++;
      if (blinkCntr >= (1000 / SSRtckrTime))
      {
        blinkCntr = 0;
        onOff = !onOff;
      }
      digitalWrite(BUILTIN_LED, onOff);
      static uint16_t elapsedTime = 0;
      elapsedTime++;
      if (getPWM() >= elapsedTime)
        ovenON();
      else
        ovenOFF();
      if (elapsedTime >= maxOutput)
        elapsedTime = 0;
    }
  }
  else
    ovenOFF();
}
// Weiterer Ofeneinschalter
void switchOvenOFF()
{
  reflowStatus = REFLOW_STATUS_OFF;
  ovenOFF();
  setPWM(minOutput);
}
// Weiterer Ofenausschalter
void switchOvenON()
{
  reflowStatus = REFLOW_STATUS_ON;
  ovenON();
  setPWM(maxOutput);
}
// Diese Prozedur durchläuft einen vollständigen Temperaturzyklus.
// Immer wenn die Temperatur in eine neue Phase eintritt,
// wird der Ofen ausgeschaltet und die Zeit gemessen,
// bis die Temperatur nicht weiter steigt. Damit werden die Überschwinger
// ermittelt. Darüber hinaus wird berechnet, welche Zeit benötigt wird, um eine
// Phase zu durchlaufen. Diese Daten bilden dann die Ausgangswerte
// für den Reflowprozess.
void ofenTESTProc()
{
  double currentTemp = 0;
  static double lastTemperature = 0;
  static double lastBeforeTemperature = 0;
  const uint8_t finishPhase = 11;
  static uint8_t phase = 0;
  static uint16_t runningTime = 0;
  static uint16_t inTime;
  static uint16_t overshootInTime = 0;;
  static double deltaPreheat = 0;
  static double deltaSoak = 0;
  static double deltaReflow = 0;
  /*
  phase0: unterhalb roomtemp
  phase1: zwischen roomtemp und preheatmax
  phase2: overshoot preheat
  phase3: warten auf preheatmax
  phase4: zwischen  preheatmax und soakmax
  phase5: overshoot soak
  phase6: warten auf soak
  phase7: zwischen  soakmax und reflowkmax
  phase8: overshoot reflow
  phase9: finish

  const double BasisTemperatur = 30.0;
  const double tempPreheatMax = 150.0;
  const double tempSoakMax = 183.0;
  const double tempReflowMax = 230.0;
  double tempReflowMin = 183.0;
  const double tempCoolMin = 100.0;
  */
  currentTemp = readTemperature();
  Serial.println("Temperatur: " + String(currentTemp));
  switch (phase)
  {
  case 0:
    // schaltet Ofen ein und
    // heizt auf bis BasisTemperatur erreicht ist
    // dann nächste Phase
    if (runningTime == 0)
    {
      // Ofen einschalten
      reflowStatus = REFLOW_STATUS_ON;
      ovenON();
      setPWM(maxOutput);
    }
    if (currentTemp >= BasisTemperatur)
    {
      phase = 1;
      inTime = runningTime;
    }
    break;
  case 1:
    // heizt auf bis tempPreheatMax erreicht ist
    // schaltet dann Ofen aus und nächste Phase
    if (currentTemp >= tempPreheatMax)
    {
      // Ofen ausschalten
      switchOvenOFF();
      deltaPreheat = (tempPreheatMax - BasisTemperatur) / (runningTime - inTime);
      overshootInTime = runningTime;
      phase = 2;
    }
    break;
  case 2:
    // wartet bis der overshoot sinkt
    if ((currentTemp < lastTemperature) && (lastTemperature < lastBeforeTemperature))
    {
      overshootTimePreheat = runningTime - overshootInTime;
      phase = 3;
    }
    break;
  case 3:
    // wartet bis tempPreheatMax wieder erreicht ist
    // und schaltet den Ofen wieder ein, nächste Phase
    if (currentTemp <= tempPreheatMax)
    {
      // Ofen einschalten
      switchOvenON();
      phase = 4;
    }
    break;
  case 4:
    // hier beginnt die soak-Phase
    if (currentTemp >= tempPreheatMax)
    {
      inTime = runningTime;
      phase = 5;
    }
    break;
  case 5:
    // heizt auf bis tempSoakMax erreicht ist
    // schaltet dann Ofen aus und nächste Phase
    if (currentTemp >= tempSoakMax)
    {
      // Ofen ausschalten
      switchOvenOFF();
      deltaSoak = (tempSoakMax - tempPreheatMax) / (runningTime - inTime);
      lastTemperature = currentTemp;
      overshootInTime = runningTime;
      phase = 6;
    }
    break;
  case 6:
    // wartet bis der overshoot sinkt
    if ((currentTemp < lastTemperature) && (lastTemperature < lastBeforeTemperature))
    {
      overshootTimeSoak = runningTime - overshootInTime;
      phase = 7;
    }
    break;
  case 7:
    // wartet bis tempSoakMax wieder erreicht ist
    // und schaltet den Ofen wieder ein, nächste Phase
    if (currentTemp <= tempSoakMax)
    {
      // Ofen einschalten
      switchOvenON();
      phase = 8;
    }
    break;
  case 8:
    // hier beinnt die reflow-Phase
    if (currentTemp >= tempSoakMax)
    {
      inTime = runningTime;
      phase = 9;
    }
    break;
  case 9:
    // heizt auf bis tempReflowMax erreicht ist
    // schaltet dann Ofen aus und nächste Phase
    if (currentTemp >= tempReflowMax)
    {
      // Ofen ausschalten
      switchOvenOFF();
      deltaReflow = (tempReflowMax - tempSoakMax) / (runningTime - inTime);
      lastTemperature = currentTemp;
      overshootInTime = runningTime;
      phase = 10;
    }
    break;
  case 10:
    // wartet bis der overshoot sinkt
    if ((currentTemp < lastTemperature) && (lastTemperature < lastBeforeTemperature))
    {
      overshootTimeReflow = runningTime - overshootInTime;
      phase = 11;
    }
    break;
  case finishPhase:
    // fertig
    // Erfebnisse speichern
    TSTtckr.detach(); // each sec

    // speichern der Ergebnisse
    // preheat
    preheatPWM = preheatDelta / deltaPreheat * maxOutput;
    EEPROM.writeByte(eepromAdrParameter0, preheatPWM);
    EEPROM.commit();
    EEPROM.writeByte(eepromAdrParameter3, overshootTimePreheat);
    EEPROM.commit();
#ifdef TESTPRINT
    Serial.println("overshootTimePreheat: " + String(overshootTimePreheat));
    Serial.println("preheatDelta: " + String(preheatDelta) + " - deltaPreheat: " + String(deltaPreheat) + " - " + String(preheatPWM));
#endif
    // soak
    soakPWM = soakDelta / deltaSoak * maxOutput;
    EEPROM.writeByte(eepromAdrParameter1, soakPWM);
    EEPROM.commit();
    EEPROM.writeByte(eepromAdrParameter4, overshootTimeSoak);
    EEPROM.commit();
#ifdef TESTPRINT
    Serial.println("overshootTimeSoak: " + String(overshootTimeSoak));
    Serial.println("soakDelta: " + String(soakDelta) + " - deltaSoak: " + String(deltaSoak) + " - " + String(soakPWM));
#endif
    // reflow
    reflowPWM = reflowDelta / deltaReflow * maxOutput;
    EEPROM.writeByte(eepromAdrParameter2, reflowPWM);
    EEPROM.commit();
    EEPROM.writeByte(eepromAdrParameter5, overshootTimeReflow);
    EEPROM.commit();
#ifdef TESTPRINT
    Serial.println("overshootTimeReflow: " + String(overshootTimeReflow));
    Serial.println("reflowDelta: " + String(reflowDelta) + " - deltaReflow: " + String(deltaReflow) + " - " + String(reflowPWM));
#endif
    EEPROM.writeByte(eepromAdrParameterFlag, savedFlag);
    EEPROM.commit();

    break;
  default:
    break;
  }
  runningTime++;
  lastBeforeTemperature = lastTemperature;
  lastTemperature = currentTemp;
  Serial.println("TEST laeuft in Phase: " + String(phase) + " nach Sekunden: " + String(runningTime));
  return;
}
// Diese Prozedur wird vom Browser gestartet. Sie initialisiert den Timer TSTtckr,
// der wiederum die Prozedur ofenTESTProc jede Sekunde (alle 1000 Millisekunden)
// aufruft.
String ofenTEST()
{
  TSTtckr.attach_ms(TSTtckrTime, ofenTESTProc); // each sec
  return "1";
}
// Die Prozedur getNextSollTemp ist eine Hilfsprozdur für die
// eigentliche Festlegung der Solltemperaturkurve in der Prozedur defineSOLLTemp.
// Sie errechnet aus der momentanen Temperatur priorTemp und der Temperatursteigungsrate
// die Rückgabetemperatur bis die Maxomaltemperatur max der jeweiligen Phase erreicht ist. 
double getNextSollTemp(double priorTemp, double max, double delta)
{
  double nextTemp = priorTemp;
  if (delta > 0)
  {
    if (priorTemp < max)
      nextTemp = priorTemp + delta;
  }
  else
  {
    if (priorTemp > max)
      nextTemp = priorTemp + delta;
  }
  return nextTemp;
}
// In dieser Prozedur wird die Solltemperatur für eine bestimmte Sekunde
// festgelegt und in das Array sollTemperatur geschrieben.
void defineSOLLTemp(uint16_t second)
{
  static double temperature;
  static double nextTemp = BasisTemperatur;

  temperature = nextTemp;
  switch (reflowState) // Reflow oven controller state machine
  {

  case preheat:                        // If state = PREHEAT
    if (temperature >= tempPreheatMax) // If minimum soak temperature is achieved
    {
      reflowState = soak; // Proceed to soaking state
    }
    else
      nextTemp = getNextSollTemp(nextTemp, tempPreheatMax, preheatDelta);
    break;

  case soak:                        // If state = SOAK
    if (temperature >= tempSoakMax) // If maximum soak temperature is achieved
    {
      reflowState = reflow; // Proceed to reflowing state
    }
    else
      nextTemp = getNextSollTemp(nextTemp, tempSoakMax, soakDelta);
    break;

  case reflow:                        // If state = REFLOW HEAT
    if (temperature >= tempReflowMax) // If maximum reflow temperature is achieved
    {
      reflowState = cooling; // Proceed to reflowing level state
    }
    else
      nextTemp = getNextSollTemp(nextTemp, tempReflowMax, reflowDelta);
    break;

  case cooling:                     // If state = REFLOW COOL
    if (temperature <= tempCoolMin) // If minimummum reflow temperature is achieved
    {
      nextTemp = BasisTemperatur - 1; // 
      reflowState = complete;          // Proceed to cool down state
    }
    else
      nextTemp = getNextSollTemp(nextTemp, tempCoolMin, coolingDelta);
    break;
  case cold:
  case complete: // If state = REFLOW COOL DOWN
    break;
  }
  sollTemperatur[second].Temperature = nextTemp;
  sollTemperatur[second].prePhase = reflowState;
  sollTemperatur[second].Phase = reflowState;
  sollTemperatur[second].preTemperature = nextTemp;
  switch (reflowState)
  {
  case preheat:
    sollTemperatur[second].PWM = preheatPWM;
    break;
  case soak:
    sollTemperatur[second].PWM = soakPWM;
    break;
  case reflow:
    sollTemperatur[second].PWM = reflowPWM;
    break;
  case cold:
  case cooling:
  case complete:
    sollTemperatur[second].PWM = minOutput;
    break;
  }
}
// Um dem Überschwingen zu begegnen, werden die Paramater der nächsten Phase
// etwas früher angewandet. Mit Hilfe der Daten aus der Prozedur Ofentest werden
// die Zeiten hier berechnet.
void calcPreValues()
{
  // Zeiten, vor denen denen bereits die PWM-Werte der nächsten Phase eingestellt werden
  timePreheatMaxPre = (overshootTimePreheat * preTimeFactor) / maxOutput;
  timeSoakMaxPre = (overshootTimeSoak * preTimeFactor) / maxOutput;
  timeReflowMaxPre = (overshootTimeReflow * preTimeFactor) / maxOutput / 4;
#ifdef TESTPRINT
  Serial.println("timePreheatMaxPre: "+String(timePreheatMaxPre));
  Serial.println("timeSoakMaxPre: "+String(timeSoakMaxPre));
  Serial.println("timeReflowMaxPre: "+String(timeReflowMaxPre));
  Serial.println("preheatPWM: "+String(preheatPWM));
  Serial.println("soakPWM: "+String(soakPWM));
  Serial.println("reflowPWM: "+String(reflowPWM));
  Serial.println("preTimeFactor: "+String(preTimeFactor));
#endif
}
// Die durch calcPreValues veränderten Werte werden in das sollTemperatur-Array
// eingearbeitet
void rebuildPreTempArray()
{
  calcPreValues();
  // soak zum Ende von preheat vorziehen
  for (uint16_t p = 0; p < timePreheatMaxPre; p++)
  {
    sollTemperatur[preheatFinishedTime - p].prePhase = sollTemperatur[preheatFinishedTime + timePreheatMaxPre - p].prePhase;
    sollTemperatur[preheatFinishedTime - p].PWM = sollTemperatur[preheatFinishedTime + timePreheatMaxPre - p].PWM;
  }
  // reflow zum Ende von soak vorziehen
  for (uint16_t s = 0; s < timeSoakMaxPre; s++)
  {
    sollTemperatur[soakFinishedTime - s].prePhase = sollTemperatur[soakFinishedTime + timeSoakMaxPre - s].prePhase;
    sollTemperatur[soakFinishedTime - s].PWM = sollTemperatur[soakFinishedTime + timeSoakMaxPre - s].PWM;
  }
  // cooling zum Ende von reflow vorziehen
  for (uint16_t r = 0; r < timeReflowMaxPre; r++)
  {
    // reflowFinishedTime = 380 - timeReflowMaxPre = 9
  //  sollTemperatur[reflowFinishedTime - r].preTemperature = sollTemperatur[reflowFinishedTime + timeReflowMaxPre - r].preTemperature;
    sollTemperatur[reflowFinishedTime - r].prePhase = sollTemperatur[reflowFinishedTime + timeReflowMaxPre - r].prePhase;
    sollTemperatur[reflowFinishedTime - r].PWM = sollTemperatur[reflowFinishedTime + timeReflowMaxPre - r].PWM;
  }
}
// Während die Prozedur defineSOLLTemp die Solltemperatur für eine bestimmte
// Sekunde festlegt, wird hier der gesamte Temperaturverlauf abgearbeitet.
void setDataSOLL()
{
  reflowState = preheat;
  for (maxSeconds = 0; maxSeconds < ultimateSeconds; maxSeconds++)
  {
    defineSOLLTemp(maxSeconds); // mindestens 1 mal in der Sekunde
    if (sollTemperatur[maxSeconds].Temperature < BasisTemperatur)
    {
      digitalWrite(BUILTIN_LED, HIGH);
      break;
    }
  }
  rebuildPreTempArray();
#ifdef TESTPRINT
  for (uint16_t sec = 0; sec < maxSeconds; sec++)
  {
    Serial.print(String(sec));
    Serial.print(" - Temp: " + String(sollTemperatur[sec].Temperature));
    Serial.print(" - prePhase: " + String(sollTemperatur[sec].prePhase));
    Serial.print(" - preTemperature: " + String(sollTemperatur[sec].preTemperature));
    Serial.println(" - PWM: " + String(sollTemperatur[sec].PWM));
  }
#endif
}
// Diese Funktion wird von der Browseranwendung direkt zu Beginn aufgeruen und
// gibt zurück, die Gesamtzeit eines Temperaturverlaufes, sowie die PWM-Werte der
// einzelnen Phasen.
String maxTime()
{
  return String(maxSeconds) + "/" + String(preheatPWM) + "/" + String(soakPWM) + "/" + String(reflowPWM) + "/" + String(preTimeFactor);
}
// Nach einem Reflowprozess wird der tatsächliche Verlauf gespeichert
// und kann später wieder abgerufen werden.
void saveTemperature()
{
  if (lastTemp == 0)
    return;
  curveTemperature[lastTemp] = 0x00;
  // Flag setzen, dass eine Temperaturkurve gespeichert wurde
  EEPROM.writeByte(eepromAdrTemperatureFlag, savedFlag);
  EEPROM.commit();
  // Temperatur sichern
  for (uint16_t x = 0; x < lastTemp + 1; x++)
  {
    EEPROM.writeByte(eepromAdrTemperature + x, curveTemperature[x]);
    Serial.println(curveTemperature[x]);
    EEPROM.commit();
  }
  lastTemp = 0;
}
// Um den tatsächlichen Temperaturverlauf möglichst nahe an die Sollwerte zu
// bringen, werden mehrere Vorkehrungen getroffen, die bereits erwähnt wurden.
// Diese Funktion versucht nun, den Fehler einer Abweichung nach oben oder
// unten durch Anpassung des aktuellen PWM-Wertes zu minimieren. Die Funktion
// gibt den aktuellen Fehlerwert zurück. 
double correctTemperature(uint16_t currtime)
{
  int16_t pwm_tmp = sollTemperatur[currtime].PWM;
  static double lastError = 0;
  static double lastBeforeError = 0;
  double error = sollTemperatur[currtime].preTemperature - currentTemperature;
  const double minOutputFactor = 10.0;
  if (abs(error) <= 1.0)
    return error;
  if (reflowStatus == REFLOW_STATUS_OFF)
    setPWM(minOutput);
  // Unterscheidung der 4 Fälle
  if (error > 0)
  {
    // Temperatur zu tief (error ist positiv) --> PWM erhöhen
    // Fall 1: Temperatur unter Soll und fällt (error wird größer)
    if ((error > lastError) && (lastError > lastBeforeError))
    {
#ifdef TESTPRINT
      Serial.println("Fall 1: Temperatur unter Soll und faellt (Fehler wird groesser)");
#endif
      pwm_tmp = sollTemperatur[currtime].PWM + error * minOutputFactor;
      if (pwm_tmp > maxOutput)
        pwm_tmp = maxOutput;
    }
    else
      // Fall 2: Temperatur unter Soll und steigt (error wird kleiner)
      if ((error < lastError) && (lastError < lastBeforeError))
      {
#ifdef TESTPRINT
        Serial.println("Fall 2: Temperatur unter Soll und steigt (Fehler wird kleiner)");
#endif
      }
  }
  else
  {
    // Temperatur zu hoch (error ist negativ) --> PWM verkleinern
    // Fall 3: Temperatur über Soll und steigt (error wird größer)
    if ((error < lastError) && (lastError < lastBeforeError))
    {
      pwm_tmp = sollTemperatur[currtime].PWM + error * minOutputFactor;
      if (pwm_tmp < minOutput)
        pwm_tmp = minOutput;
#ifdef TESTPRINT
      Serial.println("Fall 3: Temperatur ueber Soll und steigt (Fehler wird groesser)");
#endif
    }
    else
      // Fall 4: Temperatur über Soll und fällt (error wird kleiner)
      if ((error > lastError) && (lastError > lastBeforeError))
      {
  #ifdef TESTPRINT
        Serial.println("Fall 4: Temperatur ueber Soll und faellt (Fehler wird kleiner)");
        #endif
      }
  }
  setPWM(pwm_tmp);
  lastBeforeError = lastError;
  lastError = error;
  return error;
}
// Dies ist Steuerprozedur für den Reflow-Prozess. Nachdem die BasisTemperatur
// erreicht ist, werden die einzelnen Phasen des Reflowprozesses durchlaufen. Dabei
// werden die Daten, die bisher im Rahmen der Solltemperaturbestimmung festgelegt
// wurden, als Basis genommen.
String reflowProcess()
{
  static uint16_t xt = 0;
  static tempStatus_t indicator = cold;

  currentTemperature = readTemperature();

  // falls JAVASCRIPT zu viel abfragt
  if (indicator >= complete)
    return String(xt) + "/" + String(currentTemperature) + "/" + String(indicator);
  switch (indicator)
  {
  case cold: // Aufwärmen, unterhalb von BasisTemperatur
             // die Parameter werden in OnRequest festgelegt
    if (currentTemperature >= BasisTemperatur - 15)
    {
      setPWM(preheatPWM);
      if (currentTemperature >= BasisTemperatur)
      {
        indicator = preheat; // von BasisTemperatur bis tempPreheatMax
        makeBeep();
      }
    }
    break;
  case preheat: // von BasisTemperatur bis tempPreheatMax
    if (sollTemperatur[xt].prePhase == soak)
    {
      indicator = soak; // von tempPreheatMax bis tempSoakMax
      makeBeep();
      Serial.println("---> Changed to soak");
    }
    break;
  case soak: // von tempPreheatMax bis tempSoakMax
    if (sollTemperatur[xt].prePhase == reflow)
    {
      indicator = reflow; // von tempSoakMax bis tempReflowMax
      makeBeep();
      Serial.println("---> Changed to reflow");
    }
    break;
  case reflow: // von tempSoakMax bis tempReflowMax
    if (sollTemperatur[xt].prePhase == cooling)
    {
      indicator = cooling; // von tempSoakMax bis tempReflowMax
      // Ofen ausschalten
      switchOvenOFF();
      makeBeep();
      Serial.println("---> Changed to cooling");
    }
    break;
  case cooling: // von tempReflowMax bis tempCoolMin
    if (currentTemperature <= tempCoolMin)
    {
      indicator = complete; // unterhalb von tempCoolMin
      lastTemp = xt;
      makeBeep();
      Serial.println("---> Changed to complete");
    }
    break;
  case complete: // unterhalb von tempCoolMin
    break;
  }
  if ((indicator > cold) && (indicator != complete))
  {
    Serial.println(String(xt) + ": SOLL: " + String(sollTemperatur[xt].preTemperature) + " IST: " + String(currentTemperature) + " FEHLER: " + String(correctTemperature(xt)) + " PWM: " + String(getPWM()));
    curveTemperature[xt] = (uint16_t)currentTemperature;
    xt++;
  }
  else
    Serial.println(String(xt) + ": SOLL: " + String(sollTemperatur[xt].Temperature) + " IST: " + String(currentTemperature) + " FEHLER: " + String(sollTemperatur[xt].Temperature - currentTemperature) + " PWM: " + String(getPWM()));
  String answerREFLOW = String(xt) + "/" + String(currentTemperature) + "/" + String(sollTemperatur[xt].Phase);
  return answerREFLOW;
}
// Diese Funktion wird von der Browseranwendung aufgerufen,
// stoppt den Reflowprozess und schaltet den Ofen aus.
String stopProcess()
{
  // Ofen ausschalten
  switchOvenOFF();
  return "1";
}
// Diese Funktion wird von der Browseranwendung aufgerufen und stellt
// einen gespeicherten Temperaturverlauf aus einem ehemaligen Reflowprozess dar.
String showCurve()
{
  static bool firstTime = true;
  static uint16_t xt = 0;
  static tempStatus_t indicator = preheat;
  if (firstTime == true)
  {
    firstTime = false;
    uint16_t x = 0;
    // Daten/alte Kurve vorhanden?
    bool dataAvail = EEPROM.readByte(eepromAdrTemperatureFlag) == savedFlag;
    if (dataAvail)
    {
      // dann lies ein
      curveTemperature[x] = EEPROM.readByte(eepromAdrTemperature + x);
      while (curveTemperature[x] != 0x00)
      {
        x++;
        curveTemperature[x] = EEPROM.readByte(eepromAdrTemperature + x);
      }
    }
    else
    {
      // ansonsten melde das Endezeichen (Temperatur = 0)
      curveTemperature[x] = 0x00;
    }
  }
  uint8_t currTemp = curveTemperature[xt];
  if (currTemp == 0x00)
    indicator = complete;
  String answerCURVE = String(xt) + "/" + String(currTemp) + "/" + String(indicator);
  Serial.println(answerCURVE);
  xt++;
  return answerCURVE;
}
// Diese Funktion wird von der Browseranwendung aufgerufen und schreibt die
// Solltemperatur in den Browser
String liesSOLLTemperatur()
{
  // Read temperature as Celsius (the default)
  double tempSoll = sollTemperatur[secondsSOLL].Temperature;
  String answerSOLL = String(secondsSOLL) + "/" + String(tempSoll);
  secondsSOLL++;
  if (secondsSOLL >= maxSeconds)
    secondsSOLL = 0;
  return answerSOLL;
}
// Diese Funktion wird während des Leerlaufes sekündlich von der Browseranwendung
// aufgerufen und gibt die aktuelle Sensortemperatur zurück
String liesTemperatur()
{
  String answer = String(readTemperature());
  Serial.println("IST-Temperatur: " + answer);
  return answer;
}
//////////////////////////////////////////

// Teilt einen String mit Hilfe des Separators in seine Bestandteile auf
// und gibt den index-Teil zurück
String getStringPartByNr(String data, char separator, int index)
{
  int stringData = 0;   // variable to count data part nr
  String dataPart = ""; // variable to hole the return text

  for (int i = 0; i < data.length() - 1; i++)
  { // Walk through the text one letter at a time
    if (data[i] == separator)
    {
      // Count the number of times separator character appears in the text
      stringData++;
    }
    else if (stringData == index)
    {
      // get the text when separator is the rignt one
      dataPart.concat(data[i]);
    }
    else if (stringData > index)
    {
      // return text and stop if the next separator appears - to save CPU-time
      return dataPart;
      break;
    }
  }
  // return text if this is the last part
  return dataPart;
}
// Mit dieser Prozedur wird zu Beginn eines Reflowprozesses von der 
// Browseranwendung initiiert. Damit werden die PWM-Werte, die evtl.
// im Browser verändert wurden übergeben, gespeichert und dann im
// Temperaturverlauf berücksichtigt.
void onRequest(AsyncWebServerRequest *request)
{
  String urlLine = request->url();
  String val;
  if (urlLine.indexOf("/PARAM") >= 0)
  {
    Serial.println("Message: " + String(urlLine));
    for (uint8_t p = 0; p < 4; p++)
    {
      // beginne mit dem 2. Parameter (ohne /PARAM)
      val = getStringPartByNr(urlLine, '/', p + 2);
      switch (p)
      {
      case 0:
        preheatPWM = (uint8_t)val.toInt();
        EEPROM.writeByte(eepromAdrParameter0, preheatPWM);
        break;
      case 1:
        soakPWM = (uint8_t)val.toInt();
        EEPROM.writeByte(eepromAdrParameter1, soakPWM);
        break;
      case 2:
        reflowPWM = (uint8_t)val.toInt();
        EEPROM.writeByte(eepromAdrParameter2, reflowPWM);
        break;
      case 3:
        preTimeFactor = (uint8_t)val.toInt();
        EEPROM.writeByte(eepromAdrParameter6, preTimeFactor);
        break;
      }
      EEPROM.commit();
    }
    EEPROM.writeByte(eepromAdrParameterFlag, savedFlag);
    EEPROM.commit();
    Serial.println("1: " + String(preheatPWM) + " 2: " + String(soakPWM) + " 3: " + String(reflowPWM) + " 4: " + String(preTimeFactor));
    rebuildPreTempArray();
    switchOvenON();
  }
}
//////////////////////////////////////////
// Mit setup werden alle Anfangswerte gesetzt
void setup()
{
  Serial.begin(115200);
  
  // Meldung auf dem PC, falls ein Terminalprogramm lauscht. 
  Serial.println();
  Serial.print("R e f l o w - O f e n (Version: " + String(Version) + ")");
  Serial.println();
  Serial.println();

  // Initialisierung des MAX31855 bzw. des Temperatursensors
  thermocouple.begin();

  // Initialisierung von SPIFFS
  if (!SPIFFS.begin())
  {
    Serial.println("Fehler beim Start von SPIFFS");
    return;
  }

  // die EEPROM-Library wird gestartet
  if (!EEPROM.begin(EEPROM_SIZE))
  {
    Serial.println("Failed to initialise EEPROM");
  }
  get_EEPROM_SIZE(EEPROM_SIZE);

  // Die LED muss an einen Ausgang
  pinMode(LED_BUILTIN, OUTPUT);
  // Auch das SSR muss an einen Ausgang
  pinMode(ssrPin, OUTPUT);
  // Die Werte für den Buzzer werden gesetzt
  ledcSetup(channel, freqHi, resolution);
  ledcAttachPin(buzzerPin, channel);
  ledcWrite(channel, dutyCycle);
  // Die Anwendung meldet sich mit einem Beep
  makeBeep();

  // Die Anmeldeprozedur für den ersten Start an das WiFi
  Connect2WiFi();

  // Variable werden gesetzt
  secondsSOLL = 0;
  secondsIST = 0;
  lastTemp = 0;
  uint8_t oldParas = EEPROM.readByte(eepromAdrParameterFlag);
  // Voreinstellung laden
  preheatPWM = 85;
  overshootTimePreheat = 35;
  soakPWM = 60;
  overshootTimeSoak = 30;
  reflowPWM = maxOutput;
  overshootTimeReflow = 35;
  const uint8_t maxpreTemp = maxOutput;
  preTimeFactor = 35;
  if (oldParas == savedFlag)
  {
    uint16_t tmp = 0;
    // bereits gespeicherte Werte laden
    tmp = EEPROM.readByte(eepromAdrParameter0);
    if (tmp <= maxOutput)
      preheatPWM = tmp;
    tmp = EEPROM.readByte(eepromAdrParameter1);
    if (tmp <= maxOutput)
      soakPWM = tmp;
    tmp = EEPROM.readByte(eepromAdrParameter2);
    if (tmp <= maxOutput)
      reflowPWM = tmp;
    tmp = EEPROM.readByte(eepromAdrParameter3);
    if (tmp <= maxpreTemp)
      overshootTimePreheat = tmp;
    tmp = EEPROM.readByte(eepromAdrParameter4);
    if (tmp <= maxpreTemp)
      overshootTimeSoak = tmp;
    tmp = EEPROM.readByte(eepromAdrParameter5);
    if (tmp <= maxpreTemp)
      overshootTimeReflow = tmp;
    tmp = EEPROM.readByte(eepromAdrParameter6);
    if (tmp <= 2*maxOutput)
      preTimeFactor = tmp;
  }
  // Solltemperatur wird ermittelt
  setDataSOLL();

  // Die Einstiegsroutinen für den Webserver
  // Beispiel: Kommt vom Browser der String "/SOLL", wird die Funktion
  // liesSOLLTemperatur() gestartet.
  server.on("/", HTTP_GET, [](AsyncWebServerRequest *request)
            { request->send(SPIFFS, "/index.html"); });
  server.on("/SOLL", HTTP_GET, [](AsyncWebServerRequest *request)
            { request->send_P(200, "text/plain", liesSOLLTemperatur().c_str()); });
  server.on("/TEMP", HTTP_GET, [](AsyncWebServerRequest *request)
            { request->send_P(200, "text/plain", liesTemperatur().c_str()); });
  server.on("/MAXPHASE", HTTP_GET, [](AsyncWebServerRequest *request)
            { request->send_P(200, "text/plain", maxTime().c_str()); });
  server.on("/CURVE", HTTP_GET, [](AsyncWebServerRequest *request)
            { request->send_P(200, "text/plain", showCurve().c_str()); });
  server.on("/REFLOW", HTTP_GET, [](AsyncWebServerRequest *request)
            { request->send_P(200, "text/plain", reflowProcess().c_str()); });
  server.on("/STOP", HTTP_GET, [](AsyncWebServerRequest *request)
            { request->send_P(200, "text/plain", stopProcess().c_str()); });
  server.on("/OFEN", HTTP_GET, [](AsyncWebServerRequest *request)
            { request->send_P(200, "text/plain", ofenTEST().c_str()); });

  // Start server
  server.onNotFound(onRequest);
  server.begin();

  // Aktuelle Zeit
  windowStartTime = millis();
  // Start des Timers für das SSR
  SSRtckr.attach_ms(SSRtckrTime, setSSR); // each sec
  // Vorsichtshalber schalten wir den Ofen zunächst mal aus
  switchOvenOFF();
}

void loop()
{
  saveTemperature();
}
