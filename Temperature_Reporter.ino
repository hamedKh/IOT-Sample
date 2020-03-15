
#include <ESP8266WiFi.h>
#include <WiFiClient.h> 
#include <ESP8266WebServer.h>
#include <ESP8266HTTPClient.h>
#include <ArduinoJson.h>

enum SensorType
{
	Temperature,
	SoilMoisture,
	DistanceCheck
};

// Connection Properties:
const char *ssid = "Hamed S8+";
const char *password = "zpfr9610";

// LEDs
int ConnectedWifi = D1;
int Alert = D2;

// Related PHP path:
String PHP_Address = "http://seganapps.com/RemoteControl/IOT/HomeMarzdaran/SensorValuesReporter/SensorValuesWriter.php";

// Sensor Types & JSON Values:
SensorType sensorType = SoilMoisture;
char *ProjectName = "Home Plants Soil Moisture Monitoring";
char *AxisLable_X = "Time";
char *AxisLable_Y = "Soil Moisture";
char *SensorName = "Soil Moisture Sensor";

// Other:
int Delay = 1000 * 5;
int SoilMoisturePower = D6;
int SoilMoistureSensor = A0;
int Dry_Soil_Criteria = 750;
int Wet_Soil_Criteria = 500;


void setup() {

#pragma region General Initilization

	Serial.begin(115200);
	pinMode(ConnectedWifi, OUTPUT);
	pinMode(Alert, OUTPUT);
	ConnectWifi();

#pragma endregion

}

void loop()
{
	if (WiFi.status() == WL_CONNECTED) {

		digitalWrite(ConnectedWifi, HIGH);

		if (sensorType == Temperature)  // Connect analog signal of LM35 to A0
		{
			int roughLM35 = analogRead(A0);
			float mv = (roughLM35 / 1024.0) * 5000;
			float cel = mv / 10;

			float c1 = 0.1f;
			float c2 = 0.2f;
			float tempFinal = 0.2083f * cel + 15.875f;

			Serial.print("TEMPRATURE = ");

			Serial.print(tempFinal);
			Serial.print("*C");
			Serial.println();

			Send_SensorValue(ProjectName,tempFinal, 0, SensorName, AxisLable_X, AxisLable_Y);
		}
		else if (sensorType == SoilMoisture)  // Connect analog signal of YL-69 to A0
		{
			digitalWrite(SoilMoisturePower, HIGH);	                  // Turn the sensor ON
			delay(1000);							                      // Allow power to settle
			int SoilMoisture = analogRead(SoilMoistureSensor);	      // Read the analog value form sensor
			digitalWrite(SoilMoisturePower, LOW);		              // Turn the sensor OFF

			Serial.print("Soil Moisture = ");

			Serial.print(SoilMoisture);
			Serial.print("   Non Dim Yet!");
			Serial.println();

			Send_SensorValue(ProjectName,SoilMoisture, 0, SensorName, AxisLable_X, AxisLable_Y);
		}

		delay(Delay);
	}
	else
	{
		digitalWrite(ConnectedWifi, LOW);
		delay(500);
		digitalWrite(ConnectedWifi, HIGH);
		delay(500);
		digitalWrite(ConnectedWifi, LOW);
	}
}

void Send_SensorValue(char* projectName, float SensorValue, int SensorIndex, char* SensorName, char* AxisLableX, char* AxisLableY)
{
	Serial.println("Starting Send Data");
	HTTPClient http;
	String dataline = PHP_Address;

	dataline += "?ProjectName=" + (String)projectName;
	dataline += "&SensorValue=" + (String)SensorValue;
	dataline += "&SensorIndex=" + (String)SensorIndex;
	//dataline += "&SensorIndex=" + (String)SensorName;
	dataline += "&AxisLable_X=" + (String)AxisLableX;
	dataline += "&AxisLable_Y=" + (String)AxisLableY;

	bool httpResult = http.begin(dataline);
	if (!httpResult)
	{
		Serial.println("Invalid HTTP request:");
		Serial.println(dataline);
		digitalWrite(Alert, HIGH);
	}
	else
	{
		int httpCode = http.GET();
		if (httpCode > 0)
		{ // Request has been made
			Serial.printf("HTTP status: %d Message: ", httpCode);
			String payload = http.getString();
			Serial.println(payload);
			Serial.println("---------------- request is:");
			Serial.println(dataline);
			Serial.println("---------------- end request:");
			digitalWrite(Alert, HIGH);
			delay(100);
			digitalWrite(Alert, LOW);
		}
		else
		{ // Request could not be made
			Serial.printf("HTTP request failed. Error: %s\r\n", http.errorToString(httpCode).c_str());
			digitalWrite(Alert, HIGH);
		}
	}
	http.end();

	Serial.println("Ending Send Data");
}

void ConnectWifi()
{
	delay(1000);
	Serial.begin(115200);
	WiFi.mode(WIFI_OFF);        //Prevents reconnection issue (taking too long to connect)
	delay(1000);
	WiFi.mode(WIFI_STA);        //This line hides the viewing of ESP as wifi hotspot

	WiFi.begin(ssid, password);     //Connect to your WiFi router
	Serial.println("");

	Serial.print("Connecting");
	// Wait for connection
	while (WiFi.status() != WL_CONNECTED) {
		delay(500);
		Serial.print(".");
	}

	//If connection successful show IP address in serial monitor
	Serial.println("");
	Serial.print("Connected to ");
	Serial.println(ssid);
	Serial.print("IP address: ");
	Serial.println(WiFi.localIP());  //IP address assigned to your ESP

	digitalWrite(ConnectedWifi, HIGH);
}