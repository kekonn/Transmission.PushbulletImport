# Settings
## appsettings.json

This file  is to be placed in the applications main directory.

### Section "Pushbullet"
* "Pushbullet API key": String containing the Pushbullet API key created for the app.
* "Target Device": String containing the device id that PB should target when sending messages

## Section "Transmission"
* "Host": String containing the hostname of the transmission server

## Environment variables

All Environment Variables pertaining to the importer have to be prefixed with "TPI_".

* TPI_PBAPI: Pusbhullet API key
* TPI_PBDEVICE: Pushbullet target device
* TPI_THOST: Transmission Hostname