# Fraunhofer.Fit.Iot.Lora (Lora)
<!-- Short description of the project. -->

Library that connects to a radio device and recieves lora traffic. This library contains drivers for a RFM96 Chipset (DraginoLora) and a Semtech SX1302 Chipset (iC880A). This readme is meant for describing the software library.

This library is only running on a Raspberry PI with Mono, because you need the real hardware.

<!-- A teaser figure may be added here. It is best to keep the figure small (<500KB) and in the same repo -->

## Getting Started
<!-- Instruction to make the project up and running. -->

The project documentation is available on the [Wiki](https://github.com/MONICA-Project/lora/wiki).

## Usage
<!-- Deployment/Installation instructions. If this is software library, change this section to "Usage" and give usage examples -->
This software can not run in docker, you need direct access to SPI and GPIOs. Also it did not run under windows of the same reason.

If you want to use DraginoLora:
```c#
using Fraunhofer.Fit.Iot.Lora;
using Fraunhofer.Fit.Iot.Lora.Events;

class Program {
    static void Main(String[] args) => new Program(args);
    public Program(String[] args) {
        Dictionary<String, String> settings = new Dictionary<String, String>() {
            {"type", "Draginolora"},
            {"pin_sspin", "Pin06"},
            {"pin_dio0", "Pin07"},
            {"pin_rst", "Pin00"},
            {"frequency", "868100000"},
            {"spreadingfactor", "8"},
            {"signalbandwith", "125000"},
            {"codingrate", "6"}
        };
        LoraController lora = new LoraController(settings);
        lora.DataUpdate += this.LoraDataUpdate;
        lora.StatusUpdate += this.LoraStatusUpdate;
        lora.PanicUpdate += this.LoraPanicUpdate;
        while(true) {
            System.Threading.Thread.Sleep(1);
        }
        lora.Dispose();
    }

    private void LoraPanicUpdate(Object sender, PanicUpdateEvent e) => Console.WriteLine("-> Lora-Panic: " + e.ToString());
    private void LoraStatusUpdate(Object sender, StatusUpdateEvent e) => Console.WriteLine("-> Lora-Status: " + e.ToString());
    private void LoraDataUpdate(Object sender, DataUpdateEvent e) => Console.WriteLine("-> Lora-Data: " + e.ToString());
}
```

If you want to use iC880A:
```c#
using Fraunhofer.Fit.Iot.Lora;
using Fraunhofer.Fit.Iot.Lora.Events;

class Program {
    static void Main(String[] args) => new Program(args);
    public Program(String[] args) {
        Dictionary<String, String> settings = new Dictionary<String, String>() {
            {"type", "Ic880alora"},
            {"pin_sspin", "Pin10"},
            {"pin_rst", "Pin21"},
            {"spichan", "Channel0"},
            {"frequency0", "867125000"},
            {"frequency1", "868125000"},
            {"interface0enable", "true"},
            {"interface0chain", "0"},
            {"interface0frequency", "-375000"},
            {"interface1enable", "true"},
            {"interface1chain", "0"},
            {"interface1frequency", "-125000"},
            {"interface2enable", "true"},
            {"interface2chain", "0"},
            {"interface2frequency", "125000"},
            {"interface3enable", "true"},
            {"interface3chain", "0"},
            {"interface3frequency", "375000"},
            {"interface4enable", "true"},
            {"interface4chain", "1"},
            {"interface4frequency", "-375000"},
            {"interface5enable", "true"},
            {"interface5chain", "1"},
            {"interface5frequency", "-125000"},
            {"interface6enable", "true"},
            {"interface6chain", "1"},
            {"interface6frequency", "125000"},
            {"interface7enable", "true"},
            {"interface7chain", "1"},
            {"interface7frequency", "375000"},
            {"interface8enable", "false"},
            {"interface8chain", "1"},
            {"interface8frequency", "-200000"},
            {"lorabandwith", "250000"},
            {"loraspreadingfactor", "7"},
            {"interface9enable", "false"},
            {"interface9chain", "1"},
            {"interface9frequency", "300000"},
            {"fskbandwith", "125000"},
            {"fskdatarate", "50000"},
        };
        LoraController lora = new LoraController(settings);
        lora.DataUpdate += this.LoraDataUpdate;
        lora.StatusUpdate += this.LoraStatusUpdate;
        lora.PanicUpdate += this.LoraPanicUpdate;
        while(true) {
            System.Threading.Thread.Sleep(1);
        }
        lora.Dispose();
    }

    private void LoraPanicUpdate(Object sender, PanicUpdateEvent e) => Console.WriteLine("-> Lora-Panic: " + e.ToString());
    private void LoraStatusUpdate(Object sender, StatusUpdateEvent e) => Console.WriteLine("-> Lora-Status: " + e.ToString());
    private void LoraDataUpdate(Object sender, DataUpdateEvent e) => Console.WriteLine("-> Lora-Data: " + e.ToString());
}
```

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
* BlubbFish.Utils.IoT.Bots ([Bot-Utils](http://git.blubbfish.net/vs_utils/Bot-Utils))
* BlubbFish.Utils.IoT.Interfaces ([Iot-Interfaces](http://git.blubbfish.net/vs_utils/Iot-Interfaces))

#### External
* litjson
* Unosquare.RaspberryIO
* Unosquare.Swan
* Unosquare.Swan.Lite


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
