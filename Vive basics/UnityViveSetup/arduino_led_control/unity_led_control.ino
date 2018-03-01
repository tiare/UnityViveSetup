//Code based on TableControl in RHI project

#include <SoftwareSerial.h>
#include <SerialCommand.h>

SerialCommand sCmd;

int led = 9;
int timeout = 30;
int brightness = 0;

void setup() {
  // put your setup code here, to run once:
  pinMode(led, OUTPUT);
  Serial.begin(9600);

  while (!Serial);
  
  sCmd.addCommand("PING", pingHandler);
  sCmd.addCommand("ECHO", echoHandler);
  sCmd.addCommand("LED", ledHandler);
}

void loop() {
  // put your main code here, to run repeatedly:
  if(Serial.available() > 0) {
    sCmd.readSerial(); //reads strings from the serial port and invokes the right handler
  }
  // set the brightness of the LED on pin 9:
  analogWrite(led, brightness);
  // wait for 30 milliseconds to see the dimming effect
  delay(timeout); 
}

void ledHandler () {
  char *arg;
  arg = sCmd.next();

  if (arg != NULL) {
    brightness = atoi(arg);
  }
}

void echoHandler () {
  char *arg;
  arg = sCmd.next();

  if (arg != NULL) {
    Serial.println(arg);
  }
  else
    Serial.println("...silence..."); 
}

void pingHandler () {
  Serial.println("PONG");
}


