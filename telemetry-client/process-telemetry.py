#https://github.com/ttu/ruuvitag-sensor/blob/master/install_guide_pi.md

from ruuvitag_sensor.ruuvi import RuuviTagSensor
import time, json, requests, urllib, uuid, os
from requests.structures import CaseInsensitiveDict
from datetime import datetime

sensor_map = {
    "D0:B2:9E:2F:C7:28" : "Study"
    }

sensors = [
    "D0:B2:9E:2F:C7:28"
]

ingest_url = os.environ.get('INGESTURL')

def process_data(data):
    # Every minute, get data and send to server   
    sensor_name = sensor_map[data[0]]
    sensor_values = json.loads(json.dumps(data[1], indent=4))
    sensor_values.update({"sensorname":sensor_name})    
    sensor_values.update({"location":location})

    headers = CaseInsensitiveDict()
    headers["Accept"] = "application/json"
    headers["Content-Type"] = "application/json"

    now = datetime.now()
    current_time = now.strftime("%d-%m-%Y %H:%M:%S")

    resp = requests.post(ingest_url, headers=headers, data=json.dumps(sensor_values))
    
    print(current_time + ": Last status: " + str(resp.status_code))

    time.sleep(sleepfor())

def sleepfor():
    #We want to ensure that data is sent at least once every minute per sensor
    number_sensors = len(sensors)
    sleep_for = 60/number_sensors

    return sleep_for

def whereami():
    location = urllib.request.urlopen("http://ipwhois.app/json/")
    locationobj = json.load(location)    
    locationobj.pop('success', None)
    return locationobj

if __name__ == '__main__':
    global location
    location = whereami()
    RuuviTagSensor.get_datas(process_data, sensors)    