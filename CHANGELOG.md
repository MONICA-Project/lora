# Changelog
## 2.0.0 - The total rewrite

### New Features
* Dragino board now support Sending data while listen on receiving data
* Now netcore is supported

### Bugfixes

### Changes
* Rewrite LoraController and the rest so that it did not parse any data
* Rewrite Dragino driver complete
* Remove all parsing data stuff, that must now be done in the programm that call the lib
* Split Ic800A into different files
* Fix Thread abort in netcore
* Using new WiringPi lib

## 1.8.4
### New Features
### Bugfixes
* #1 Fixing a bug related to last version

### Changes
* Try to get rid of ppl not sync error

## 1.8.3
### New Features
* Make using threads on event occours

### Bugfixes
### Changes
* Refactoring

## 1.8.2
### New Features
### Bugfixes
* Create also an event for sending normal loradata when update panic

### Changes

## 1.8.1
### New Features
* Add Hostname to MQTT, so you can see from witch device the data is recieved

### Bugfixes
### Changes

## 1.8.0
### New Features
* Add field that indicates when the last gps position was recieved, change all times to UTC

### Bugfixes
### Changes

## 1.7.0
### New Features
* Add Parsing for new Statusformat and Panic Packet

### Bugfixes
### Changes


## 1.6.1
### New Features
### Bugfixes
### Changes
* Update to local librarys

## 1.6.0
### New Features
### Bugfixes
* Fixing binary data transmission

### Changes
* Change internal data handling from String to Byte[]

## 1.5.0
### New Features
* Add support for IC880A board

### Bugfixes
### Changes
* Rename LoraConnector to DraginoLora

## 1.4.2
### New Features
### Bugfixes
* Fixing parsing bug with linebreaks

### Changes
* Adding test for LoraBinary

## 1.4.0
### New Features
* Implement Height in Lora

### Bugfixes
### Changes
* Restructure and remove old code
* Status event and fileds added and refactoring

## 1.1.0
### New Features
### Bugfixes
* fix the sending twise issue

### Changes
* Now awaiing Battery as Double

## 1.0.0.0
### New Features
* First Version of LoraConnector

### Bugfixes
### Changes