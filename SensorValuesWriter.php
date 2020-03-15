<!DOCTYPE html>
<html>
<body>

<?php
	
	date_default_timezone_set('Asia/Tehran');
	$SensorValues_Buffer = file_get_contents("SensorValues_Buffer.json");
	$SensorsData = json_decode($SensorValues_Buffer,true);
	
	$SensorsData['SensorsProjectName'] =$_GET['ProjectName'];
	$SensorsData['Graph_X_Lable'] =$_GET['AxisLable_X'];
	$SensorsData['Graph_Y_Lable'] =$_GET['AxisLable_Y'];
	
 	$SensorsData['sensorsList'][(int)$_GET['SensorIndex']]["Rows"] ++;
	$currentSensorRows = $SensorsData['sensorsList'][(int)$_GET['SensorIndex']]["Rows"];
	$CurrentSensorReport = array("row"=> $currentSensorRows ,"SensorValue"=> $_GET['SensorValue'] ,"time"=> date('Y-m-d H:i:s') );
	$SensorsData['sensorsList'][(int)$_GET['SensorIndex']]['sensorDataList'][] = $CurrentSensorReport;
	
	echo $currentSensorRows . "\n".$CurrentSensorReport;
	
	$fh = fopen("SensorValues_Buffer.json", 'w')
    or die("Error opening output file");
	fwrite($fh, json_encode($SensorsData,JSON_UNESCAPED_UNICODE));
	fclose($fh);
	
?>

</body>
</html>
