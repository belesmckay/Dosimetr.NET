# Dosimeter.NET

Simple program for geting values from FS-5000 and sending them to MQTT. I use them for Home assistant

## Description
This app read data from serial port of FS-5000 dosimeter and send them via MQTT to Home assistant. 
Dont worry this program is compactible with Home assistant discovery. In future i will add Dose reset button. 
Be sure you have version of dosimeter with J321 tube.

## Getting Started

### Dependencies

* .NET framework 
* MQTTnet
### Installing

* If you wanna run this program on you current computer.

```
make
```
* It do all work by self
* If you wanna make it run on any ARM based computer, you can too, but in this time you have to do it your self. I use Raspberry pi zero 2W for reading and sending data. 
* Default archytecture is set to linux-arm64. If you have diferent archytecture you have to change it in Makefile

```
make publishARM
```
* Copy package to your device, extract and run. Package should contain all nesesary libraries
### Config file structure

```
MQTT_USER: mqttuser
MQTT_PASSWORD: mqttpassword
MQTT_HOST: localhost
MQTT_PORT:1883
USB_DEVICE: /dev/ttyUSB0

```

### Executing program

* How to run the program
```
./Dosimeter.NET -c config.cfx
```

## Help

Check if you have config file. Program notice you about missing config file.
Check if you have all values set as described before in config file structure.
Make sure your user have rights to acces to you serial port (dialout, uucp groups)

## Authors

Contributors names and contact info

ex. Beles Mckay
email: belesmckay@gmail.com

## Version History

* 0.1
    * Initial Release
* 0.2
    * First working version
## License

This project is licensed under the MIT License - see the LICENSE.md file for details

## Acknowledgments

Inspiration, code snippets, etc.
* [Serial interface to Bosean FS-5000 radiation detector](https://gist.github.com/brookst/bdbede3a8d40eb8940a5b53e7ca1f6ce)
