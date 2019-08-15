# Settings
## appsettings.json

This file  is to be placed in the applications main directory.

### Section "Pushbullet"
* "Pushbullet API key": String containing the Pushbullet API key created for the app.
* "Channel tag": String containing the channel tag where PB should listen

## Section "Transmission"
* "Host": String containing the hostname of the transmission server

## Environment variables

All Environment Variables pertaining to the importer have to be prefixed with "TPI_".

* TPI_PBAPI: Pusbhullet API key
* TPI_CHANNEL: Pushbullet channel tag
* TPI_THOST: Transmission Hostname