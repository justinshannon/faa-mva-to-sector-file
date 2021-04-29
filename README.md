# Convert FAA MVA and MIA XML to Sector File Format
This tool will convert FAA MVA and MIA charts to the sector file format for use on the VATSIM Network.

The XML files can be downloaded from the FAA's website at https://www.faa.gov/air_traffic/flight_info/aeronav/digital_products/mva_mia/

**Do not use for real world flight navigation.**

## Requirements
* .NET Core 3.1 or newer

## How to use

1. Download the XML file from the FAA's website (link above).

2. Execute the following in a command prompt:
`FaaMvaToSectorFile.exe full\path\to\mva.xml MAP_NAME`
You can optionally provide a third argument to specify the color key
`FaaMvaToSectorFile.exe full\path\to\mva.xml MAP_NAME COLOR_KEY`

3. The tool will parse the XML file and save the results in a `.txt` file in the same folder as `FaaMvaToSectorFile.exe`

## Download
A compiled exe binary of this tool can be downloaded from the Releases section of this repository. Alternatively, you can build the project yourself. 

## License
This project is licensed under the [GPLv3 License](LICENSE).

## Acknowledgements
* [Polylabel](https://github.com/mapbox/polylabel) and [Polylabel-CSharp](https://github.com/qodbtn41/polylabel-csharp)
* [High Speed Priority Queue for C#](https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp)
* [Hershey Vector Font](http://paulbourke.net/dataformats/hershey/)
