# AtHome
A DIY home automation and security project

## Security PI Setup
The security PI is used as a NAS/FTP for IP cameras to store video and a application server for
the AtHome app to communicate with. All security related devices are setup on an isolated LAN.

- Change password
- Enable SSH
- Change host name
- enable Wifi to connect to primary network
- setup static IP for isolated LAN
- update apt-get - sudo apt-get update
- Create user and user groups
- Configure storage
- Setup FTP server
- Install dotnet SDK
- Install git - sudo apt-get install git
- Install ffmpeg - sudo apt-get install ffmpeg
- Install ngrok


## Setup static IP for isolated LAN
Add the follow to dhcpcd.conf - sudo nano /etc/dhcpcd.conf
``` txt
# Static LAN for security Devices
interface eth0
static ip_address=192.168.55.100/24
```


## Create user and user groups
Here we will create a user group called hometeam and a user called homer
``` sh
sudo groupadd hometeam
sudo useradd homer -g hometeam -s /sbin/nologin -d /dev/null
sudo adduser pi hometeam
```

### Configure Storage
Mount storage used to store camera recordings. 


#### Mount Disk
https://askubuntu.com/questions/154180/how-to-mount-a-new-drive-on-startup

If you have already created a disk partition and file system skip to step 9

1. Run sudo fdisk /dev/sdb
2. Press O and press Enter (creates a new table)
3. Press N and press Enter (creates a new partition)
4. Press P and press Enter (makes a primary partition)
5. Then press 1 and press Enter (creates it as the 1st partition)
6. Finally, press W (this will write any changes to disk).
7. Exit fdisk tool
8. sudo mkfs.ext4 /dev/sda1
9. Add the text below to fstab - sudo nano /etc/fstab
``` txt
#device        mountpoint             fstype    options  dump   fsck
/dev/sda1    /media/secure-data/       ext4    defaults    0    1
```
10. sudo reboot

#### Update Data Directory
``` sh
sudo chown -R homer:hometeam /media/secure-data
sudo chmod -R 774 /media/secure-data
sudo rm -r /media/secure-data/lost+found
mkdir /media/secure-data/live
mkdir /media/secure-data/archive
```


### FTP Server Setup
https://www.raspberrypi.org/documentation/remote-access/ftp.md

``` sh
sudo apt install pure-ftpd
# -u and -g values are ids for the homer user and hometeam group
sudo pure-pw useradd homer -u 1001 -g 1001 -d /media/secure-data/live -m
sudo pure-pw mkdb
sudo ln -s /etc/pure-ftpd/conf/PureDB /etc/pure-ftpd/auth/60puredb
sudo service pure-ftpd restart
```


### Insall dotnet SDK

Manual dotnet install
``` sh
cd ~
wget https://download.visualstudio.microsoft.com/download/pr/dbf4ea18-70bf-4b0f-ae9c-65c8c88bcadd/115e84fb95170ddeeaf9bdb9222c964d/dotnet-sdk-3.1.301-linux-arm.tar.gz

mkdir /usr/share/dotnet
sudo chown -R pi:hometeam /usr/share/dotnet
tar zxf dotnet-sdk-3.1.301-linux-arm.tar.gz -C /usr/share/dotnet
```

Add dotnet to PATH - nano ~/.bashrc
``` txt
export DOTNET_ROOT=/usr/share/dotnet
export PATH=$PATH:/usr/share/dotnet
```


### Install NGrok

``` sh
wget -O ngrok.zip https://bin.equinox.io/c/4VmDzA7iaHb/ngrok-stable-linux-arm.zip
unzip ngrok.zip
rm ngrok.zip
sudo mv ngrok /usr/local/bin
# Get token from https://dashboard.ngrok.com/auth/your-authtoken
ngrok authtoken <YOUR_AUTH_TOKEN>
```