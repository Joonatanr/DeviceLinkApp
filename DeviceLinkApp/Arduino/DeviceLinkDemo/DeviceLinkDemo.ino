#include <Servo.h>

#define SERVO_PIN 13
#define SWITCH_PIN 8
#define LED1_PIN 12
#define LED2_PIN 11
#define POT_PIN A1

const unsigned int MAX_MESSAGE_LENGTH = 128;

Servo myServo;  // create servo object to control a servo
// twelve servo objects can be created on most boards

int pos = 0;    // variable to store the servo position

void setup() 
{
  myServo.attach(SERVO_PIN);  // attaches the servo on pin 9 to the servo object
  Serial.begin(115200); // opens serial port, sets data rate to 9600 bps

  //Set up GPIO
  pinMode(SWITCH_PIN, INPUT);
  pinMode(LED1_PIN, OUTPUT);
  pinMode(LED2_PIN, OUTPUT);
}

void loop() 
{
  int val;
  
  if (Serial.available() > 0)
  {
      //Create a place to hold the incoming message
      static char message[MAX_MESSAGE_LENGTH];
      static unsigned int message_pos = 0;

      //Read the next available byte in the serial receive buffer
      char inByte = Serial.read();

      //Message coming in (check not terminating character) and guard for over message size
      if ( inByte != '\n' && (message_pos < MAX_MESSAGE_LENGTH - 1) )
      {
        //Add the incoming byte to our message
        message[message_pos] = inByte;
        message_pos++;
      }
      //Full message received...
      else
      {
        //Add null character to string
        message[message_pos] = '\0';
      
        //Print the message (or do other things)
        //Serial.print("Re:");
        //Serial.println(message);
        parseString(message);

        //Reset for the next message
        message_pos = 0;
      }
    }
  
    delay(15);                           // waits for the servo to get there
}


void parseString(String s)
{
  int target = 0;
  int x = 1;
  int switch_state;
  int pot_state;
  
  if (s.length() > 1)
  {
    if(s[0] == 'S')
    {
          //target = s[1] - '0';
          for(x = 1; (s[x]>='0') && (s[x] <= '9'); x++)
          {
              target *= 10;
              target += (s[x] - '0'); 
          }
          //Serial.print("Target:");
          //Serial.print(target);
          //Serial.print("\n");
          if(target > 150)
          {
            target = 150;  
          }
          myServo.write(150 - target);

          if(s[x] == 'G')
          {
            if(s[x+1] == '0')
            {
              digitalWrite(LED1_PIN, HIGH);
              digitalWrite(LED2_PIN, LOW);
            }
            else
            {
              digitalWrite(LED1_PIN, LOW);
              digitalWrite(LED2_PIN, HIGH);
            }
          }

          switch_state = digitalRead(SWITCH_PIN);
          pot_state  = analogRead(POT_PIN);            // reads the value of the potentiometer (value between 0 and 1023)
          pot_state = map(pot_state, 0, 1023, 0, 151);

          /* Send the state of the potentiometer and switch as response. */
          Serial.print("P");
          Serial.print(pot_state);
          Serial.print("I");
          Serial.print(switch_state);
          Serial.print("\n");   
    }
  }
}
