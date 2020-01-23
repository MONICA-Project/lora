# Fraunhofer.Fit.Iot.Lora (Lora)
<!-- Short description of the project. -->

Library that connects to a radio device and recieves lora traffic. This library contains drivers for a RFM96 Chipset (DraginoLora) and a Semtech SX1302 Chipset (iC880A). This readme is meant for describing the software library.

This library is only running on a Raspberry PI with dotnet, because you need the real hardware.

<!-- A teaser figure may be added here. It is best to keep the figure small (<500KB) and in the same repo -->

## Getting Started
<!-- Instruction to make the project up and running. -->

The project documentation is available on the [Wiki](https://github.com/MONICA-Project/lora/wiki).

## Usage
<!-- Deployment/Installation instructions. If this is software library, change this section to "Usage" and give usage examples -->
This software can not run in docker, you need direct access to SPI and GPIOs. Also it did not run under windows of the same reason.


## Development
<!-- Developer instructions. -->
* Versioning: Use [SemVer](http://semver.org/) and tag the repository with full version string. E.g. `v1.0.0`

### Copyright
For the driver of the IC880A Board, please look into the Licence file!
```
 / _____)             _              | |    
( (____  _____ ____ _| |_ _____  ____| |__  
 \____ \| ___ |    (_   _) ___ |/ ___)  _ \ 
 _____) ) ____| | | || |_| ____( (___| | | |
(______/|_____)_|_|_| \__)_____)\____)_| |_|
  (C)2013 Semtech-Cycleo
```

### Prerequisite

If you want to work with this library, please checkout [lora-project](https://github.com/MONICA-Project/lora-project) with all submodules, to get all dependencies.

#### Internal
* BlubbFish.Utils ([Utils](http://git.blubbfish.net/vs_utils/Utils))
* BlubbFish.Utils.IoT.Interfaces ([Iot-Interfaces](http://git.blubbfish.net/vs_utils/Iot-Interfaces))

#### External
* litjson
* Unosquare.RaspberryIO
* Unosquare.WiringPi


### Test

Its complicate to test this library, you can only compile and upload to real hardware to test it.

### Build

Build it with Visual Studio.

## Contributing
Contributions are welcome. 

Please fork, make your changes, and submit a pull request. For major changes, please open an issue first and discuss it with the other authors.

## Affiliation
![MONICA](https://github.com/MONICA-Project/template/raw/master/monica.png)  
This work is supported by the European Commission through the [MONICA H2020 PROJECT](https://www.monica-project.eu) under grant agreement No 732350.
